using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SafeRouting.Generator
{
  internal sealed class CandidateClassInfo
  {
    public CandidateClassInfo(TypeDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol classSymbol, SemanticModel semanticModel, bool isController, bool isPage)
    {
      TypeDeclarationSyntax = typeDeclarationSyntax;
      ClassSymbol = classSymbol;
      SemanticModel = semanticModel;
      IsController = isController;
      IsPage = isPage;
    }

    public TypeDeclarationSyntax TypeDeclarationSyntax { get; }
    public INamedTypeSymbol ClassSymbol { get; }
    public SemanticModel SemanticModel { get; }
    public bool IsController { get; }
    public bool IsPage { get; }
  }

  internal interface IMvcObjectInfo
  {
    string RouteValue { get; }
    string OutputClassName { get; }
    string? Area { get; }
    string FullyQualifiedTypeName { get; }
    string Noun { get; }
    string DivisionName { get; }
    IReadOnlyCollection<MvcPropertyInfo> Properties { get; }
    IReadOnlyCollection<IMvcMethodInfo> Methods { get; }
  }

  internal sealed class ControllerInfo : IMvcObjectInfo
  {
    public ControllerInfo(string routeValue, string name, string? area, string fullyQualifiedTypeName, TypeDeclarationSyntax typeDeclarationSyntax, IReadOnlyCollection<MvcPropertyInfo> properties, IReadOnlyCollection<ControllerMethodInfo> methods)
    {
      RouteValue = routeValue;
      Name = name;
      Area = area;
      FullyQualifiedTypeName = fullyQualifiedTypeName;
      TypeDeclarationSyntax = typeDeclarationSyntax;
      Properties = properties;
      Methods = methods;
    }

    public string RouteValue { get; }
    public string Name { get; }
    public string? Area { get; }
    public string FullyQualifiedTypeName { get; }
    public TypeDeclarationSyntax TypeDeclarationSyntax { get; }
    public IReadOnlyCollection<MvcPropertyInfo> Properties { get; }
    public IReadOnlyCollection<ControllerMethodInfo> Methods { get; }

    string IMvcObjectInfo.OutputClassName => Name;
    string IMvcObjectInfo.Noun => "Controller";
    string IMvcObjectInfo.DivisionName => "Action";
    IReadOnlyCollection<IMvcMethodInfo> IMvcObjectInfo.Methods => Methods;
  }

  internal sealed class PageInfo : IMvcObjectInfo
  {
    public PageInfo(string routeValue, string name, string? area, string pageNamespace, string fullyQualifiedTypeName, TypeDeclarationSyntax typeDeclarationSyntax, IReadOnlyCollection<MvcPropertyInfo> properties, IReadOnlyCollection<PageMethodInfo> methods)
    {
      RouteValue = routeValue;
      Name = name;
      Area = area;
      PageNamespace = pageNamespace;
      FullyQualifiedTypeName = fullyQualifiedTypeName;
      TypeDeclarationSyntax = typeDeclarationSyntax;
      Properties = properties;
      Methods = methods;
    }

    public string RouteValue { get; }
    public string Name { get; }
    public string? Area { get; }
    public string PageNamespace { get; }
    public string FullyQualifiedTypeName { get; }
    public TypeDeclarationSyntax TypeDeclarationSyntax { get; }
    public IReadOnlyCollection<MvcPropertyInfo> Properties { get; }
    public IReadOnlyCollection<PageMethodInfo> Methods { get; }

    string IMvcObjectInfo.OutputClassName => $"{(PageNamespace.Length > 0 ? $"{PageNamespace}_" : null)}{Name}";
    string IMvcObjectInfo.Noun => "Page";
    string IMvcObjectInfo.DivisionName => "Handler";
    IReadOnlyCollection<IMvcMethodInfo> IMvcObjectInfo.Methods => Methods;
  }

  internal sealed class MvcPropertyInfo
  {
    public MvcPropertyInfo(string originalName, string name, TypeInfo type, MvcBindingSourceInfo bindingSource)
    {
      OriginalName = originalName;
      Name = name;
      Type = type;
      BindingSource = bindingSource;
    }

    public string OriginalName { get; }
    public string Name { get; }
    public TypeInfo Type { get; }
    public MvcBindingSourceInfo BindingSource { get; }
  }

  internal interface IMvcMethodInfo
  {
    string EscapedName { get; }
    string UniqueName { get; }
    string? DivisionRouteValue { get; }
    string? Area { get; }
    string FullyQualifiedMethodDeclaration { get; }
    IReadOnlyCollection<MvcMethodParameterInfo> Parameters { get; }

    IEnumerable<MvcMethodParameterInfo> GetUrlParameters();
  }

  internal sealed class ControllerMethodInfo : IMvcMethodInfo
  {
    public ControllerMethodInfo(string name, string escapedName, string uniqueName, string actionName, string? area, string fullyQualifiedMethodDeclaration, IReadOnlyCollection<MvcMethodParameterInfo> parameters)
    {
      Name = name;
      EscapedName = escapedName;
      UniqueName = uniqueName;
      ActionName = actionName;
      Area = area;
      FullyQualifiedMethodDeclaration = fullyQualifiedMethodDeclaration;
      Parameters = parameters;
    }

    public string Name { get; }
    public string EscapedName { get; }
    public string UniqueName { get; }
    public string ActionName { get; }
    public string? Area { get; }
    public string FullyQualifiedMethodDeclaration { get; }
    public IReadOnlyCollection<MvcMethodParameterInfo> Parameters { get; }

    string IMvcMethodInfo.DivisionRouteValue => ActionName;

    public IEnumerable<MvcMethodParameterInfo> GetUrlParameters()
      => Parameters.Where(x => x.AffectsUrl());
  }

  internal sealed class PageMethodInfo : IMvcMethodInfo
  {
    public PageMethodInfo(string name, string uniqueName, string? handlerName, string fullyQualifiedMethodDeclaration, IReadOnlyCollection<MvcMethodParameterInfo> parameters)
    {
      Name = name;
      UniqueName = uniqueName;
      HandlerName = handlerName;
      FullyQualifiedMethodDeclaration = fullyQualifiedMethodDeclaration;
      Parameters = parameters;
    }

    public string Name { get; }
    public string UniqueName { get; }
    public string? HandlerName { get; }
    public string FullyQualifiedMethodDeclaration { get; }
    public IReadOnlyCollection<MvcMethodParameterInfo> Parameters { get; }

    string IMvcMethodInfo.EscapedName => Name;
    string? IMvcMethodInfo.DivisionRouteValue => HandlerName;
    string? IMvcMethodInfo.Area => null;

    public IEnumerable<MvcMethodParameterInfo> GetUrlParameters()
      => Parameters.Where(x => x.AffectsUrl());
  }

  internal sealed class MvcMethodParameterInfo
  {
    public MvcMethodParameterInfo(string originalName, string name, TypeInfo type, bool hasExplicitDefault, object? explicitDefaultValue, MvcBindingSourceInfo? bindingSource)
    {
      OriginalName = originalName;
      Name = name;
      Type = type;
      HasExplicitDefault = hasExplicitDefault;
      ExplicitDefaultValue = explicitDefaultValue;
      BindingSource = bindingSource;
    }

    public string OriginalName { get; }
    public string Name { get; }
    public TypeInfo Type { get; }
    public bool HasExplicitDefault { get; }
    public object? ExplicitDefaultValue { get; }
    public MvcBindingSourceInfo? BindingSource { get; }

    public bool AffectsUrl()
      => BindingSource?.AffectsUrl() ?? true;
  }

  internal sealed class MvcBindingSourceInfo
  {
    public MvcBindingSourceInfo(MvcBindingSourceType sourceType, string? name = null)
    {
      SourceType = sourceType;
      Name = name;
    }

    public MvcBindingSourceType SourceType { get; }
    public string? Name { get; }

    public bool AffectsUrl() => SourceType switch
    {
      MvcBindingSourceType.Query => true,
      MvcBindingSourceType.Route => true,
      MvcBindingSourceType.Custom => true,
      _ => false
    };
  }

  internal enum MvcBindingSourceType
  {
    Body,
    Form,
    Header,
    Query,
    Route,
    Custom
  }

  internal sealed class TypeInfo
  {
    public TypeInfo(string fullyQualifiedName, string fullyQualifiedNameSansAnnotations, bool annotationsEnabled)
    {
      FullyQualifiedName = fullyQualifiedName;
      FullyQualifiedNameSansAnnotations = fullyQualifiedNameSansAnnotations;
      AnnotationsEnabled = annotationsEnabled;
    }

    public string FullyQualifiedName { get; }
    public string FullyQualifiedNameSansAnnotations { get; }
    public bool AnnotationsEnabled { get; }
  }

  internal sealed class GeneratorOptions
  {
    public GeneratorOptions(string generatedAccessModifier, string generatedNamespace, Dictionary<string, string> optionErrors)
    {
      GeneratedAccessModifier = generatedAccessModifier;
      GeneratedNamespace = generatedNamespace;
      OptionErrors = optionErrors;
    }

    public string GeneratedAccessModifier { get; }
    public string GeneratedNamespace { get; }
    public Dictionary<string, string> OptionErrors { get; }
  }
}
