﻿//HintName: ControllerRoutes.g.cs
// <auto-generated/>

#nullable enable

namespace Routes
{
  namespace Controllers
  {
    /// <summary>
    /// Generates route values for <see cref="global::a.ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    public static class Products
    {
      /// <summary>
      /// Generates route values for <see cref="global::a.ProductsController.Index()"/>.
      /// </summary>
      public static Support.Controllers_Products.IndexRouteValues Index()
      {
        var routeInfo = new Support.Controllers_Products.IndexRouteValues();
        routeInfo.RouteValues["area"] = "";
        return routeInfo;
      }
    }
  }
  
  namespace Support.Controllers_Products
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::a.ProductsController.Index()"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    public sealed class IndexRouteValues : global::SafeRouting.IControllerRouteValues
    {
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
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; } = new global::Microsoft.AspNetCore.Routing.RouteValueDictionary();
    }
  }
  
  namespace Areas.Other.Controllers
  {
    /// <summary>
    /// Generates route values for <see cref="global::B.ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    public static class Products
    {
      /// <summary>
      /// Generates route values for <see cref="global::B.ProductsController.Index()"/>.
      /// </summary>
      public static Support.Other_Controllers_Products.IndexRouteValues Index()
      {
        var routeInfo = new Support.Other_Controllers_Products.IndexRouteValues();
        routeInfo.RouteValues["area"] = "Other";
        return routeInfo;
      }
    }
  }
  
  namespace Support.Other_Controllers_Products
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::B.ProductsController.Index()"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    public sealed class IndexRouteValues : global::SafeRouting.IControllerRouteValues
    {
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
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; } = new global::Microsoft.AspNetCore.Routing.RouteValueDictionary();
    }
  }
}