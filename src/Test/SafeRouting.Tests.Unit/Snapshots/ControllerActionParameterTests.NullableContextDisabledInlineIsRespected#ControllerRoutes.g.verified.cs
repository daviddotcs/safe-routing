﻿//HintName: ControllerRoutes.g.cs
// <auto-generated/>

#pragma warning disable
#nullable enable

namespace Routes
{
  namespace Controllers
  {
    /// <summary>
    /// Generates route values for <see cref="global::ProductsController"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class Products
    {
      /// <summary>
      /// Generates route values for <see cref="global::ProductsController.A(global::System.Collections.Generic.IEnumerable{string}, string)"/>.
      /// </summary>
      public static Support.Controllers_Products.ARouteValues A(
#nullable disable
        global::System.Collections.Generic.IEnumerable<string> x, string y
#nullable restore
      )
      {
        return new Support.Controllers_Products.ARouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = "",
          ["x"] = x,
          ["y"] = y
        });
      }
    }
  }

  namespace Support.Controllers_Products
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::ProductsController.A(global::System.Collections.Generic.IEnumerable{string}, string)"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class ARouteValues : global::SafeRouting.IControllerRouteValues
    {
      /// <summary>
      /// Initialises a new instance of the <see cref="ARouteValues"/> class.
      /// </summary>
      /// <param name="routeValues">The initial values for the route.</param>
      public ARouteValues(global::Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues)
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
      public string ActionName => "A";
      /// <summary>
      /// Values for the route.
      /// </summary>
      public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; }

      /// <summary>
      /// Parameters of <see cref="global::ProductsController.A(global::System.Collections.Generic.IEnumerable{string}, string)"/> which can be used in the route.
      /// </summary>
      public A.ParameterData Parameters { get; } = new A.ParameterData();
      /// <summary>
      /// Removes a parameter value from the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
      public bool Remove<T>(global::SafeRouting.RouteKey<A.ParameterData, T> key) => RouteValues.Remove(key.Name);
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <param name="value">The value for the route.</param>
      public void Set<T>(global::SafeRouting.RouteKey<A.ParameterData, T> key, T value) => RouteValues[key.Name] = value;
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
#nullable disable
      public global::System.Collections.Generic.IEnumerable<string> this[global::SafeRouting.RouteKey<A.ParameterData, global::System.Collections.Generic.IEnumerable<string>> key] { set => RouteValues[key.Name] = value; }
#nullable restore
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
#nullable disable
      public string this[global::SafeRouting.RouteKey<A.ParameterData, string> key] { set => RouteValues[key.Name] = value; }
#nullable restore
    }

    namespace A
    {
      /// <summary>
      /// Represents route keys for parameters to <see cref="global::ProductsController.A(global::System.Collections.Generic.IEnumerable{string}, string)"/>.
      /// </summary>
      [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
      [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      public sealed class ParameterData
      {
        /// <summary>
        /// Route key for the <c>x</c> parameter in <see cref="global::ProductsController.A(global::System.Collections.Generic.IEnumerable{string}, string)"/>.
        /// </summary>
#nullable disable
        public global::SafeRouting.RouteKey<ParameterData, global::System.Collections.Generic.IEnumerable<string>> X { get; } = new global::SafeRouting.RouteKey<ParameterData, global::System.Collections.Generic.IEnumerable<string>>("x");
#nullable restore

        /// <summary>
        /// Route key for the <c>y</c> parameter in <see cref="global::ProductsController.A(global::System.Collections.Generic.IEnumerable{string}, string)"/>.
        /// </summary>
#nullable disable
        public global::SafeRouting.RouteKey<ParameterData, string> Y { get; } = new global::SafeRouting.RouteKey<ParameterData, string>("y");
#nullable restore
      }
    }
  }
}
