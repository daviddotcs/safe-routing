﻿//HintName: PageRoutes.g.cs
// <auto-generated/>

#pragma warning disable
#nullable enable

namespace Routes
{
  namespace Pages
  {
    /// <summary>
    /// Generates route values for <see cref="global::EditModel"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class Products_Edit
    {
      /// <summary>
      /// Generates route values for <see cref="global::EditModel.OnGet()"/>.
      /// </summary>
      public static Support.Pages_Products_Edit.GetRouteValues Get()
      {
        return new Support.Pages_Products_Edit.GetRouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = ""
        });
      }
    }
  }

  namespace Support.Pages_Products_Edit
  {
    /// <summary>
    /// Represents route keys for the properties of <see cref="global::EditModel"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class PropertyData
    {
      /// <summary>
      /// Route key for the property <see cref="global::EditModel.SomeProperty"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string?> SomeProperty { get; } = new global::SafeRouting.RouteKey<PropertyData, string?>("SomeProperty");
    }

    /// <summary>
    /// Represents route values for routes to <see cref="global::EditModel.OnGet()"/>.
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
      public string PageName => "/Products/Edit";
      /// <summary>
      /// The HTTP method used for the route.
      /// </summary>
      public global::System.Net.Http.HttpMethod HttpMethod => global::System.Net.Http.HttpMethod.Get;
      /// <summary>
      /// The name of the handler for the route.
      /// </summary>
      public string? HandlerName => null;
      /// <summary>
      /// Values for the route.
      /// </summary>
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; }

      /// <summary>
      /// Properties of <see cref="global::EditModel"/> which can be used in the route.
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
      public string? this[global::SafeRouting.RouteKey<PropertyData, string?> key] { set => RouteValues[key.Name] = value; }
    }
  }
}
