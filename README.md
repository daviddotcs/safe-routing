# Safe Routing Source Generator for ASP.NET Core

[![SafeRouting NuGet Package](https://img.shields.io/nuget/v/SafeRouting.svg?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/SafeRouting)

Safe Routing is a [source generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) which analyses a project's razor pages and MVC controllers, producing strongly-typed representations of those routes as you type. This enables you to link between pages with compile time safety instead of using the standard _"stringly typed"_ approach.

* [Usage Example](#Usage-Example)
* [Installation](#Installation)
  * [Tag Helpers](#Tag-Helpers)
  * [Extension Methods](#Extension-Methods)
* [Getting Started](#Getting-Started)
  * [Binding Source Attributes](#Binding-Source-Attributes)
  * [Helper Attributes](#Helper-Attributes)
  * [Areas](#Areas)
  * [Using Razor Class Libraries](#Using-Razor-Class-Libraries)
* [Configuration](#Configuration)
  * [Available Configuration Options](#Available-Configuration-Options)
* [Limitations](#Limitations)
* [Building the NuGet Package](#Building-the-NuGet-Package)

## Usage Example

Consider the following contrived example of a controller class.

```C#
public class ProductsController : Controller
{
  [FromRoute]
  public int? Limit { get; set; }

  [Route("/Products/Search/{name}/{Limit?}")]
  public IActionResult Search(string name)
  {
    // ...
  }
}
```

Ordinarily, you would need to write something like the following to redirect to the `Search` action from another controller or page:

```C#
return RedirectToAction("Search", "Products", new { Name = "chair" });
```

Instead, by using the generated code, that can be simplified to the following:

```C#
return Routes.Controllers.Products.Search("chair").Redirect(this);
```

The controller name, action name, names of action method parameters, and names of bound properties on the controller are no longer referenced with strings, and are instead referenced with C# classes, methods, parameters, and properties that offer compile time safety.

Similarly, consider the following razor page model class:

```C#
public sealed class EditModel : PageModel
{
  public Task OnGetAsync()
  {
    // ...
  }
}
```

The generated code enables you to access the URL for the `OnGetAsync` handler with the following code:

```C#
string myUrl = Routes.Pages.Edit.Get().Url(Url);
```

## Installation

To install, simply add the [SafeRouting](https://www.nuget.org/packages/SafeRouting) package to your ASP.NET Core project.

### Tag Helpers

To enable the included tag helpers, add the following line to `_ViewImports.cshtml` files where required.

```cshtml
@addTagHelper SafeRouting.TagHelpers.*, SafeRouting.Common
```

This enables `for-route` attributes to be added to `<a>`, `<img>`, and `<form>` elements, for example:

```html
@{
  var controllerRoute = Routes.Controllers.Products.Search("chair");
  var pageRoute = Routes.Pages.Edit.Post();
}

<!-- Adds the URL in the href attribute -->
<a for-route="controllerRoute">Search for chairs</a>

<!-- Adds the URL in the src attribute -->
<img for-route="controllerRoute" alt="" />

<!-- Adds the URL in the action attribute -->
<form for-route="pageRoute" method="post"></form>
```

### Extension Methods

Add `using SafeRouting.Extensions;` to your source code to access the extension methods `.Redirect()` and `.Url()`. The Redirect extension methods return `RedirectToActionResult` or `RedirectToPageResult` values as appropriate for the particular route, and accept the active controller or page model as a parameter. The Url extension methods return a string with a URL for the route, accepting an `IUrlHelper` instance as a parameter.

## Getting Started

The following code snippet demonstrates accessing, modifying, and retrieving generated route information for the `ProductsController` class defined above.

```C#
// Enable the .Redirect() and .Url() extension methods
using SafeRouting.Extensions;

// Get route information for the Search method on ProductsController with a name value of "chair"
// Route: /Products/Search/chair
var route = Routes.Controllers.Products.Search("chair");

// Assign a value for the Limit property (defined on the controller class)
// Route: /Products/Search/chair/5
route[route.Properties.Limit] = 5;

// Set the value of a parameter
// Route: /Products/Search/book/5
route[route.Parameters.Name] = "book";

// Set a value using the Set method
// Route: /Products/Search/book/10
route.Set(route.Properties.Limit, 10);

// Remove a route value
// Route: /Products/Search/book
route.Remove(route.Properties.Limit);

// Access the URL for the route using an IUrlHelper
// Value: "/Products/Search/book"
string routeUrl = route.Url(Url);

// Redirect from within a controller action method or a page handler method
return route.Redirect(this);
```

### Binding Source Attributes

The generated methods will closely resemble your original controller action methods and page handler methods, but will only include parameters which can be bound via the URL. Consider the following action method:

```C#
public IActionResult Index(
  string standard,
  [FromBody] string fromBody,
  [FromForm] string fromForm,
  [FromHeader] string fromHeader,
  [FromQuery] string fromQuery,
  [FromRoute] string fromRoute,
  [FromServices] string fromServices)
{
  // ...
}
```

The generated route helper method omits the parameters with the attributes `[FromBody]`, `[FromForm]`, `[FromHeader]`, and `[FromServices]` because they are not bound to any part of the URL. The generated helper method instead looks like this:

```C#
public static IndexRouteInfo Index(string standard, string fromQuery, string fromRoute)
{
  // ...
}
```

### Helper Attributes

A couple of attributes exist which allow you to customise how the source generator interprets your code. `[ExcludeFromRouteGenerator]` can be applied to a class, property, method, or parameter to have it be ignored by the analyser. `[RouteGeneratorName]` allows you to rename any symbol (class, property, method, or parameter) in the generated code, which can help you avoid naming conflicts.

### Areas

By default, the generated helper classes for controller and page routes will be added to the namespaces `Routes.Controllers` and `Routes.Pages`, respectively. Controllers adorned with the `[Area]` attribute, and pages within an `/Areas/{area-name}/Pages/` directory structure have their helper classes added to `Routes.Areas.AreaName.Controllers` and `Routes.Areas.AreaName.Pages` respectively (replacing _AreaName_ with the name of the area).

### Using Razor Class Libraries

Route information is only generated for source code within each project which references the SafeRouting package. In order to reference routes within another library, that library must reference SafeRouting and be configured to use the public access modifier for classes (which is the default).

## Configuration

This source generator can be configured via a [Global AnalyzerConfig](https://docs.microsoft.com/en-gb/dotnet/fundamentals/code-analysis/configuration-files#global-analyzerconfig) file.

Example `.globalconfig` file:

```editorconfig
is_global = true

safe_routing.generated_access_modifier = internal
safe_routing.generated_namespace = Example.Namespace.Routes
```

### Available configuration options

| Option                                   | Description                                                                                              |
|------------------------------------------|----------------------------------------------------------------------------------------------------------|
| `safe_routing.generated_access_modifier` | The access modifier used for all generated classes. Can be _public_ or _internal_. Defaults to _public_. |
| `safe_routing.generated_namespace`       | The namespace under which all generated route classes are created. Defaults to _Routes_.                 |

## Limitations

* The including project must use C# 8 or later.
* Pages must have a `PageModel` inheriting class within a `.cshtml.cs` file in either a `Pages` or `Areas/{area name}/Pages` directory at any depth to be discovered.
* Multiple classes which inherit from `PageModel` cannot be declared in the same `.cshtml.cs` file.
* Custom attributes which affect routing are unsupported and will be ignored by the source generator.
* Using identifiers which need to be escaped with `@` for names of classes, properties, or parameters is unsupported.
* Nullable annotations on parameter and property types are respected, but attributes affecting nullability are not copied across to the generated code.
* Generic classes, nested classes, and non-public classes which inherit from `PageModel` are ignored by the source generator.
* For .NET 7, it is recommended to either continue using the `[FromServices]` attribute for parameters which are implicitly injected, or to replace it with `[ExcludeFromRouteGenerator]`. Otherwise injected parameters will be included in the method signatures of the generated route methods.

## Building the NuGet Package

* Ensure you have the latest .NET SDK installed via https://dotnet.microsoft.com/en-us/download/dotnet.
* Install [dotnet-script](https://github.com/filipw/dotnet-script).

```
dotnet tool install -g dotnet-script
```

* Within the project directory, run the build script with the new build number as an argument, e.g.; 1.2.3.

```
dotnet script build.csx -- 1.2.3
```

* Review the output to ensure that the build succeeded and all tests passed.
