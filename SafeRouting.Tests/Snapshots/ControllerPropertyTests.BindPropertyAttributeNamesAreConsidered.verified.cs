﻿//HintName: ControllerRoutes.g.cs
// <auto-generated/>

#nullable enable

namespace Routes
{
  namespace Controllers
  {
    /// <summary>
    /// Generates route values for <see cref="global::ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    public static class Products
    {
      /// <summary>
      /// Generates route values for <see cref="global::ProductsController.Index()"/>.
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
    /// Represents route keys for the properties of <see cref="global::ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    public sealed class PropertyData
    {
      /// <summary>
      /// Route key for the property <see cref="global::ProductsController.BindProperty"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string> BindProperty { get; } = new global::SafeRouting.RouteKey<PropertyData, string>("RenamedBindProperty");
      
      /// <summary>
      /// Route key for the property <see cref="global::ProductsController.FromForm"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string> FromForm { get; } = new global::SafeRouting.RouteKey<PropertyData, string>("RenamedFromForm");
      
      /// <summary>
      /// Route key for the property <see cref="global::ProductsController.FromHeader"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string> FromHeader { get; } = new global::SafeRouting.RouteKey<PropertyData, string>("RenamedFromHeader");
      
      /// <summary>
      /// Route key for the property <see cref="global::ProductsController.FromQuery"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string> FromQuery { get; } = new global::SafeRouting.RouteKey<PropertyData, string>("RenamedFromQuery");
      
      /// <summary>
      /// Route key for the property <see cref="global::ProductsController.FromRoute"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string> FromRoute { get; } = new global::SafeRouting.RouteKey<PropertyData, string>("RenamedFromRoute");
    }
    
    /// <summary>
    /// Represents route values for routes to <see cref="global::ProductsController.Index()"/>.
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
      
      /// <summary>
      /// Properties of <see cref="global::ProductsController"/> which can be used in the route.
      /// </summary>
      public PropertyData Properties { get; } = new PropertyData();
      /// <summary>
      /// Removes a property value from the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
      public bool Remove<T>(global::SafeRouting.RouteKey<PropertyData, T> key) => RouteValues.Remove(key.Name);
      /// <summary>
      /// Sets a property value for the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <param name="value">The value for the route.</param>
      public void Set<T>(global::SafeRouting.RouteKey<PropertyData, T> key, T value) => RouteValues[key.Name] = value;
      /// <summary>
      /// Sets a property value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public string this[global::SafeRouting.RouteKey<PropertyData, string> key] { set => RouteValues[key.Name] = value; }
    }
  }
}