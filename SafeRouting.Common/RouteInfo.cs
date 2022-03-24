using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace SafeRouting;

/// <summary>
/// Represents route values for an arbitrary route.
/// </summary>
public interface IRouteValues
{
  /// <summary>
  /// A collection of key/value pairs for the route.
  /// </summary>
  RouteValueDictionary RouteValues { get; }
}

/// <summary>
/// Represents route values for a <see cref="Controller"/>'s action method.
/// </summary>
public interface IControllerRouteValues : IRouteValues
{
  /// <summary>
  /// The name of the controller.
  /// </summary>
  string ControllerName { get; }
  /// <summary>
  /// The name of the action method.
  /// </summary>
  string ActionName { get; }
}

/// <summary>
/// Represents route values for a <see cref="PageModel"/>'s handler method.
/// </summary>
public interface IPageRouteValues : IRouteValues
{
  /// <summary>
  /// The name of the page.
  /// </summary>
  string PageName { get; }
  /// <summary>
  /// The name of the handler.
  /// </summary>
  string? HandlerName { get; }
}

/// <summary>
/// Represents a strongly-typed key within a route.
/// </summary>
/// <typeparam name="TScope">A type representing the scope in which the key is valid.</typeparam>
/// <typeparam name="TValue">The type of values which can be used with this key.</typeparam>
public sealed class RouteKey<TScope, TValue>
{
  /// <summary>
  /// Creates a route key with the specified name.
  /// </summary>
  /// <param name="name">The name of the key</param>
  public RouteKey(string name)
  {
    Name = name;
  }

  /// <summary>
  /// The name of the key.
  /// </summary>
  public string Name { get; }
}
