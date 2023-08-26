using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SafeRouting.Generator;

internal static class Parser
{
  public static GeneratorOptions GetOptions(AnalyzerConfigOptionsProvider optionsProvider)
  {
    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    var generatedAccessModifier = GetGeneratedAccessModifierOption(optionsProvider.GlobalOptions, diagnostics);
    var generatedNamespace = GetGeneratedNamespaceOption(optionsProvider.GlobalOptions, diagnostics);
    var generatedParameterCase = GetGeneratedParameterCaseOption(optionsProvider.GlobalOptions, diagnostics);

    return new GeneratorOptions(generatedAccessModifier, generatedNamespace, generatedParameterCase, diagnostics.ToImmutable());
  }

  public static bool IsCandidateNode(SyntaxNode node)
    => node is TypeDeclarationSyntax { TypeParameterList: null, Parent: not TypeDeclarationSyntax } typeDeclarationSyntax
      and not InterfaceDeclarationSyntax
      && (typeDeclarationSyntax.AttributeLists.Count > 0 || typeDeclarationSyntax.BaseList?.Types.Count > 0)
      && typeDeclarationSyntax.Modifiers.Any(t => t.IsKind(SyntaxKind.PublicKeyword))
      && !typeDeclarationSyntax.Modifiers.Any(t => t.IsKind(SyntaxKind.StaticKeyword) || t.IsKind(SyntaxKind.AbstractKeyword));

  public static CandidateClassInfo? TransformCandidateClassNode(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;

    if (context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax, cancellationToken) is not { } typeSymbol)
    {
      return null;
    }

    var (isController, isPage) = IsControllerOrPage(typeSymbol);

    if (!isController && !isPage)
    {
      return null;
    }

    return new CandidateClassInfo(typeDeclarationSyntax, typeSymbol, context.SemanticModel, isController, isPage);
  }

  public static (ControllerInfo? ControllerInfo, ImmutableArray<Diagnostic> Diagnostics) GetControllerInfo(CandidateClassInfo classInfo, CancellationToken cancellationToken)
  {
    if (!classInfo.IsController)
    {
      return (null, ImmutableArray<Diagnostic>.Empty);
    }

    var typeSymbol = classInfo.TypeSymbol;

    // https://github.com/dotnet/aspnetcore/blob/2862028573708e5684bf17526c43127e178525d4/src/Mvc/Mvc.Core/src/ApplicationModels/DefaultApplicationModelProvider.cs#L167
    var controllerName = typeSymbol.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
      ? typeSymbol.Name[..^"Controller".Length]
      : typeSymbol.Name;

    if (controllerName.Length == 0)
    {
      return (null, ImmutableArray<Diagnostic>.Empty);
    }

    var generatorName = controllerName;

    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    GetControllerAttributes(typeSymbol, diagnostics, out var areaName, out var defaultBindingSource, out var defaultBindingLevel, ref generatorName, cancellationToken);

    GetControllerMembers(classInfo, typeSymbol, defaultBindingSource, defaultBindingLevel, diagnostics, out var properties, out var methods, cancellationToken);

    if (methods.Length == 0)
    {
      return (null, diagnostics.ToImmutable());
    }

    return (new ControllerInfo(controllerName, generatorName, areaName, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), classInfo.TypeDeclarationSyntax, properties, methods), diagnostics.ToImmutable());
  }

  public static (ImmutableArray<ControllerInfo> Controllers, ImmutableArray<Diagnostic> Diagnostics) GetUniqueControllers(ImmutableArray<ControllerInfo> controllers)
  {
    var controllerNames = new HashSet<string>(StringComparer.Ordinal);
    var emitControllers = ImmutableArray.CreateBuilder<ControllerInfo>(controllers.Length);
    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    foreach (var controller in controllers)
    {
      if (!controllerNames.Add($"{controller.Area} {controller.Name}"))
      {
        diagnostics.Add(Diagnostics.CreateConflictingControllerDiagnostic(controller.Name, controller.TypeDeclarationSyntax.GetLocation()));
        continue;
      }

      emitControllers.Add(controller);
    }

    return (emitControllers.ToImmutable(), diagnostics.ToImmutable());
  }

  public static (PageInfo? PageInfo, ImmutableArray<Diagnostic> Diagnostics) GetPageInfo(CandidateClassInfo classInfo, CancellationToken cancellationToken)
  {
    if (!classInfo.IsPage)
    {
      return (null, ImmutableArray<Diagnostic>.Empty);
    }

    var typeSymbol = classInfo.TypeSymbol;
    var typeDeclarationSyntax = classInfo.TypeDeclarationSyntax;

    var filePath = typeDeclarationSyntax.SyntaxTree.FilePath;
    if (string.IsNullOrEmpty(filePath))
    {
      return (null, ImmutableArray<Diagnostic>.Empty);
    }

    var fileInfo = new FileInfo(filePath);
    if (!fileInfo.Name.EndsWith(".cshtml.cs", StringComparison.InvariantCultureIgnoreCase))
    {
      return (null, ImmutableArray<Diagnostic>.Empty);
    }

    var directory = fileInfo.Directory;
    var pathSegments = ImmutableArray.CreateBuilder<string>();

    for (; directory != null && !string.Equals(directory.Name, "Pages", StringComparison.InvariantCultureIgnoreCase); directory = directory.Parent)
    {
      pathSegments.Insert(0, directory.Name);
    }

    if (directory is null)
    {
      return (null, ImmutableArray<Diagnostic>.Empty);
    }

    var pageNamespace = string.Join("_", pathSegments);
    var areaName = default(string);
    if (directory.Parent?.Parent?.Name.Equals("Areas", StringComparison.InvariantCultureIgnoreCase) ?? false)
    {
      areaName = directory.Parent.Name;
    }

    var pageName = fileInfo.Name[..^".cshtml.cs".Length];
    pathSegments.Add(pageName);
    var pagePath = "/" + string.Join("/", pathSegments);

    var generatorName = pageName;

    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    GetPageAttributes(typeSymbol, diagnostics, out var defaultBindingSource, ref generatorName, cancellationToken);

    GetPageMembers(classInfo, typeSymbol, defaultBindingSource, diagnostics, out var properties, out var methods, cancellationToken);

    if (methods.Length == 0)
    {
      return (null, diagnostics.ToImmutable());
    }

    return (new PageInfo(pagePath, generatorName, areaName, pageNamespace, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), typeDeclarationSyntax, properties, methods), diagnostics.ToImmutable());
  }

  public static (ImmutableArray<PageInfo> Pages, ImmutableArray<Diagnostic> Diagnostics) GetUniquePages(ImmutableArray<PageInfo> pages)
  {
    var pageIdentifiers = new HashSet<string>(StringComparer.Ordinal);
    var emitPages = ImmutableArray.CreateBuilder<PageInfo>(pages.Length);
    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    foreach (var page in pages)
    {
      if (!pageIdentifiers.Add($"{page.Area}_{page.PageNamespace}_{page.Name}"))
      {
        diagnostics.Add(Diagnostics.CreateConflictingPageClassDiagnostic(page.FullyQualifiedTypeName, page.TypeDeclarationSyntax.GetLocation()));
        continue;
      }

      emitPages.Add(page);
    }

    return (emitPages.ToImmutable(), diagnostics.ToImmutable());
  }

  private static string GetGeneratedAccessModifierOption(AnalyzerConfigOptions options, ImmutableArray<Diagnostic>.Builder diagnostics)
  {
    var generatedAccessModifier = GeneratorSupport.DefaultGeneratedAccessModifier;

    if (!options.TryGetValue(GeneratorSupport.GeneratedAccessModifierOption, out var generatedAccessModifierValue))
    {
      return generatedAccessModifier;
    }

    if (new[] { "public", "internal" }.Any(x => string.Equals(x, generatedAccessModifierValue, StringComparison.Ordinal)))
    {
      generatedAccessModifier = generatedAccessModifierValue;
    }
    else
    {
      diagnostics.Add(Diagnostics.CreateInvalidOptionDiagnostic(GeneratorSupport.GeneratedAccessModifierOption, $"'{generatedAccessModifierValue}' is not a supported access modifier, must be public or internal."));
    }

    return generatedAccessModifier;
  }
  private static string GetGeneratedNamespaceOption(AnalyzerConfigOptions options, ImmutableArray<Diagnostic>.Builder diagnostics)
  {
    var generatedNamespace = GeneratorSupport.DefaultGeneratedRootNamespace;

    if (!options.TryGetValue(GeneratorSupport.GeneratedNamespaceOption, out var generatedNamespaceValue))
    {
      return generatedNamespace;
    }

    if (generatedNamespaceValue.Split('.').All(SyntaxFacts.IsValidIdentifier))
    {
      generatedNamespace = generatedNamespaceValue;
    }
    else
    {
      diagnostics.Add(Diagnostics.CreateInvalidOptionDiagnostic(GeneratorSupport.GeneratedNamespaceOption, $"'{generatedNamespaceValue}' is not a valid namespace identifier."));
    }

    return generatedNamespace;
  }
  private static IdentifierCase GetGeneratedParameterCaseOption(AnalyzerConfigOptions options, ImmutableArray<Diagnostic>.Builder diagnostics)
  {
    var generatedParameterCase = GeneratorSupport.DefaultGeneratedParameterCase;

    if (!options.TryGetValue(GeneratorSupport.GeneratedParameterCaseOption, out var generatedParameterCaseValue))
    {
      return generatedParameterCase;
    }

    if (string.Equals(generatedParameterCaseValue, "standard", StringComparison.Ordinal))
    {
      return IdentifierCase.Standard;
    }
    else if (string.Equals(generatedParameterCaseValue, "pascal", StringComparison.Ordinal))
    {
      return IdentifierCase.Pascal;
    }
    else
    {
      diagnostics.Add(Diagnostics.CreateInvalidOptionDiagnostic(GeneratorSupport.GeneratedParameterCaseOption, $"'{generatedParameterCaseValue}' is not a supported parameter case, must be standard or pascal."));
    }

    return generatedParameterCase;
  }
  private static void GetControllerAttributes(INamedTypeSymbol typeSymbol, ImmutableArray<Diagnostic>.Builder diagnostics, out string? areaName, out MvcBindingSourceInfo? defaultBindingSource, out INamedTypeSymbol? defaultBindingLevel, ref string generatorName, CancellationToken cancellationToken)
  {
    areaName = null;
    defaultBindingSource = null;
    defaultBindingLevel = null;

    foreach (var symbol in typeSymbol.EnumerateSelfAndBaseTypes())
    {
      foreach (var attribute in symbol.GetAttributes())
      {
        switch (attribute.AttributeClass?.ToDisplayString())
        {
          case AspNetClassNames.AreaAttribute:
            if (areaName is null)
            {
              attribute.TryGetOptionalStringArgumentAttribute(out areaName);
            }
            break;

          case AspNetClassNames.BindPropertiesAttribute:
            if (defaultBindingSource is null)
            {
              defaultBindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Custom);
              defaultBindingLevel = symbol;
            }
            break;

          case GeneratorClassNames.RouteGeneratorNameAttribute:
            if (!ReferenceEquals(symbol, typeSymbol) || !attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
            {
              break;
            }

            if (SyntaxFacts.IsValidIdentifier(generatorNameValue))
            {
              generatorName = CSharpSupport.CamelToPascalCase(generatorNameValue);
            }
            else
            {
              diagnostics.Add(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()));
            }
            break;
        }
      }
    }
  }
  private static void GetControllerMembers(CandidateClassInfo classInfo, INamedTypeSymbol typeSymbol, MvcBindingSourceInfo? defaultBindingSource, INamedTypeSymbol? defaultBindingLevel, ImmutableArray<Diagnostic>.Builder diagnostics, out ImmutableArray<MvcPropertyInfo> properties, out ImmutableArray<ControllerMethodInfo> methods, CancellationToken cancellationToken)
  {
    var propertiesBuilder = ImmutableArray.CreateBuilder<MvcPropertyInfo>();
    var methodSymbols = ImmutableArray.CreateBuilder<IMethodSymbol>();
    var methodIdentifiers = new HashSet<string>(StringComparer.Ordinal);
    var methodsBuilder = ImmutableArray.CreateBuilder<ControllerMethodInfo>();
    // Track all unique member names to avoid including the same member from base classes
    var accessedMembers = new HashSet<string>(StringComparer.Ordinal);
    var urlAffectedIdentifiers = new HashSet<string>(StringComparer.Ordinal);
    var methodNames = new HashSet<string>(StringComparer.Ordinal);

    foreach (var symbol in typeSymbol.EnumerateSelfAndBaseTypes())
    {
      var typeName = symbol.ToDisplayString();
      if (string.Equals(typeName, AspNetClassNames.Controller, StringComparison.Ordinal)
        || string.Equals(typeName, AspNetClassNames.ControllerBase, StringComparison.Ordinal)
        || string.Equals(typeName, "object", StringComparison.Ordinal))
      {
        break;
      }

      foreach (var member in symbol.GetMembers())
      {
        if (member.IsAbstract || member.IsImplicitlyDeclared || member.IsStatic || member.DeclaredAccessibility != Accessibility.Public
          || !accessedMembers.Add(member.ToDisplayString(UniqueClassMemberSymbolDisplayFormat)))
        {
          continue;
        }

        if (member is IPropertySymbol propertySymbol)
        {
          if (GetMvcPropertyInfo(propertySymbol, classInfo.SemanticModel, defaultBindingSource, diagnostics, cancellationToken) is { } propertyInfo)
          {
            propertiesBuilder.Add(propertyInfo);
          }
        }
        else if (member is IMethodSymbol methodSymbol)
        {
          methodSymbols.Add(methodSymbol);
        }
      }

      if (ReferenceEquals(symbol, defaultBindingLevel))
      {
        // Inherited members don't receive default binding
        defaultBindingSource = null;
      }
    }

    properties = propertiesBuilder.ToImmutable();

    foreach (var methodSymbol in methodSymbols)
    {
      if (GetControllerMethodInfo(methodSymbol, classInfo.SemanticModel, diagnostics, cancellationToken) is { } method)
      {
        var (isModified, parameters) = CombineBoundProperties(method.Parameters, properties);
        if (isModified)
        {
          method = method with { Parameters = parameters };
        }

        AddUniqueControllerMethodInfo(classInfo, method, methodSymbol, methodIdentifiers, methodsBuilder, urlAffectedIdentifiers, methodNames, diagnostics, cancellationToken);
      }
    }

    methods = methodsBuilder.ToImmutableArray();
  }
  private static bool TryGetControllerMethodAttributes(IMethodSymbol methodSymbol, ImmutableArray<Diagnostic>.Builder diagnostics, out string? areaName, ref string name, ref string actionName, CancellationToken cancellationToken)
  {
    areaName = null;

    foreach (var attribute in methodSymbol.GetAttributes())
    {
      switch (attribute.AttributeClass?.ToDisplayString())
      {
        case AspNetClassNames.ActionNameAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var actionNameValue))
          {
            actionName = actionNameValue;
          }
          break;

        case AspNetClassNames.AreaAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var areaNameValue))
          {
            areaName = areaNameValue;
          }
          break;

        case AspNetClassNames.NonActionAttribute:
        case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
          return false;

        case GeneratorClassNames.RouteGeneratorNameAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
          {
            if (SyntaxFacts.IsValidIdentifier(generatorNameValue))
            {
              name = CSharpSupport.CamelToPascalCase(generatorNameValue);
            }
            else
            {
              diagnostics.Add(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()));
            }
          }
          break;
      }
    }

    return true;
  }
  private static ControllerMethodInfo? GetControllerMethodInfo(IMethodSymbol methodSymbol, SemanticModel semanticModel, ImmutableArray<Diagnostic>.Builder diagnostics, CancellationToken cancellationToken)
  {
    if (methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.IsGenericMethod)
    {
      return null;
    }

    var name = methodSymbol.Name.EndsWith("Async", StringComparison.Ordinal)
      ? methodSymbol.Name[..^"Async".Length]
      : methodSymbol.Name;

    var actionName = name;

    if (!TryGetControllerMethodAttributes(methodSymbol, diagnostics, out var areaName, ref name, ref actionName, cancellationToken)
      || name.Length == 0)
    {
      return null;
    }

    var parameters = ImmutableArray.CreateBuilder<MvcMethodParameterInfo>();

    foreach (var parameterSymbol in methodSymbol.Parameters)
    {
      // Methods with in/out/ref parameters are ignored by ASP.NET
      if (parameterSymbol.RefKind != RefKind.None)
      {
        return null;
      }

      if (GetMvcMethodParameterInfo(parameterSymbol, semanticModel, diagnostics, cancellationToken) is { } parameter)
      {
        parameters.Add(parameter);
      }
    }

    var escapedName = CSharpSupport.EscapeIdentifier(name);
    var fullyQualifiedMethodDeclaration = methodSymbol.ToDisplayString(UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat);

    return new ControllerMethodInfo(name, escapedName, name, actionName, areaName, fullyQualifiedMethodDeclaration, parameters.ToImmutable());
  }
  private static void AddUniqueControllerMethodInfo(CandidateClassInfo classInfo, ControllerMethodInfo method, IMethodSymbol methodSymbol, HashSet<string> methodIdentifiers, ImmutableArray<ControllerMethodInfo>.Builder methods, HashSet<string> urlAffectedIdentifiers, HashSet<string> methodNames, ImmutableArray<Diagnostic>.Builder diagnostics, CancellationToken cancellationToken)
  {
    var identifier = $"{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Type.FullyQualifiedName))})";

    // Collapse multiple methods with the same parameters
    if (methodIdentifiers.Contains(identifier))
    {
      return;
    }

    var urlAffectedIdentifier = $"{method.Name}({string.Join(", ", method.Parameters.Where(x => x.AffectsUrl()).Select(x => x.Type.FullyQualifiedName))})";

    if (!urlAffectedIdentifiers.Add(urlAffectedIdentifier))
    {
      diagnostics.Add(Diagnostics.CreateConflictingMethodsDiagnostic(classInfo.TypeSymbol.Name, urlAffectedIdentifier, methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken).GetLocation()));
      return;
    }

    // Methods with the same name but different parameters need to be given unique identifiers for the code generated classes
    if (!methodNames.Add(method.Name))
    {
      var suffix = 2;
      string uniqueName;

      do
      {
        uniqueName = FormattableString.Invariant($"{method.Name}{suffix++:D}");
      } while (!methodNames.Add(uniqueName));

      method = method with { UniqueName = uniqueName };
    }

    methodIdentifiers.Add(identifier);
    methods.Add(method);
  }
  private static bool TryGetMvcMethodParameterAttributes(IParameterSymbol parameterSymbol, ImmutableArray<Diagnostic>.Builder diagnostics, ref string generatorName, out MvcBindingSourceInfo? bindingSource, CancellationToken cancellationToken)
  {
    bindingSource = null;

    foreach (var attribute in parameterSymbol.GetAttributes())
    {
      switch (attribute.AttributeClass?.ToDisplayString())
      {
        case AspNetClassNames.FromBodyAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Body);
          break;

        case AspNetClassNames.FromFormAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Form);
          break;

        case AspNetClassNames.FromHeaderAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Header);
          break;

        case AspNetClassNames.FromQueryAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Query);
          break;

        case AspNetClassNames.FromRouteAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Route);
          break;

        case AspNetClassNames.FromKeyedServicesAttribute:
        case AspNetClassNames.FromServicesAttribute:
          // Exclude service-bound parameters since their values come from services rather than the HTTP request
          if (bindingSource is null)
          {
            return false;
          }
          break;

        case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
          return false;

        case GeneratorClassNames.RouteGeneratorNameAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
          {
            if (SyntaxFacts.IsValidIdentifier(generatorNameValue))
            {
              generatorName = CSharpSupport.PascalToCamelCase(generatorNameValue);
            }
            else
            {
              diagnostics.Add(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()));
            }
          }
          break;
      }
    }

    return true;
  }
  private static MvcMethodParameterInfo? GetMvcMethodParameterInfo(IParameterSymbol parameterSymbol, SemanticModel semanticModel, ImmutableArray<Diagnostic>.Builder diagnostics, CancellationToken cancellationToken)
  {
    if (string.Equals(parameterSymbol.Type.ToDisplayString(), AspNetClassNames.CancellationToken, StringComparison.Ordinal))
    {
      return null;
    }

    var name = parameterSymbol.Name;
    var generatorName = name;

    if (!TryGetMvcMethodParameterAttributes(parameterSymbol, diagnostics, ref generatorName, out var bindingSource, cancellationToken))
    {
      return null;
    }

    var escapedName = CSharpSupport.EscapeIdentifier(generatorName);
    var propertyName = CSharpSupport.EscapeIdentifier(CSharpSupport.CamelToPascalCase(generatorName));
    var routeKey = bindingSource?.Name ?? name;
    var type = GetTypeInfo(parameterSymbol, parameterSymbol.Type, semanticModel);
    var defaultValueExpression = GetSanitisedDefaultValue(parameterSymbol, semanticModel, cancellationToken);

    return new MvcMethodParameterInfo(name, escapedName, propertyName, routeKey, type, defaultValueExpression, bindingSource);
  }
  private static bool TryGetMvcPropertyAttributes(IPropertySymbol propertySymbol, ImmutableArray<Diagnostic>.Builder diagnostics, ref string generatorName, out MvcBindingSourceInfo? bindingSource, CancellationToken cancellationToken)
  {
    bindingSource = null;

    foreach (var attribute in propertySymbol.GetAttributes())
    {
      switch (attribute.AttributeClass?.ToDisplayString())
      {
        case AspNetClassNames.BindPropertyAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Custom);
          break;

        case AspNetClassNames.FromBodyAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Body);
          break;

        case AspNetClassNames.FromFormAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Form);
          break;

        case AspNetClassNames.FromHeaderAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Header);
          break;

        case AspNetClassNames.FromQueryAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Query);
          break;

        case AspNetClassNames.FromRouteAttribute:
          bindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Route);
          break;

        case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
          return false;

        case GeneratorClassNames.RouteGeneratorNameAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
          {
            if (SyntaxFacts.IsValidIdentifier(generatorNameValue))
            {
              generatorName = CSharpSupport.CamelToPascalCase(generatorNameValue);
            }
            else
            {
              diagnostics.Add(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()));
            }
          }
          break;
      }
    }

    return true;
  }
  private static MvcPropertyInfo? GetMvcPropertyInfo(IPropertySymbol propertySymbol, SemanticModel semanticModel, MvcBindingSourceInfo? defaultBindingSource, ImmutableArray<Diagnostic>.Builder diagnostics, CancellationToken cancellationToken)
  {
    if (propertySymbol.GetMethod?.DeclaredAccessibility != Accessibility.Public || propertySymbol.SetMethod?.DeclaredAccessibility != Accessibility.Public)
    {
      return null;
    }

    var generatorName = propertySymbol.Name;

    if (!TryGetMvcPropertyAttributes(propertySymbol, diagnostics, ref generatorName, out var bindingSource, cancellationToken))
    {
      return null;
    }

    bindingSource ??= defaultBindingSource;

    if (bindingSource is null)
    {
      return null;
    }

    var originalName = propertySymbol.Name;
    var escapedOriginalName = CSharpSupport.EscapeIdentifier(originalName);
    var escapedName = CSharpSupport.EscapeIdentifier(generatorName);
    var routeKey = bindingSource.Name ?? originalName;
    var type = GetTypeInfo(propertySymbol, propertySymbol.Type, semanticModel);

    return new MvcPropertyInfo(originalName, escapedOriginalName, escapedName, routeKey, type, bindingSource);
  }
  private static void GetPageAttributes(INamedTypeSymbol typeSymbol, ImmutableArray<Diagnostic>.Builder diagnostics, out MvcBindingSourceInfo? defaultBindingSource, ref string generatorName, CancellationToken cancellationToken)
  {
    defaultBindingSource = null;

    foreach (var attribute in typeSymbol.GetAttributes())
    {
      switch (attribute.AttributeClass?.ToDisplayString())
      {
        case AspNetClassNames.BindPropertiesAttribute:
          defaultBindingSource ??= attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Custom);
          break;

        case GeneratorClassNames.RouteGeneratorNameAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
          {
            if (SyntaxFacts.IsValidIdentifier(generatorNameValue))
            {
              generatorName = CSharpSupport.CamelToPascalCase(generatorNameValue);
            }
            else
            {
              diagnostics.Add(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()));
            }
          }
          break;
      }
    }
  }
  private static void GetPageMembers(CandidateClassInfo classInfo, INamedTypeSymbol typeSymbol, MvcBindingSourceInfo? defaultBindingSource, ImmutableArray<Diagnostic>.Builder diagnostics, out ImmutableArray<MvcPropertyInfo> properties, out ImmutableArray<PageMethodInfo> methods, CancellationToken cancellationToken)
  {
    var propertiesBuilder = ImmutableArray.CreateBuilder<MvcPropertyInfo>();
    var methodsBuilder = ImmutableArray.CreateBuilder<PageMethodInfo>();
    var methodSymbols = ImmutableArray.CreateBuilder<IMethodSymbol>();
    var accessedMembers = new HashSet<string>(StringComparer.Ordinal);
    var methodNames = new HashSet<string>(StringComparer.Ordinal);

    foreach (var symbol in typeSymbol.EnumerateSelfAndBaseTypes())
    {
      var typeName = symbol.ToDisplayString();

      if (string.Equals(typeName, AspNetClassNames.PageModel, StringComparison.Ordinal)
        || string.Equals(typeName, "object", StringComparison.Ordinal))
      {
        break;
      }

      foreach (var member in symbol.GetMembers())
      {
        if (member.IsAbstract || member.IsImplicitlyDeclared || member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
        {
          continue;
        }

        var displayName = member.ToDisplayString(UniqueClassMemberSymbolDisplayFormat);

        if (!accessedMembers.Add(displayName))
        {
          continue;
        }

        if (member is IPropertySymbol propertySymbol)
        {
          if (GetMvcPropertyInfo(propertySymbol, classInfo.SemanticModel, defaultBindingSource, diagnostics, cancellationToken) is { } propertyInfo)
          {
            propertiesBuilder.Add(propertyInfo);
          }
        }
        else if (member is IMethodSymbol methodSymbol)
        {
          methodSymbols.Add(methodSymbol);
        }
      }

      // Inherited members don't receive default binding
      defaultBindingSource = null;
    }

    properties = propertiesBuilder.ToImmutable();

    foreach (var methodSymbol in methodSymbols)
    {
      if (GetPageMethodInfo(methodSymbol, classInfo.SemanticModel, diagnostics, cancellationToken) is not { } method)
      {
        continue;
      }

      var (isModified, parameters) = CombineBoundProperties(method.Parameters, properties);
      if (isModified)
      {
        method = method with { Parameters = parameters };
      }

      if (!methodNames.Add(method.Name))
      {
        diagnostics.Add(Diagnostics.CreateConflictingMethodsDiagnostic(typeSymbol.Name, method.Name, classInfo.TypeDeclarationSyntax.GetLocation()));
        continue;
      }

      methodsBuilder.Add(method);
    }

    methods = methodsBuilder.ToImmutable();
  }
  private static PageMethodInfo? GetPageMethodInfo(IMethodSymbol methodSymbol, SemanticModel semanticModel, ImmutableArray<Diagnostic>.Builder diagnostics, CancellationToken cancellationToken)
  {
    if (methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.IsGenericMethod
      || !TryParseRazorPageMethodName(methodSymbol.Name, out var name, out var handlerName))
    {
      return null;
    }

    foreach (var attribute in methodSymbol.GetAttributes())
    {
      switch (attribute.AttributeClass?.ToDisplayString())
      {
        case AspNetClassNames.NonHandlerAttribute:
        case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
          return null;

        case GeneratorClassNames.RouteGeneratorNameAttribute:
          if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
          {
            if (SyntaxFacts.IsValidIdentifier(generatorNameValue))
            {
              name = CSharpSupport.CamelToPascalCase(generatorNameValue);
            }
            else
            {
              diagnostics.Add(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()));
            }
          }
          break;
      }
    }

    var parameters = ImmutableArray.CreateBuilder<MvcMethodParameterInfo>();

    foreach (var parameterSymbol in methodSymbol.Parameters)
    {
      if (parameterSymbol.RefKind != RefKind.None)
      {
        return null;
      }

      if (GetMvcMethodParameterInfo(parameterSymbol, semanticModel, diagnostics, cancellationToken) is { } parameter)
      {
        parameters.Add(parameter);
      }
    }

    var escapedName = CSharpSupport.EscapeIdentifier(name);

    return new PageMethodInfo(name, escapedName, name, handlerName, methodSymbol.ToDisplayString(UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat), parameters.ToImmutable());
  }
  private static ExpressionSyntax? GetSanitisedDefaultValue(IParameterSymbol parameterSymbol, SemanticModel semanticModel, CancellationToken cancellationToken)
  {
    if (!parameterSymbol.HasExplicitDefaultValue)
    {
      return null;
    }

    foreach (var reference in parameterSymbol.DeclaringSyntaxReferences)
    {
      var node = reference.GetSyntax(cancellationToken);
      if (node is ParameterSyntax { Default: { } equalsValueClauseSyntax }
        && new DefaultValueExpressionRewriter(semanticModel, cancellationToken).Visit(equalsValueClauseSyntax.Value) is ExpressionSyntax rewrittenExpression)
      {
        return rewrittenExpression;
      }
    }

    return null;
  }
  private static TypeInfo GetTypeInfo(ISymbol symbol, ITypeSymbol typeSymbol, SemanticModel semanticModel)
  {
    var annotationsEnabled = semanticModel
      .GetNullableContext(symbol.Locations[0].SourceSpan.Start)
      .AnnotationsEnabled();

    var parameterTypeName = annotationsEnabled
      ? typeSymbol.ToDisplayString(FullyQualifiedWithAnnotationsFormat)
      : typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    var parameterTypeNameSansAnnotations = annotationsEnabled
      ? typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
      : parameterTypeName;

    return new TypeInfo(parameterTypeName, parameterTypeNameSansAnnotations, annotationsEnabled);
  }
  private static (bool IsController, bool IsPage) IsControllerOrPage(INamedTypeSymbol typeSymbol)
  {
    var isCandidateController = false;
    var isExcludedController = false;
    var inheritsFromPageModel = false;
    var isAncestor = false;

    (bool IsController, bool IsPage) ToResult() => (IsController: isCandidateController && !isExcludedController, IsPage: inheritsFromPageModel);

    foreach (var symbol in typeSymbol.EnumerateSelfAndBaseTypes())
    {
      switch (symbol.ToDisplayString())
      {
        case AspNetClassNames.Controller:
        case AspNetClassNames.ControllerBase:
          isCandidateController = true;
          return ToResult();

        case AspNetClassNames.PageModel:
          inheritsFromPageModel = true;
          return ToResult();
      }

      foreach (var attribute in symbol.GetAttributes())
      {
        switch (attribute.AttributeClass?.ToDisplayString())
        {
          case AspNetClassNames.ControllerAttribute:
            isCandidateController = true;
            break;

          case AspNetClassNames.NonControllerAttribute:
            isExcludedController = true;
            break;

          case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
            if (!isAncestor)
            {
              return (IsController: false, IsPage: false);
            }
            break;
        }
      }

      isAncestor = true;
    }

    return ToResult();
  }
  private static bool TryParseRazorPageMethodName(string methodName, [MaybeNullWhen(false)] out string name, out string? handler)
  {
    var methodNameMatch = RazorPageMethodNameRegex.Match(methodName);
    if (!methodNameMatch.Success)
    {
      name = null;
      handler = null;
      return false;
    }

    handler = methodNameMatch.Groups["handler"].Value;
    if (string.IsNullOrEmpty(handler))
    {
      handler = null;
    }

    name = methodNameMatch.Groups["name"].Value;

    return true;
  }
  private static (bool IsModified, ImmutableArray<MvcMethodParameterInfo> Parameters) CombineBoundProperties(ImmutableArray<MvcMethodParameterInfo> parameters, ImmutableArray<MvcPropertyInfo> properties)
  {
    var resultParameters = new Lazy<ImmutableArray<MvcMethodParameterInfo>.Builder>(() =>
    {
      var builder = ImmutableArray.CreateBuilder<MvcMethodParameterInfo>(parameters.Length);
      builder.AddRange(parameters);
      return builder;
    });

    foreach (var property in properties)
    {
      if (!property.AffectsUrl())
      {
        continue;
      }

      var propertyBoundName = property.BindingSource.Name ?? property.EscapedName;
      var hasConflictingBoundParameter = false;
      var hasConflictingParameterName = false;

      foreach (var parameter in parameters)
      {
        if (string.Equals(property.EscapedName, parameter.EscapedName, StringComparison.OrdinalIgnoreCase))
        {
          hasConflictingParameterName = true;
          break;
        }

        if (!parameter.AffectsUrl())
        {
          continue;
        }

        var parameterBoundName = parameter.BindingSource?.Name ?? parameter.EscapedName;

        if (string.Equals(propertyBoundName, parameterBoundName, StringComparison.OrdinalIgnoreCase))
        {
          hasConflictingBoundParameter = true;
          break;
        }
      }

      if (hasConflictingBoundParameter || hasConflictingParameterName)
      {
        continue;
      }

      resultParameters.Value.Add(new MvcMethodParameterInfo(property.OriginalName, CSharpSupport.PascalToCamelCase(property.EscapedName), PropertyName: null, property.RouteKey, property.Type, DefaultValueExpression: null, property.BindingSource));
    }

    return (resultParameters.IsValueCreated, resultParameters.IsValueCreated ? resultParameters.Value.ToImmutable() : parameters);
  }

  private static readonly SymbolDisplayFormat UniqueClassMemberSymbolDisplayFormat = new(
    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
    memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
    delegateStyle: SymbolDisplayDelegateStyle.NameAndParameters,
    extensionMethodStyle: SymbolDisplayExtensionMethodStyle.StaticMethod,
    parameterOptions: SymbolDisplayParameterOptions.IncludeType,
    propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
    localOptions: SymbolDisplayLocalOptions.None,
    kindOptions: SymbolDisplayKindOptions.None,
    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

  private static readonly SymbolDisplayFormat UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat = UniqueClassMemberSymbolDisplayFormat
    .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

  private static readonly SymbolDisplayFormat FullyQualifiedWithAnnotationsFormat = SymbolDisplayFormat.FullyQualifiedFormat
    .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

  private static readonly Regex RazorPageMethodNameRegex = new("^On(?<name>(?<verb>Delete|Get|Head|Options|Patch|Post|Put)(?<handler>.*?))(Async)?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
}
