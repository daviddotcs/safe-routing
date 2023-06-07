﻿//HintName: PageRoutes.g.cs
// <auto-generated/>

#pragma warning disable
#nullable enable

namespace Routes
{
  namespace Pages
  {
    /// <summary>
    /// Generates route values for <see cref="global::IndexModel"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class Index
    {
      /// <summary>
      /// Generates route values for <see cref="global::IndexModel.OnGet()"/>.
      /// </summary>
      public static Support.Pages_Index.GetRouteValues Get()
      {
        return new Support.Pages_Index.GetRouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = ""
        });
      }
    }
  }

  namespace Support.Pages_Index
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::IndexModel.OnGet()"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class GetRouteValues : global::SafeRouting.IPageRouteValues
    {
      /// <summary>
      /// Initialises a new instance of the <see cref="GetRouteValues"/> class.
      /// </summary>
      /// <param name="routeValues">The initial values for the route.</param>
      public GetRouteValues(global::Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues)
      {
        RouteValues = routeValues;
      }

      /// <summary>
      /// The name of the page for the route.
      /// </summary>
      public string PageName => "/Index";
      /// <summary>
      /// The name of the handler for the route.
      /// </summary>
      public string? HandlerName => null;
      /// <summary>
      /// Values for the route.
      /// </summary>
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; }
    }
  }
}
