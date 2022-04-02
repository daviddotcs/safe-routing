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
      /// Generates route values for <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
      /// </summary>
      public static Support.Controllers_Products.IndexRouteValues Index(global::System.DateTime a = default(global::System.DateTime), int b = 5, string c = "he\"llo\\", global::System.DayOfWeek d = global::System.DayOfWeek.Wednesday, bool e = false, bool f = true, string g = "ProductsController", char h = '?', char i = (char)42, char j = '\'', char k = '\\')
      {
        return new Support.Controllers_Products.IndexRouteValues(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()
        {
          ["area"] = "",
          ["a"] = a,
          ["b"] = b,
          ["c"] = c,
          ["d"] = d,
          ["e"] = e,
          ["f"] = f,
          ["g"] = g,
          ["h"] = h,
          ["i"] = i,
          ["j"] = j,
          ["k"] = k
        });
      }
    }
  }
  
  namespace Support.Controllers_Products
  {
    /// <summary>
    /// Represents route values for routes to <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
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
      
      /// <summary>
      /// Parameters of <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/> which can be used in the route.
      /// </summary>
      public Index.ParameterData Parameters { get; } = new Index.ParameterData();
      /// <summary>
      /// Removes a parameter value from the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise <see langword="false"/>.</returns>
      public bool Remove<T>(global::SafeRouting.RouteKey<Index.ParameterData, T> key) => RouteValues.Remove(key.Name);
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <typeparam name="T">The type of values applicable to the key.</typeparam>
      /// <param name="key">The key for the route.</param>
      /// <param name="value">The value for the route.</param>
      public void Set<T>(global::SafeRouting.RouteKey<Index.ParameterData, T> key, T value) => RouteValues[key.Name] = value;
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public bool this[global::SafeRouting.RouteKey<Index.ParameterData, bool> key] { set => RouteValues[key.Name] = value; }
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public char this[global::SafeRouting.RouteKey<Index.ParameterData, char> key] { set => RouteValues[key.Name] = value; }
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public global::System.DateTime this[global::SafeRouting.RouteKey<Index.ParameterData, global::System.DateTime> key] { set => RouteValues[key.Name] = value; }
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public global::System.DayOfWeek this[global::SafeRouting.RouteKey<Index.ParameterData, global::System.DayOfWeek> key] { set => RouteValues[key.Name] = value; }
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public int this[global::SafeRouting.RouteKey<Index.ParameterData, int> key] { set => RouteValues[key.Name] = value; }
      /// <summary>
      /// Sets a parameter value for the route.
      /// </summary>
      /// <param name="key">The key for the route.</param>
      public string this[global::SafeRouting.RouteKey<Index.ParameterData, string> key] { set => RouteValues[key.Name] = value; }
    }
    
    namespace Index
    {
      /// <summary>
      /// Represents route keys for parameters to <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
      /// </summary>
      [global::System.CodeDom.Compiler.GeneratedCode("SafeRouting.Generator", "1.0.0.0")]
      [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      public sealed class ParameterData
      {
        /// <summary>
        /// Route key for the <c>a</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, global::System.DateTime> A { get; } = new global::SafeRouting.RouteKey<ParameterData, global::System.DateTime>("a");
        
        /// <summary>
        /// Route key for the <c>b</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, int> B { get; } = new global::SafeRouting.RouteKey<ParameterData, int>("b");
        
        /// <summary>
        /// Route key for the <c>c</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, string> C { get; } = new global::SafeRouting.RouteKey<ParameterData, string>("c");
        
        /// <summary>
        /// Route key for the <c>d</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, global::System.DayOfWeek> D { get; } = new global::SafeRouting.RouteKey<ParameterData, global::System.DayOfWeek>("d");
        
        /// <summary>
        /// Route key for the <c>e</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, bool> E { get; } = new global::SafeRouting.RouteKey<ParameterData, bool>("e");
        
        /// <summary>
        /// Route key for the <c>f</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, bool> F { get; } = new global::SafeRouting.RouteKey<ParameterData, bool>("f");
        
        /// <summary>
        /// Route key for the <c>g</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, string> G { get; } = new global::SafeRouting.RouteKey<ParameterData, string>("g");
        
        /// <summary>
        /// Route key for the <c>h</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, char> H { get; } = new global::SafeRouting.RouteKey<ParameterData, char>("h");
        
        /// <summary>
        /// Route key for the <c>i</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, char> I { get; } = new global::SafeRouting.RouteKey<ParameterData, char>("i");
        
        /// <summary>
        /// Route key for the <c>j</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, char> J { get; } = new global::SafeRouting.RouteKey<ParameterData, char>("j");
        
        /// <summary>
        /// Route key for the <c>k</c> parameter in <see cref="global::ProductsController.Index(global::System.DateTime, int, string, global::System.DayOfWeek, bool, bool, string, char, char, char, char)"/>.
        /// </summary>
        public global::SafeRouting.RouteKey<ParameterData, char> K { get; } = new global::SafeRouting.RouteKey<ParameterData, char>("k");
      }
    }
  }
}
