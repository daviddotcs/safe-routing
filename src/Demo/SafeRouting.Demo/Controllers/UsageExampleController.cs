using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.Demo.Controllers;

public sealed class UsageExampleController : Controller
{
  public IActionResult OriginalRedirect()
  {
    #region OriginalRedirect
    return RedirectToAction("Search", "Product", new { Name = "chair", Limit = 10 });
    #endregion
  }

  public IActionResult SourceGeneratorRedirect()
  {
    #region NewRedirect
    return Routes.Controllers.Product.Search("chair", 10).Redirect(this);
    #endregion
  }

  public string? PageUrl()
  {
    #region EditUrl
    string? editUrl = Routes.Pages.Edit.Get(123).Url(Url);
    #endregion
    return editUrl;
  }

  public IActionResult GettingStarted([FromServices] LinkGenerator linkGenerator)
  {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
    #region GettingStarted
    // For C# 9 and below include this using directive to enable the Redirect() and Url() extension methods:
    //using SafeRouting.Extensions;

    // Get route information for the Search method on ProductController with a name value of "chair" and limit unset
    // Route: /Product/Search/chair
    var route = Routes.Controllers.Product.Search("chair", limit: null);

    // Assign a value for the Limit property (defined on the controller class)
    // Route: /Product/Search/chair/5
    route[route.Properties.Limit] = 5;

    // Set the value of a parameter
    // Route: /Product/Search/book/5
    route[route.Parameters.Name] = "book";

    // Set a value using the Set method
    // Route: /Product/Search/book/10
    route.Set(route.Properties.Limit, 10);

    // Remove a route value
    // Route: /Product/Search/book
    route.Remove(route.Properties.Limit);

    // Access the URL for the route using an IUrlHelper
    // Value: "/Product/Search/book"
    string? routeUrl = route.Url(Url);

    // Get route information for the OnGet method on the /Edit page
    var pageRoute = Routes.Pages.Edit.Get(123);

    // "/Edit?Id=123"
    var path = pageRoute.Path(linkGenerator);

    // "https://example.org/Edit?Id=123"
    var uri = pageRoute.Url(linkGenerator, "https", new HostString("example.org"));

    // Redirect from within a controller action method or a page handler method
    return route.Redirect(this);
    #endregion
#pragma warning restore IDE0059 // Unnecessary assignment of a value
  }
}
