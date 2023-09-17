using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.Demo.Controllers;

#pragma warning disable ASP0018 // Unused route parameter
#pragma warning disable IDE0060 // Remove unused parameter

#region ProductController

public sealed class ProductController : Controller
{
  [FromRoute]
  public int? Limit { get; set; }

  [Route("/Product/Search/{name}/{Limit?}")]
  public IActionResult Search(string name) => Ok();
}

#endregion

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore ASP0018 // Unused route parameter
