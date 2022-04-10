using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.Tests.Integration.Areas.Blog.Controllers;

[Area("Blog")]
public sealed class PostController : Controller
{
  public IActionResult Index() => View();
}
