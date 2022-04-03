using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SafeRouting.Generator
{
  internal static class Parser
  {
    public static GeneratorOptions GetOptions(AnalyzerConfigOptionsProvider optionsProvider)
    {
      var diagnostics = new List<Diagnostic>();

      var generatedAccessModifier = GetGeneratedAccessModifierOption(optionsProvider.GlobalOptions, diagnostics);
      var generatedNamespace = GetGeneratedNamespaceOption(optionsProvider.GlobalOptions, diagnostics);

      return new GeneratorOptions(generatedAccessModifier, generatedNamespace, diagnostics);
    }

    public static bool IsCandidateNode(SyntaxNode node)
      => node is TypeDeclarationSyntax typeDeclarationSyntax
        && node is not InterfaceDeclarationSyntax
        && typeDeclarationSyntax.TypeParameterList is null
        && typeDeclarationSyntax.Parent is not TypeDeclarationSyntax
        && (typeDeclarationSyntax.AttributeLists.Count > 0 || typeDeclarationSyntax.BaseList?.Types.Count > 0)
        && typeDeclarationSyntax.Modifiers.Any(t => t.IsKind(SyntaxKind.PublicKeyword))
        && !typeDeclarationSyntax.Modifiers.Any(t => t.IsKind(SyntaxKind.StaticKeyword) || t.IsKind(SyntaxKind.AbstractKeyword));

    public static CandidateClassInfo? TransformCandidateClassNode(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
      var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;

      if (context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax, cancellationToken) is not INamedTypeSymbol typeSymbol)
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

    public static ControllerInfo? GetControllerInfo(CandidateClassInfo classInfo, SourceProductionContext context)
    {
      if (!classInfo.IsController)
      {
        return null;
      }

      var typeSymbol = classInfo.TypeSymbol;

      // https://github.com/dotnet/aspnetcore/blob/2862028573708e5684bf17526c43127e178525d4/src/Mvc/Mvc.Core/src/ApplicationModels/DefaultApplicationModelProvider.cs#L167
      var controllerName = typeSymbol.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
        ? typeSymbol.Name.Substring(0, typeSymbol.Name.Length - "Controller".Length)
        : typeSymbol.Name;

      if (controllerName.Length == 0)
      {
        return null;
      }

      var generatorName = controllerName;

      GetControllerAttributes(context, typeSymbol, out var areaName, out var defaultBindingSource, out var defaultBindingLevel, ref generatorName);

      GetControllerMembers(classInfo, context, typeSymbol, defaultBindingSource, defaultBindingLevel, out var properties, out var methods);

      if (methods.Count == 0)
      {
        return null;
      }

      return new ControllerInfo(controllerName, generatorName, areaName, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), classInfo.TypeDeclarationSyntax, properties, methods);
    }

    public static PageInfo? GetPageInfo(CandidateClassInfo classInfo, SourceProductionContext context)
    {
      if (!classInfo.IsPage)
      {
        return null;
      }

      var typeSymbol = classInfo.TypeSymbol;
      var typeDeclarationSyntax = classInfo.TypeDeclarationSyntax;

      var filePath = typeDeclarationSyntax.SyntaxTree.FilePath;
      if (string.IsNullOrEmpty(filePath))
      {
        return null;
      }

      var fileInfo = new FileInfo(filePath);
      if (!fileInfo.Name.EndsWith(".cshtml.cs", StringComparison.InvariantCultureIgnoreCase))
      {
        return null;
      }

      var directory = fileInfo.Directory;
      var pathSegments = new List<string>();

      for (; directory != null && !string.Equals(directory.Name, "Pages", StringComparison.InvariantCultureIgnoreCase); directory = directory.Parent)
      {
        pathSegments.Insert(0, directory.Name);
      }

      if (directory is null)
      {
        return null;
      }

      var pageNamespace = string.Join("_", pathSegments);
      var areaName = default(string);
      if (directory.Parent?.Parent?.Name.Equals("Areas", StringComparison.InvariantCultureIgnoreCase) ?? false)
      {
        areaName = directory.Parent.Name;
      }

      var pageName = fileInfo.Name.Substring(0, fileInfo.Name.Length - ".cshtml.cs".Length);
      pathSegments.Add(pageName);
      var pagePath = "/" + string.Join("/", pathSegments);

      var generatorName = pageName;

      GetPageAttributes(context, typeSymbol, out var defaultBindingSource, ref generatorName);

      GetPageMembers(classInfo, context, typeSymbol, defaultBindingSource, out var properties, out var methods);

      if (methods.Count == 0)
      {
        return null;
      }

      return new PageInfo(pagePath, generatorName, areaName, pageNamespace, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), typeDeclarationSyntax, properties, methods);
    }

    private static string GetGeneratedAccessModifierOption(AnalyzerConfigOptions options, IList<Diagnostic> diagnostics)
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
    private static string GetGeneratedNamespaceOption(AnalyzerConfigOptions options, IList<Diagnostic> diagnostics)
    {
      var generatedNamespace = GeneratorSupport.DefaultGeneratedRootNamespace;

      if (!options.TryGetValue(GeneratorSupport.GeneratedNamespaceOption, out var generatedNamespaceValue))
      {
        return generatedNamespace;
      }

      if (generatedNamespaceValue.Split('.').All(x => SyntaxFacts.IsValidIdentifier(x)))
      {
        generatedNamespace = generatedNamespaceValue;
      }
      else
      {
        diagnostics.Add(Diagnostics.CreateInvalidOptionDiagnostic(GeneratorSupport.GeneratedNamespaceOption, $"'{generatedNamespaceValue}' is not a valid namespace identifier."));
      }

      return generatedNamespace;
    }
    private static void GetControllerAttributes(SourceProductionContext context, INamedTypeSymbol typeSymbol, out string? areaName, out MvcBindingSourceInfo? defaultBindingSource, out INamedTypeSymbol? defaultBindingLevel, ref string generatorName)
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
                defaultBindingSource = new MvcBindingSourceInfo(MvcBindingSourceType.Custom);
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
                context.ReportDiagnostic(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
              break;
          }
        }
      }
    }
    private static void GetControllerMembers(CandidateClassInfo classInfo, SourceProductionContext context, INamedTypeSymbol typeSymbol, MvcBindingSourceInfo? defaultBindingSource, INamedTypeSymbol? defaultBindingLevel, out List<MvcPropertyInfo> properties, out IReadOnlyCollection<ControllerMethodInfo> methods)
    {
      properties = new List<MvcPropertyInfo>();
      var methodIdentifierDictionary = new Dictionary<string, ControllerMethodInfo>(StringComparer.Ordinal);
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
            if (propertySymbol.GetMethod?.DeclaredAccessibility != Accessibility.Public || propertySymbol.SetMethod?.DeclaredAccessibility != Accessibility.Public || GetMvcPropertyInfo(context, propertySymbol, defaultBindingSource, classInfo.SemanticModel) is not MvcPropertyInfo propertyInfo)
            {
              continue;
            }

            properties.Add(propertyInfo);
          }
          else if (member is IMethodSymbol methodSymbol)
          {
            if (methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.IsGenericMethod || GetControllerMethodInfo(context, methodSymbol, classInfo.SemanticModel) is not ControllerMethodInfo method)
            {
              continue;
            }

            var identifier = $"{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Type.FullyQualifiedName))})";

            // Collapse multiple methods with the same parameters
            if (methodIdentifierDictionary.ContainsKey(identifier))
            {
              continue;
            }

            var urlAffectedIdentifier = $"{method.Name}({string.Join(", ", method.Parameters.Where(x => x.AffectsUrl()).Select(x => x.Type.FullyQualifiedName))})";

            if (!urlAffectedIdentifiers.Add(urlAffectedIdentifier))
            {
              context.ReportDiagnostic(Diagnostics.CreateConflictingMethodsDiagnostic(classInfo.TypeSymbol.Name, urlAffectedIdentifier, methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken).GetLocation()));
              continue;
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

            methodIdentifierDictionary[identifier] = method;
          }
        }

        if (ReferenceEquals(symbol, defaultBindingLevel))
        {
          // Inherited members don't receive default binding
          defaultBindingSource = null;
        }
      }

      methods = methodIdentifierDictionary.Values;
    }
    private static bool TryGetControllerMethodAttributes(SourceProductionContext context, IMethodSymbol methodSymbol, out string? areaName, ref string name, ref string actionName)
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
                context.ReportDiagnostic(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }

      return true;
    }
    private static ControllerMethodInfo? GetControllerMethodInfo(SourceProductionContext context, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
      var name = methodSymbol.Name.EndsWith("Async", StringComparison.Ordinal)
        ? methodSymbol.Name.Substring(0, methodSymbol.Name.Length - "Async".Length)
        : methodSymbol.Name;

      var actionName = name;

      if (!TryGetControllerMethodAttributes(context, methodSymbol, out var areaName, ref name, ref actionName)
        || name.Length == 0)
      {
        return null;
      }

      var parameters = new List<MvcMethodParameterInfo>();

      foreach (var parameterSymbol in methodSymbol.Parameters)
      {
        // Methods with in/out/ref parameters are ignored by ASP.NET
        if (parameterSymbol.RefKind != RefKind.None)
        {
          return null;
        }

        if (GetMvcMethodParameterInfo(context, parameterSymbol, semanticModel) is MvcMethodParameterInfo parameter)
        {
          parameters.Add(parameter);
        }
      }

      var escapedName = CSharpSupport.EscapeIdentifier(name);
      var fullyQualifiedMethodDeclaration = methodSymbol.ToDisplayString(UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat);

      return new ControllerMethodInfo(name, escapedName, name, actionName, areaName, fullyQualifiedMethodDeclaration, parameters);
    }
    private static bool TryGetMvcMethodParameterAttributes(SourceProductionContext context, IParameterSymbol parameterSymbol, ref string generatorName, out MvcBindingSourceInfo? bindingSource)
    {
      bindingSource = null;

      foreach (var attribute in parameterSymbol.GetAttributes())
      {
        switch (attribute.AttributeClass?.ToDisplayString())
        {
          case AspNetClassNames.FromBodyAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Body);
            }
            break;

          case AspNetClassNames.FromFormAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Form);
            }
            break;

          case AspNetClassNames.FromHeaderAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Header);
            }
            break;

          case AspNetClassNames.FromQueryAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Query);
            }
            break;

          case AspNetClassNames.FromRouteAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Route);
            }
            break;

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
                context.ReportDiagnostic(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }

      return true;
    }
    private static MvcMethodParameterInfo? GetMvcMethodParameterInfo(SourceProductionContext context, IParameterSymbol parameterSymbol, SemanticModel semanticModel)
    {
      if (string.Equals(parameterSymbol.Type.ToDisplayString(), AspNetClassNames.CancellationToken, StringComparison.Ordinal))
      {
        return null;
      }

      var name = parameterSymbol.Name;
      var generatorName = name;

      if (!TryGetMvcMethodParameterAttributes(context, parameterSymbol, ref generatorName, out var bindingSource))
      {
        return null;
      }

      var escapedName = CSharpSupport.EscapeIdentifier(generatorName);
      var propertyName = CSharpSupport.EscapeIdentifier(CSharpSupport.CamelToPascalCase(generatorName));
      var routeKey = bindingSource?.Name ?? name;
      var type = GetTypeInfo(parameterSymbol, parameterSymbol.Type, semanticModel);
      var defaultValueExpression = GetSanitisedDefaultValue(parameterSymbol, semanticModel, context.CancellationToken);

      return new MvcMethodParameterInfo(name, escapedName, propertyName, routeKey, type, defaultValueExpression, bindingSource);
    }
    private static bool TryGetMvcPropertyAttributes(SourceProductionContext context, IPropertySymbol propertySymbol, ref string generatorName, out MvcBindingSourceInfo? bindingSource)
    {
      bindingSource = null;

      foreach (var attribute in propertySymbol.GetAttributes())
      {
        switch (attribute.AttributeClass?.ToDisplayString())
        {
          case AspNetClassNames.BindPropertyAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Custom);
            }
            break;

          case AspNetClassNames.FromBodyAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Body);
            }
            break;

          case AspNetClassNames.FromFormAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Form);
            }
            break;

          case AspNetClassNames.FromHeaderAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Header);
            }
            break;

          case AspNetClassNames.FromQueryAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Query);
            }
            break;

          case AspNetClassNames.FromRouteAttribute:
            if (bindingSource is null)
            {
              bindingSource = attribute.ParseBindingSourceAttribute(MvcBindingSourceType.Route);
            }
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
                context.ReportDiagnostic(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }

      return true;
    }
    private static MvcPropertyInfo? GetMvcPropertyInfo(SourceProductionContext context, IPropertySymbol propertySymbol, MvcBindingSourceInfo? defaultBindingSource, SemanticModel semanticModel)
    {
      var generatorName = propertySymbol.Name;

      if (!TryGetMvcPropertyAttributes(context, propertySymbol, ref generatorName, out var bindingSource))
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
    private static void GetPageAttributes(SourceProductionContext context, INamedTypeSymbol typeSymbol, out MvcBindingSourceInfo? defaultBindingSource, ref string generatorName)
    {
      defaultBindingSource = null;

      foreach (var attribute in typeSymbol.GetAttributes())
      {
        switch (attribute.AttributeClass?.ToDisplayString())
        {
          case AspNetClassNames.BindPropertiesAttribute:
            if (defaultBindingSource is not null)
            {
              break;
            }

            defaultBindingSource = new MvcBindingSourceInfo(MvcBindingSourceType.Custom);
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
                context.ReportDiagnostic(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }
    }
    private static void GetPageMembers(CandidateClassInfo classInfo, SourceProductionContext context, INamedTypeSymbol typeSymbol, MvcBindingSourceInfo? defaultBindingSource, out List<MvcPropertyInfo> properties, out List<PageMethodInfo> methods)
    {
      properties = new List<MvcPropertyInfo>();
      methods = new List<PageMethodInfo>();
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
            if (propertySymbol.GetMethod?.DeclaredAccessibility != Accessibility.Public || propertySymbol.SetMethod?.DeclaredAccessibility != Accessibility.Public || GetMvcPropertyInfo(context, propertySymbol, defaultBindingSource, classInfo.SemanticModel) is not MvcPropertyInfo propertyInfo)
            {
              continue;
            }

            properties.Add(propertyInfo);
          }
          else if (member is IMethodSymbol methodSymbol)
          {
            if (methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.IsGenericMethod || GetPageMethodInfo(context, methodSymbol, classInfo.SemanticModel) is not PageMethodInfo method)
            {
              continue;
            }

            if (!methodNames.Add(method.Name))
            {
              context.ReportDiagnostic(Diagnostics.CreateConflictingMethodsDiagnostic(typeSymbol.Name, method.Name, classInfo.TypeDeclarationSyntax.GetLocation()));
              continue;
            }

            methods.Add(method);
          }
        }

        // Inherited members don't receive default binding
        defaultBindingSource = null;
      }
    }
    private static PageMethodInfo? GetPageMethodInfo(SourceProductionContext context, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
      if (!ParseRazorPageMethodName(methodSymbol.Name, out var name, out var handlerName))
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
                context.ReportDiagnostic(Diagnostics.CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }

      var parameters = new List<MvcMethodParameterInfo>();

      foreach (var parameterSymbol in methodSymbol.Parameters)
      {
        if (parameterSymbol.RefKind != RefKind.None)
        {
          return null;
        }

        if (GetMvcMethodParameterInfo(context, parameterSymbol, semanticModel) is MvcMethodParameterInfo parameter)
        {
          parameters.Add(parameter);
        }
      }

      var escapedName= CSharpSupport.EscapeIdentifier(name);

      return new PageMethodInfo(name, escapedName, name, handlerName, methodSymbol.ToDisplayString(UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat), parameters);
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
        if (node is ParameterSyntax parameterSyntax
          && parameterSyntax.Default is EqualsValueClauseSyntax equalsValueClauseSyntax
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
    private static bool ParseRazorPageMethodName(string methodName, [MaybeNullWhen(false)] out string name, out string? handler)
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

    private static SymbolDisplayFormat UniqueClassMemberSymbolDisplayFormat { get; } = new SymbolDisplayFormat(
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

    private static SymbolDisplayFormat UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat { get; } = UniqueClassMemberSymbolDisplayFormat
      .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static SymbolDisplayFormat FullyQualifiedWithAnnotationsFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
      .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static Regex RazorPageMethodNameRegex { get; } = new Regex(@"^On(?<name>(?<verb>Delete|Get|Head|Options|Patch|Post|Put)(?<handler>.*?))(Async)?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
  }
}
