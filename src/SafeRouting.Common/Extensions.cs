using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
}
