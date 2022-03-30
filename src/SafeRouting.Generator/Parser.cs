using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SafeRouting.Generator
{
  internal static class Parser
  {
    public static GeneratorOptions GetOptions(AnalyzerConfigOptionsProvider optionsProvider)
    {
      var optionErrors = new Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer);

      var generatedAccessModifier = GeneratorSupport.DefaultGeneratedAccessModifier;
      if (optionsProvider.GlobalOptions.TryGetValue(GeneratorSupport.GeneratedAccessModifierOption, out var generatedAccessModifierValue))
      {
        if (new[] { "public", "internal" }.Any(x => string.Equals(x, generatedAccessModifierValue, StringComparison.Ordinal)))
        {
          generatedAccessModifier = generatedAccessModifierValue;
        }
        else
        {
          optionErrors[GeneratorSupport.GeneratedNamespaceOption] = $"'{generatedAccessModifierValue}' is not a supported access modifier, must be public or internal.";
        }
      }

      var generatedNamespace = GeneratorSupport.DefaultGeneratedRootNamespace;
      if (optionsProvider.GlobalOptions.TryGetValue(GeneratorSupport.GeneratedNamespaceOption, out var generatedNamespaceValue))
      {
        if (generatedNamespaceValue.Split('.').All(x => CSharpSupport.IsValidIdentifier(x)))
        {
          generatedNamespace = generatedNamespaceValue;
        }
        else
        {
          optionErrors[GeneratorSupport.GeneratedNamespaceOption] = $"'{generatedNamespaceValue}' is not a valid namespace identifier.";
        }
      }

      return new GeneratorOptions(generatedAccessModifier, generatedNamespace, optionErrors);
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

      if (context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax, cancellationToken) is not INamedTypeSymbol classSymbol)
      {
        return null;
      }

      var isController = false;
      var hasControllerAttribute = false;
      var isExcludedController = false;
      var isPage = false;
      var isAncestor = false;

      foreach (var typeSymbol in classSymbol.EnumerateSelfAndBaseTypes())
      {
        switch (typeSymbol.ToDisplayString())
        {
          case AspNetClassNames.Controller:
          case AspNetClassNames.ControllerBase:
            isController = true;
            break;

          case AspNetClassNames.PageModel:
            isPage = true;
            break;
        }

        if (isController || isPage)
        {
          break;
        }  

        foreach (var attribute in typeSymbol.GetAttributes())
        {
          switch (attribute.AttributeClass?.ToDisplayString())
          {
            case AspNetClassNames.ControllerAttribute:
              hasControllerAttribute = true;
              break;

            case AspNetClassNames.NonControllerAttribute:
              isExcludedController = true;
              break;

            case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
              if (!isAncestor)
              {
                return null;
              }
              break;
          }
        }

        isAncestor = true;
      }

      isController |= hasControllerAttribute;
      isController &= !isExcludedController;

      if (!isController && !isPage)
      {
        return null;
      }

      return new CandidateClassInfo(typeDeclarationSyntax, classSymbol, context.SemanticModel, isController, isPage);
    }

    public static ControllerInfo? GetControllerInfo(CandidateClassInfo classInfo, SourceProductionContext context)
    {
      if (!classInfo.IsController)
      {
        return null;
      }

      var classSymbol = classInfo.ClassSymbol;

      // https://github.com/dotnet/aspnetcore/blob/2862028573708e5684bf17526c43127e178525d4/src/Mvc/Mvc.Core/src/ApplicationModels/DefaultApplicationModelProvider.cs#L167
      var controllerName = classSymbol.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
        ? classSymbol.Name.Substring(0, classSymbol.Name.Length - "Controller".Length)
        : classSymbol.Name;

      if (controllerName.Length == 0)
      {
        return null;
      }

      var generatorName = controllerName;

      GetControllerAttributes(context, classSymbol, out var areaName, out var defaultBindingSource, out var defaultBindingLevel, ref generatorName);

      GetControllerMembers(classInfo, context, classSymbol, defaultBindingSource, defaultBindingLevel, out var properties, out var methods);

      if (methods.Count == 0)
      {
        return null;
      }

      return new ControllerInfo(controllerName, generatorName, areaName, classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), classInfo.TypeDeclarationSyntax, properties, methods);
    }

    public static PageInfo? GetPageInfo(CandidateClassInfo classInfo, SourceProductionContext context)
    {
      if (!classInfo.IsPage)
      {
        return null;
      }

      var classSymbol = classInfo.ClassSymbol;
      var classDeclarationSyntax = classInfo.TypeDeclarationSyntax;

      var filePath = classDeclarationSyntax.SyntaxTree.FilePath;
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

      GetPageAttributes(context, classSymbol, out var defaultBindingSource, ref generatorName);

      GetPageMembers(classInfo, context, classSymbol, defaultBindingSource, out var properties, out var methods);

      if (methods.Count == 0)
      {
        return null;
      }

      return new PageInfo(pagePath, generatorName, areaName, pageNamespace, classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), classDeclarationSyntax, properties, methods);
    }

    public static Diagnostic CreateConflictingMethodsDiagnostic(string className, string methodName, Location location) => Diagnostic.Create(ConflictingMethodsDescriptor, location, className, methodName);

    public static Diagnostic CreateInvalidOptionDiagnostic(string optionKey, string errorText) => Diagnostic.Create(InvalidOptionsDescriptor, location: null, optionKey, errorText);

    public static Diagnostic CreateInvalidIdentifierDiagnostic(string identifier, Location? location) => Diagnostic.Create(InvalidIdentifierDescriptor, location, identifier);

    public static Diagnostic CreateConflictingControllerDiagnostic(string controllerName, Location location) => Diagnostic.Create(ConflictingControllerDescriptor, location, controllerName);

    public static Diagnostic CreateConflictingPageClassDiagnostic(string pageClassName, Location location) => Diagnostic.Create(ConflictingPageClassDescriptor, location, pageClassName);

    public static Diagnostic CreateUnsupportedLanguageVersionDiagnostic() => Diagnostic.Create(UnsupportedLanguageVersionDescriptor, location: null);

    private static void GetControllerAttributes(SourceProductionContext context, INamedTypeSymbol classSymbol, out string? areaName, out MvcBindingSourceInfo? defaultBindingSource, out INamedTypeSymbol? defaultBindingLevel, ref string generatorName)
    {
      areaName = null;
      defaultBindingSource = null;
      defaultBindingLevel = null;

      foreach (var symbol in classSymbol.EnumerateSelfAndBaseTypes())
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
              if (!ReferenceEquals(symbol, classSymbol) || !attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
              {
                break;
              }

              if (CSharpSupport.IsValidIdentifier(generatorNameValue))
              {
                generatorName = CSharpSupport.CamelToPascalCase(generatorNameValue);
              }
              else
              {
                context.ReportDiagnostic(CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
              break;
          }
        }
      }
    }
    private static void GetControllerMembers(CandidateClassInfo classInfo, SourceProductionContext context, INamedTypeSymbol classSymbol, MvcBindingSourceInfo? defaultBindingSource, INamedTypeSymbol? defaultBindingLevel, out List<MvcPropertyInfo> properties, out IReadOnlyCollection<ControllerMethodInfo> methods)
    {
      properties = new List<MvcPropertyInfo>();
      var methodIdentifierDictionary = new Dictionary<string, ControllerMethodInfo>(StringComparer.Ordinal);
      // Track all unique member names to avoid including the same member from base classes
      var accessedMembers = new HashSet<string>(StringComparer.Ordinal);
      var urlAffectedIdentifiers = new HashSet<string>(StringComparer.Ordinal);
      var methodNames = new HashSet<string>(StringComparer.Ordinal);

      foreach (var targetSymbol in classSymbol.EnumerateSelfAndBaseTypes())
      {
        var className = targetSymbol.ToDisplayString();
        if (string.Equals(className, AspNetClassNames.Controller, StringComparison.Ordinal)
          || string.Equals(className, AspNetClassNames.ControllerBase, StringComparison.Ordinal)
          || string.Equals(className, "object", StringComparison.Ordinal))
        {
          break;
        }

        foreach (var member in targetSymbol.GetMembers())
        {
          if (member.IsAbstract || member.IsImplicitlyDeclared || member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
          {
            continue;
          }

          var displayName = member.ToDisplayString(RoslynSupport.UniqueClassMemberSymbolDisplayFormat);

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
              context.ReportDiagnostic(CreateConflictingMethodsDiagnostic(classInfo.ClassSymbol.Name, urlAffectedIdentifier, methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken).GetLocation()));
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

              method = new ControllerMethodInfo(method.Name, method.EscapedName, uniqueName, method.ActionName, method.Area, method.FullyQualifiedMethodDeclaration, method.Parameters);
            }

            methodIdentifierDictionary[identifier] = method;
          }
        }

        if (ReferenceEquals(targetSymbol, defaultBindingLevel))
        {
          // Inherited members don't receive default binding
          defaultBindingSource = null;
        }
      }

      methods = methodIdentifierDictionary.Values;
    }
    private static bool TryGetControllerMethodAttributes(SourceProductionContext context, IMethodSymbol methodSymbol, out string? areaName, ref string name, ref string escapedName, ref string actionName)
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
            return false;

          case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
            return false;

          case GeneratorClassNames.RouteGeneratorNameAttribute:
            if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
            {
              if (CSharpSupport.IsValidIdentifier(generatorNameValue))
              {
                name = CSharpSupport.CamelToPascalCase(generatorNameValue);
                escapedName = name;
              }
              else
              {
                context.ReportDiagnostic(CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }

      return true;
    }
    private static ControllerMethodInfo? GetControllerMethodInfo(SourceProductionContext context, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
      var name = methodSymbol.Name;
      var escapedName = methodSymbol.ToDisplayString(RoslynSupport.EscapedIdentifierSymbolDisplayFormat);
      if (name.EndsWith("Async", StringComparison.Ordinal))
      {
        name = name.Substring(0, name.Length - "Async".Length);
        escapedName = name;
      }

      var actionName = name;

      if (!TryGetControllerMethodAttributes(context, methodSymbol, out var areaName, ref name, ref escapedName, ref actionName)
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

      return new ControllerMethodInfo(name, escapedName, name, actionName, areaName, methodSymbol.ToDisplayString(RoslynSupport.UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat), parameters);
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
              if (CSharpSupport.IsValidIdentifier(generatorNameValue))
              {
                generatorName = CSharpSupport.PascalToCamelCase(generatorNameValue);
              }
              else
              {
                context.ReportDiagnostic(CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
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

      var type = GetTypeInfo(parameterSymbol, parameterSymbol.Type, semanticModel);

      return new MvcMethodParameterInfo(name, generatorName, type, parameterSymbol.HasExplicitDefaultValue, parameterSymbol.HasExplicitDefaultValue ? parameterSymbol.ExplicitDefaultValue : null, bindingSource);
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
              if (CSharpSupport.IsValidIdentifier(generatorNameValue))
              {
                generatorName = CSharpSupport.CamelToPascalCase(generatorNameValue);
              }
              else
              {
                context.ReportDiagnostic(CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
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

      if (bindingSource is null)
      {
        bindingSource = defaultBindingSource;
      }

      if (bindingSource is null)
      {
        return null;
      }

      var type = GetTypeInfo(propertySymbol, propertySymbol.Type, semanticModel);

      return new MvcPropertyInfo(propertySymbol.Name, generatorName, type, bindingSource);
    }
    private static void GetPageAttributes(SourceProductionContext context, INamedTypeSymbol classSymbol, out MvcBindingSourceInfo? defaultBindingSource, ref string generatorName)
    {
      defaultBindingSource = null;

      foreach (var attribute in classSymbol.GetAttributes())
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
              if (CSharpSupport.IsValidIdentifier(generatorNameValue))
              {
                generatorName = CSharpSupport.CamelToPascalCase(generatorNameValue);
              }
              else
              {
                context.ReportDiagnostic(CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
              }
            }
            break;
        }
      }
    }
    private static void GetPageMembers(CandidateClassInfo classInfo, SourceProductionContext context, INamedTypeSymbol classSymbol, MvcBindingSourceInfo? defaultBindingSource, out List<MvcPropertyInfo> properties, out List<PageMethodInfo> methods)
    {
      properties = new List<MvcPropertyInfo>();
      methods = new List<PageMethodInfo>();
      var accessedMembers = new HashSet<string>(StringComparer.Ordinal);
      var methodNames = new HashSet<string>(StringComparer.Ordinal);

      foreach (var typeSymbol in classSymbol.EnumerateSelfAndBaseTypes())
      {
        var className = typeSymbol.ToDisplayString();

        if (string.Equals(className, AspNetClassNames.PageModel, StringComparison.Ordinal)
          || string.Equals(className, "object", StringComparison.Ordinal))
        {
          break;
        }

        foreach (var member in typeSymbol.GetMembers())
        {
          if (member.IsAbstract || member.IsImplicitlyDeclared || member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
          {
            continue;
          }

          var displayName = member.ToDisplayString(RoslynSupport.UniqueClassMemberSymbolDisplayFormat);

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
              context.ReportDiagnostic(CreateConflictingMethodsDiagnostic(classSymbol.Name, method.Name, classInfo.TypeDeclarationSyntax.GetLocation()));
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
      if (!RoslynSupport.ParseRazorPageMethodName(methodSymbol.Name, out var name, out var handlerName))
      {
        return null;
      }

      foreach (var attribute in methodSymbol.GetAttributes())
      {
        switch (attribute.AttributeClass?.ToDisplayString())
        {
          case AspNetClassNames.NonHandlerAttribute:
            return null;

          case GeneratorClassNames.ExcludeFromRouteGeneratorAttribute:
            return null;

          case GeneratorClassNames.RouteGeneratorNameAttribute:
            if (attribute.TryGetOptionalStringArgumentAttribute(out var generatorNameValue))
            {
              if (CSharpSupport.IsValidIdentifier(generatorNameValue))
              {
                name = CSharpSupport.CamelToPascalCase(generatorNameValue);
              }
              else
              {
                context.ReportDiagnostic(CreateInvalidIdentifierDiagnostic(generatorNameValue, attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()));
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

      return new PageMethodInfo(name, name, handlerName, methodSymbol.ToDisplayString(RoslynSupport.UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat), parameters);
    }
    private static TypeInfo GetTypeInfo(ISymbol symbol, ITypeSymbol typeSymbol, SemanticModel semanticModel)
    {
      var annotationsEnabled = semanticModel
        .GetNullableContext(symbol.Locations[0].SourceSpan.Start)
        .AnnotationsEnabled();

      var parameterTypeName = annotationsEnabled
        ? typeSymbol.ToDisplayString(RoslynSupport.FullyQualifiedWithAnnotationsFormat)
        : typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      var parameterTypeNameSansAnnotations = annotationsEnabled
        ? typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        : parameterTypeName;

      return new TypeInfo(parameterTypeName, parameterTypeNameSansAnnotations, annotationsEnabled);
    }

    private static DiagnosticDescriptor ConflictingMethodsDescriptor => new("CSR0001", "Conflicting methods", "The class '{0}' contains multiple methods which map to the route method '{1}'.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor InvalidOptionsDescriptor => new("CSR0002", "Invalid options", "Value for the option '{0}' is invalid. {1}", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor InvalidIdentifierDescriptor => new("CSR0003", "Invalid identifier", "The text '{0}' is not a valid C# identifier.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor ConflictingControllerDescriptor => new("CSR0004", "Conflicting Controller", "The controller '{0}' conflicts with another controller of the same name.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor ConflictingPageClassDescriptor => new("CSR0005", "Conflicting PageModel Class", "The page class '{0}' conflicts with another page class with the same resulting name.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor UnsupportedLanguageVersionDescriptor => new("CSR0006", "Unsupported Language Version", "C# 8 or later is required for route generation.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
  }
}
