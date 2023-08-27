using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace SafeRouting.Extensions;

/// <summary>
/// Provides a set of <see langword="static"/> methods for working with <see cref="IControllerRouteValues"/> and <see cref="IPageRouteValues"/> instances.
/// </summary>
public static class RouteValueExtensions
{
  /// <summary>
  /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to redirect to.</param>
  /// <param name="page">The <see cref="PageModel"/>.</param>
  /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
  public static RedirectToPageResult Redirect(this IPageRouteValues route, PageModel page)
    => page.RedirectToPage(route.PageName, route.HandlerName, route.RouteValues);

  /// <summary>
  /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to redirect to.</param>
  /// <param name="page">The <see cref="PageModel"/>.</param>
  /// <returns>The <see cref="RedirectToActionResult"/>.</returns>
  public static RedirectToActionResult Redirect(this IControllerRouteValues route, PageModel page)
    => page.RedirectToAction(route.ActionName, route.ControllerName, route.RouteValues);

  /// <summary>
  /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to redirect to.</param>
  /// <param name="page">The <see cref="PageModel"/>.</param>
  /// <returns>The <see cref="ActionResult"/>.</returns>
  public static ActionResult Redirect(this IRouteValues route, PageModel page)
    => route switch
    {
      IControllerRouteValues controllerRoute => Redirect(controllerRoute, page),
      IPageRouteValues pageRoute => Redirect(pageRoute, page),
      _ => page.RedirectToRoute(route.RouteValues)
    };

  /// <summary>
  /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to redirect to.</param>
  /// <param name="controller">The <see cref="ControllerBase"/>.</param>
  /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
  public static RedirectToPageResult Redirect(this IPageRouteValues route, ControllerBase controller)
    => controller.RedirectToPage(route.PageName, route.HandlerName, route.RouteValues);

  /// <summary>
  /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to redirect to.</param>
  /// <param name="controller">The <see cref="ControllerBase"/>.</param>
  /// <returns>The <see cref="RedirectToActionResult"/>.</returns>
  public static RedirectToActionResult Redirect(this IControllerRouteValues route, ControllerBase controller)
    => controller.RedirectToAction(route.ActionName, route.ControllerName, route.RouteValues);

  /// <summary>
  /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to redirect to.</param>
  /// <param name="controller">The <see cref="ControllerBase"/>.</param>
  /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
  public static ActionResult Redirect(this IRouteValues route, ControllerBase controller)
    => route switch
    {
      IControllerRouteValues controllerRoute => Redirect(controllerRoute, controller),
      IPageRouteValues pageRoute => Redirect(pageRoute, controller),
      _ => controller.RedirectToRoute(route.RouteValues)
    };

  /// <summary>
  /// Generates a URL with a relative path for the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to generate the URL for.</param>
  /// <param name="url">The <see cref="IUrlHelper"/>.</param>
  /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
  /// <param name="host">The host name for the URL.</param>
  /// <param name="fragment">The fragment for the URL.</param>
  /// <returns>The generated URL.</returns>
  /// <remarks>The value of host should be a trusted value. Relying on the value of the current request can allow untrusted input to influence the resulting URI unless the Host header has been validated. See the deployment documentation for instructions on how to properly validate the Host header in your deployment environment.</remarks>
  public static string? Url(this IPageRouteValues route, IUrlHelper url, string? protocol = null, string? host = null, string? fragment = null)
    => url.Page(route.PageName, route.HandlerName, route.RouteValues, protocol, host, fragment);

  /// <summary>
  /// Generates a URL with a relative path for the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to generate the URL for.</param>
  /// <param name="url">The <see cref="IUrlHelper"/>.</param>
  /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
  /// <param name="host">The host name for the URL.</param>
  /// <param name="fragment">The fragment for the URL.</param>
  /// <returns>The generated URL.</returns>
  /// <remarks>The value of host should be a trusted value. Relying on the value of the current request can allow untrusted input to influence the resulting URI unless the Host header has been validated. See the deployment documentation for instructions on how to properly validate the Host header in your deployment environment.</remarks>
  public static string? Url(this IControllerRouteValues route, IUrlHelper url, string? protocol = null, string? host = null, string? fragment = null)
    => url.Action(route.ActionName, route.ControllerName, route.RouteValues, protocol, host, fragment);

  /// <summary>
  /// Generates a URL with a relative path for the specified <paramref name="route"/>.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to generate the URL for.</param>
  /// <param name="url">The <see cref="IUrlHelper"/>.</param>
  /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
  /// <param name="host">The host name for the URL.</param>
  /// <param name="fragment">The fragment for the URL.</param>
  /// <returns>The generated URL.</returns>
  /// <remarks>The value of host should be a trusted value. Relying on the value of the current request can allow untrusted input to influence the resulting URI unless the Host header has been validated. See the deployment documentation for instructions on how to properly validate the Host header in your deployment environment.</remarks>
  public static string? Url(this IRouteValues route, IUrlHelper url, string? protocol = null, string? host = null, string? fragment = null)
    => route switch
    {
      IControllerRouteValues controllerRoute => Url(controllerRoute, url, protocol, host, fragment),
      IPageRouteValues pageRoute => Url(pageRoute, url, protocol, host, fragment),
      _ => url.RouteUrl(routeName: null, route.RouteValues, protocol, host, fragment)
    };

  /// <summary>
  /// Generates a URI with an absolute path based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A URI with an absolute path, or <c>null</c> if a URI cannot be created.</returns>
  public static string Path(this IPageRouteValues route, LinkGenerator generator, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetPathByPage(route.PageName, route.HandlerName, route.RouteValues, pathBase, fragment, options);

  /// <summary>
  /// Generates a URI with an absolute path based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A URI with an absolute path, or <c>null</c> if a URI cannot be created.</returns>
  public static string Path(this IPageRouteValues route, LinkGenerator generator, HttpContext httpContext, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetPathByPage(httpContext, route.PageName, route.HandlerName, route.RouteValues, pathBase, fragment, options);

  /// <summary>
  /// Generates an absolute URI based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="scheme">The URI scheme, applied to the resulting URI.</param>
  /// <param name="host">The URI host/authority, applied to the resulting URI.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A absolute URI, or <c>null</c> if a URI cannot be created.</returns>
  /// <remarks>
  /// <para>
  /// The value of <paramref name="host" /> should be a trusted value. Relying on the value of the current request
  /// can allow untrusted input to influence the resulting URI unless the <c>Host</c> header has been validated.
  /// See the deployment documentation for instructions on how to properly validate the <c>Host</c> header in
  /// your deployment environment.
  /// </para>
  /// </remarks>
  public static string Url(this IPageRouteValues route, LinkGenerator generator, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetUriByPage(route.PageName, route.HandlerName, route.RouteValues, scheme, host, pathBase, fragment, options);

  /// <summary>
  /// Generates an absolute URI based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IPageRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
  /// <param name="scheme">The URI scheme, applied to the resulting URI. Optional. If not provided, the value of <see cref="HttpRequest.Scheme"/> will be used.</param>
  /// <param name="host">The URI host/authority, applied to the resulting URI. Optional. If not provided, the value <see cref="HttpRequest.Host"/> will be used.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A absolute URI, or <c>null</c> if a URI cannot be created.</returns>
  /// <remarks>
  /// <para>
  /// The value of <paramref name="host" /> should be a trusted value. Relying on the value of the current request
  /// can allow untrusted input to influence the resulting URI unless the <c>Host</c> header has been validated.
  /// See the deployment documentation for instructions on how to properly validate the <c>Host</c> header in
  /// your deployment environment.
  /// </para>
  /// </remarks>
  public static string Url(this IPageRouteValues route, LinkGenerator generator, HttpContext httpContext, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetUriByPage(httpContext, route.PageName, route.HandlerName, route.RouteValues, scheme, host, pathBase, fragment, options);

  /// <summary>
  /// Generates a URI with an absolute path based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A URI with an absolute path, or <c>null</c> if a URI cannot be created.</returns>
  public static string Path(this IControllerRouteValues route, LinkGenerator generator, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetPathByAction(route.ActionName, route.ControllerName, route.RouteValues, pathBase, fragment, options);

  /// <summary>
  /// Generates a URI with an absolute path based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A URI with an absolute path, or <c>null</c> if a URI cannot be created.</returns>
  public static string Path(this IControllerRouteValues route, LinkGenerator generator, HttpContext httpContext, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetPathByAction(httpContext, route.ActionName, route.ControllerName, route.RouteValues, pathBase, fragment, options);

  /// <summary>
  /// Generates an absolute URI based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="scheme">The URI scheme, applied to the resulting URI.</param>
  /// <param name="host">The URI host/authority, applied to the resulting URI.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A absolute URI, or <c>null</c> if a URI cannot be created.</returns>
  /// <remarks>
  /// <para>
  /// The value of <paramref name="host" /> should be a trusted value. Relying on the value of the current request
  /// can allow untrusted input to influence the resulting URI unless the <c>Host</c> header has been validated.
  /// See the deployment documentation for instructions on how to properly validate the <c>Host</c> header in
  /// your deployment environment.
  /// </para>
  /// </remarks>
  public static string Url(this IControllerRouteValues route, LinkGenerator generator, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetUriByAction(route.ActionName, route.ControllerName, route.RouteValues, scheme, host, pathBase, fragment, options);

  /// <summary>
  /// Generates an absolute URI based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IControllerRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
  /// <param name="scheme">The URI scheme, applied to the resulting URI. Optional. If not provided, the value of <see cref="HttpRequest.Scheme"/> will be used.</param>
  /// <param name="host">The URI host/authority, applied to the resulting URI. Optional. If not provided, the value <see cref="HttpRequest.Host"/> will be used.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A absolute URI, or <c>null</c> if a URI cannot be created.</returns>
  /// <remarks>
  /// <para>
  /// The value of <paramref name="host" /> should be a trusted value. Relying on the value of the current request
  /// can allow untrusted input to influence the resulting URI unless the <c>Host</c> header has been validated.
  /// See the deployment documentation for instructions on how to properly validate the <c>Host</c> header in
  /// your deployment environment.
  /// </para>
  /// </remarks>
  public static string Url(this IControllerRouteValues route, LinkGenerator generator, HttpContext httpContext, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    => generator.GetUriByAction(httpContext, route.ActionName, route.ControllerName, route.RouteValues, scheme, host, pathBase, fragment, options);

  /// <summary>
  /// Generates a URI with an absolute path based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A URI with an absolute path, or <c>null</c> if a URI cannot be created.</returns>
  public static string Path(this IRouteValues route, LinkGenerator generator, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    => route switch
    {
      IControllerRouteValues controllerRoute => Path(controllerRoute, generator, pathBase, fragment, options),
      IPageRouteValues pageRoute => Path(pageRoute, generator, pathBase, fragment, options),
      _ => generator.GetPathByRouteValues(routeName: null, route.RouteValues, pathBase, fragment, options)
    };

  /// <summary>
  /// Generates a URI with an absolute path based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A URI with an absolute path, or <c>null</c> if a URI cannot be created.</returns>
  public static string Path(this IRouteValues route, LinkGenerator generator, HttpContext httpContext, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    => route switch
    {
      IControllerRouteValues controllerRoute => Path(controllerRoute, generator, httpContext, pathBase, fragment, options),
      IPageRouteValues pageRoute => Path(pageRoute, generator, httpContext, pathBase, fragment, options),
      _ => generator.GetPathByRouteValues(httpContext, routeName: null, route.RouteValues, pathBase, fragment, options)
    };

  /// <summary>
  /// Generates an absolute URI based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="scheme">The URI scheme, applied to the resulting URI.</param>
  /// <param name="host">The URI host/authority, applied to the resulting URI.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A absolute URI, or <c>null</c> if a URI cannot be created.</returns>
  /// <remarks>
  /// <para>
  /// The value of <paramref name="host" /> should be a trusted value. Relying on the value of the current request
  /// can allow untrusted input to influence the resulting URI unless the <c>Host</c> header has been validated.
  /// See the deployment documentation for instructions on how to properly validate the <c>Host</c> header in
  /// your deployment environment.
  /// </para>
  /// </remarks>
  public static string Url(this IRouteValues route, LinkGenerator generator, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    => route switch
    {
      IControllerRouteValues controllerRoute => Url(controllerRoute, generator, scheme, host, pathBase, fragment, options),
      IPageRouteValues pageRoute => Url(pageRoute, generator, scheme, host, pathBase, fragment, options),
      _ => generator.GetUriByRouteValues(routeName: null, route.RouteValues, scheme, host, pathBase, fragment, options)
    };

  /// <summary>
  /// Generates an absolute URI based on the provided values.
  /// </summary>
  /// <param name="route">The <see cref="IRouteValues"/> to generate the URI for.</param>
  /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
  /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
  /// <param name="scheme">The URI scheme, applied to the resulting URI. Optional. If not provided, the value of <see cref="HttpRequest.Scheme"/> will be used.</param>
  /// <param name="host">The URI host/authority, applied to the resulting URI. Optional. If not provided, the value <see cref="HttpRequest.Host"/> will be used.</param>
  /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.</param>
  /// <param name="fragment">A URI fragment. Optional. Appended to the resulting URI.</param>
  /// <param name="options">An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching names from <c>RouteOptions</c>.</param>
  /// <returns>A absolute URI, or <c>null</c> if a URI cannot be created.</returns>
  /// <remarks>
  /// <para>
  /// The value of <paramref name="host" /> should be a trusted value. Relying on the value of the current request
  /// can allow untrusted input to influence the resulting URI unless the <c>Host</c> header has been validated.
  /// See the deployment documentation for instructions on how to properly validate the <c>Host</c> header in
  /// your deployment environment.
  /// </para>
  /// </remarks>
  public static string Url(this IRouteValues route, LinkGenerator generator, HttpContext httpContext, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    => route switch
    {
      IControllerRouteValues controllerRoute => Url(controllerRoute, generator, httpContext, scheme, host, pathBase, fragment, options),
      IPageRouteValues pageRoute => Url(pageRoute, generator, httpContext, scheme, host, pathBase, fragment, options),
      _ => generator.GetUriByRouteValues(httpContext, routeName: null, route.RouteValues, scheme, host, pathBase, fragment, options)
    };
}
