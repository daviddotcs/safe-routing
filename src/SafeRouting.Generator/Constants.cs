namespace SafeRouting.Generator;

internal static class AspNetClassNames
{
  public const string ActionNameAttribute = "Microsoft.AspNetCore.Mvc.ActionNameAttribute";
  public const string AreaAttribute = "Microsoft.AspNetCore.Mvc.AreaAttribute";
  public const string BindPropertiesAttribute = "Microsoft.AspNetCore.Mvc.BindPropertiesAttribute";
  public const string BindPropertyAttribute = "Microsoft.AspNetCore.Mvc.BindPropertyAttribute";
  public const string CancellationToken = "System.Threading.CancellationToken";
  public const string Controller = "Microsoft.AspNetCore.Mvc.Controller";
  public const string ControllerAttribute = "Microsoft.AspNetCore.Mvc.ControllerAttribute";
  public const string ControllerBase = "Microsoft.AspNetCore.Mvc.ControllerBase";
  public const string FromBodyAttribute = "Microsoft.AspNetCore.Mvc.FromBodyAttribute";
  public const string FromFormAttribute = "Microsoft.AspNetCore.Mvc.FromFormAttribute";
  public const string FromHeaderAttribute = "Microsoft.AspNetCore.Mvc.FromHeaderAttribute";
  public const string FromQueryAttribute = "Microsoft.AspNetCore.Mvc.FromQueryAttribute";
  public const string FromRouteAttribute = "Microsoft.AspNetCore.Mvc.FromRouteAttribute";
  public const string FromServicesAttribute = "Microsoft.AspNetCore.Mvc.FromServicesAttribute";
  public const string NonActionAttribute = "Microsoft.AspNetCore.Mvc.NonActionAttribute";
  public const string NonControllerAttribute = "Microsoft.AspNetCore.Mvc.NonControllerAttribute";
  public const string NonHandlerAttribute = "Microsoft.AspNetCore.Mvc.RazorPages.NonHandlerAttribute";
  public const string PageModel = "Microsoft.AspNetCore.Mvc.RazorPages.PageModel";
}

internal static class GeneratorClassNames
{
  public const string ExcludeFromRouteGeneratorAttribute = $"{GeneratorSupport.RootNamespace}.ExcludeFromRouteGeneratorAttribute";
  public const string RouteGeneratorNameAttribute = $"{GeneratorSupport.RootNamespace}.RouteGeneratorNameAttribute";
}

internal static class GeneratorSupport
{
  public const string DefaultGeneratedAccessModifier = "public";
  public const string DefaultGeneratedRootNamespace = "Routes";
  public const string GeneratedAccessModifierOption = $"{GlobalOptionsPrefix}.generated_access_modifier";
  public const string GeneratedNamespaceOption = $"{GlobalOptionsPrefix}.generated_namespace";
  public const string GlobalOptionsPrefix = "safe_routing";
  public const string RootNamespace = "SafeRouting";
}
