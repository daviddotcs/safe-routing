using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SafeRouting.Generator;

internal sealed record CandidateClassInfo(
  TypeDeclarationSyntax TypeDeclarationSyntax,
  INamedTypeSymbol TypeSymbol,
  SemanticModel SemanticModel,
  bool IsController,
  bool IsPage);

internal sealed class CandidateClassInfoEqualityComparer : IEqualityComparer<CandidateClassInfo>
{
  public bool Equals(CandidateClassInfo x, CandidateClassInfo y)
    => Comparer.Equals(x.TypeDeclarationSyntax, y.TypeDeclarationSyntax);

  public int GetHashCode(CandidateClassInfo obj) => obj.GetHashCode();

  public static CandidateClassInfoEqualityComparer Default { get; } = new CandidateClassInfoEqualityComparer();

  private static readonly EqualityComparer<TypeDeclarationSyntax> Comparer = EqualityComparer<TypeDeclarationSyntax>.Default;
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

internal sealed record ControllerInfo(
  string RouteValue,
  string Name,
  string? Area,
  string FullyQualifiedTypeName,
  TypeDeclarationSyntax TypeDeclarationSyntax,
  IReadOnlyCollection<MvcPropertyInfo> Properties,
  IReadOnlyCollection<ControllerMethodInfo> Methods) : IMvcObjectInfo
{
  string IMvcObjectInfo.OutputClassName => Name;
  string IMvcObjectInfo.Noun => "Controller";
  string IMvcObjectInfo.DivisionName => "Action";
  IReadOnlyCollection<IMvcMethodInfo> IMvcObjectInfo.Methods => Methods;
}

internal sealed record PageInfo(
  string RouteValue,
  string Name,
  string? Area,
  string PageNamespace,
  string FullyQualifiedTypeName,
  TypeDeclarationSyntax TypeDeclarationSyntax,
  IReadOnlyCollection<MvcPropertyInfo> Properties,
  IReadOnlyCollection<PageMethodInfo> Methods) : IMvcObjectInfo
{
  string IMvcObjectInfo.OutputClassName => $"{(PageNamespace.Length > 0 ? $"{PageNamespace}_" : null)}{Name}";
  string IMvcObjectInfo.Noun => "Page";
  string IMvcObjectInfo.DivisionName => "Handler";
  IReadOnlyCollection<IMvcMethodInfo> IMvcObjectInfo.Methods => Methods;
}

internal sealed record MvcPropertyInfo(
  string OriginalName,
  string EscapedOriginalName,
  string EscapedName,
  string RouteKey,
  TypeInfo Type,
  MvcBindingSourceInfo BindingSource)
{
  public bool AffectsUrl()
    => BindingSource.AffectsUrl(forParameter: false);
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

internal sealed record ControllerMethodInfo(
  string Name,
  string EscapedName,
  string UniqueName,
  string ActionName,
  string? Area,
  string FullyQualifiedMethodDeclaration,
  IReadOnlyCollection<MvcMethodParameterInfo> Parameters) : IMvcMethodInfo
{
  string IMvcMethodInfo.DivisionRouteValue => ActionName;

  public IEnumerable<MvcMethodParameterInfo> GetUrlParameters()
    => Parameters.Where(x => x.AffectsUrl());
}

internal sealed record PageMethodInfo(
  string Name,
  string EscapedName,
  string UniqueName,
  string? HandlerName,
  string FullyQualifiedMethodDeclaration,
  IReadOnlyCollection<MvcMethodParameterInfo> Parameters) : IMvcMethodInfo
{
  string? IMvcMethodInfo.DivisionRouteValue => HandlerName;
  string? IMvcMethodInfo.Area => null;

  public IEnumerable<MvcMethodParameterInfo> GetUrlParameters()
    => Parameters.Where(x => x.AffectsUrl());
}

internal sealed record MvcMethodParameterInfo(
  string OriginalName,
  string EscapedName,
  string? PropertyName,
  string RouteKey,
  TypeInfo Type,
  ExpressionSyntax? DefaultValueExpression,
  MvcBindingSourceInfo? BindingSource)
{
  public bool AffectsUrl()
    => BindingSource?.AffectsUrl(forParameter: true) ?? true;
}

internal sealed record MvcBindingSourceInfo(
  MvcBindingSourceType SourceType,
  string? Name = null,
  bool SupportsGet = false)
{
  public bool AffectsUrl(bool forParameter) => SourceType switch
  {
    MvcBindingSourceType.Query => true,
    MvcBindingSourceType.Route => true,
    MvcBindingSourceType.Custom => forParameter || SupportsGet,
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

internal sealed record TypeInfo(
  string FullyQualifiedName,
  string FullyQualifiedNameSansAnnotations,
  bool AnnotationsEnabled);

internal sealed record GeneratorOptions(
  string GeneratedAccessModifier,
  string GeneratedNamespace,
  IReadOnlyCollection<Diagnostic> Diagnostics);
