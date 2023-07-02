﻿//HintName: ControllerRoutes.g.cs
// <auto-generated/>

#pragma warning disable
#nullable enable

namespace Routes
{
  namespace Controllers
  {
    /// <summary>
    /// Generates route values for <see cref="global::a.ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class Products
    {
      /// <summary>
      /// Generates route values for <see cref="global::a.ProductsController.Index()"/>.
      /// </summary>
      public static Support.Controllers_Products.IndexRouteValues Index()
      {
        return new Support.Controllers_Products.IndexRouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = ""
        });
      }
    }
  }

  namespace Support.Controllers_Products
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::a.ProductsController.Index()"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class IndexRouteValues : global::SafeRouting.IControllerRouteValues
    {
      /// <summary>
      /// Initialises a new instance of the <see cref="IndexRouteValues"/> class.
      /// </summary>
      /// <param name="routeValues">The initial values for the route.</param>
      public IndexRouteValues(global::Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues)
      {
        RouteValues = routeValues;
      }

      /// <summary>
      /// The name of the controller for the route.
      /// </summary>
      public string ControllerName => "Products";
      /// <summary>
      /// The name of the action for the route.
      /// </summary>
      public string ActionName => "Index";
      /// <summary>
      /// Values for the route.
      /// </summary>
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; }
    }
  }

  namespace Areas.Other.Controllers
  {
    /// <summary>
    /// Generates route values for <see cref="global::B.ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class Products
    {
      /// <summary>
      /// Generates route values for <see cref="global::B.ProductsController.Index()"/>.
      /// </summary>
      public static Support.Other_Controllers_Products.IndexRouteValues Index()
      {
        return new Support.Other_Controllers_Products.IndexRouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = "Other"
        });
      }
    }
  }

  namespace Support.Other_Controllers_Products
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::B.ProductsController.Index()"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class IndexRouteValues : global::SafeRouting.IControllerRouteValues
    {
      /// <summary>
      /// Initialises a new instance of the <see cref="IndexRouteValues"/> class.
      /// </summary>
      /// <param name="routeValues">The initial values for the route.</param>
      public IndexRouteValues(global::Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues)
      {
        RouteValues = routeValues;
      }

      /// <summary>
      /// The name of the controller for the route.
      /// </summary>
      public string ControllerName => "Products";
      /// <summary>
      /// The name of the action for the route.
      /// </summary>
      public string ActionName => "Index";
      /// <summary>
      /// Values for the route.
      /// </summary>
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; }
    }
  }
}