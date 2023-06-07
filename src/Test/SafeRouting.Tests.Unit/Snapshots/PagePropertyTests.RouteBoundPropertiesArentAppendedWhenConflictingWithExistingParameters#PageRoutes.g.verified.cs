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
      /// Generates route values for <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
      /// </summary>
      public static Support.Pages_Products_Edit.GetRouteValues Get(string? a, string? x, string? c, string? d, int y)
      {
        return new Support.Pages_Products_Edit.GetRouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = "",
          ["a"] = a,
          ["B"] = x,
          ["c"] = c,
          ["d"] = d,
          ["y"] = y
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
      /// Route key for the property <see cref="global::EditModel.A"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string?> A { get; } = new global::SafeRouting.RouteKey<PropertyData, string?>("A");

      /// <summary>
      /// Route key for the property <see cref="global::EditModel.B"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string?> B { get; } = new global::SafeRouting.RouteKey<PropertyData, string?>("B");

      /// <summary>
      /// Route key for the property <see cref="global::EditModel.SomeProperty"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string?> SomeProperty { get; } = new global::SafeRouting.RouteKey<PropertyData, string?>("C");

      /// <summary>
      /// Route key for the property <see cref="global::EditModel.OtherProperty"/>.
      /// </summary>
      public global::SafeRouting.RouteKey<PropertyData, string?> D { get; } = new global::SafeRouting.RouteKey<PropertyData, string?>("OtherProperty");
    }

    /// <summary>
    /// Represents route values for routes to <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
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

      /// <summary>
      /// Parameters of <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/> which can be used in the route.
      /// </summary>
      public Get.ParameterData Parameters { get; } = new Get.ParameterData();
      /// <summary>
      /// Removes a parameter value from the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
      public bool Remove<T>(global::SafeRouting.RouteKey<Get.ParameterData, T> key) => RouteValues.Remove(key.Name);
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <param name="value">The value for the route.</param>
      public void Set<T>(global::SafeRouting.RouteKey<Get.ParameterData, T> key, T value) => RouteValues[key.Name] = value;
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public int this[global::SafeRouting.RouteKey<Get.ParameterData, int> key] { set => RouteValues[key.Name] = value; }
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public string? this[global::SafeRouting.RouteKey<Get.ParameterData, string?> key] { set => RouteValues[key.Name] = value; }
    }

    namespace Get
    {
      /// <summary>
      /// Represents route keys for parameters to <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
      /// </summary>
      [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
      [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      public sealed class ParameterData
      {
        /// <summary>
        /// Route key for the <c>a</c> parameter in <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, string?> A { get; } = new global::SafeRouting.RouteKey<ParameterData, string?>("a");

        /// <summary>
        /// Route key for the <c>x</c> parameter in <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, string?> X { get; } = new global::SafeRouting.RouteKey<ParameterData, string?>("B");

        /// <summary>
        /// Route key for the <c>c</c> parameter in <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, string?> C { get; } = new global::SafeRouting.RouteKey<ParameterData, string?>("c");

        /// <summary>
        /// Route key for the <c>d</c> parameter in <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, string?> D { get; } = new global::SafeRouting.RouteKey<ParameterData, string?>("d");

        /// <summary>
        /// Route key for the <c>y</c> parameter in <see cref="global::EditModel.OnGet(string?, string?, string?, string?, int)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, int> Y { get; } = new global::SafeRouting.RouteKey<ParameterData, int>("y");
      }
    }
  }
}
