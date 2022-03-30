using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.Demo.Areas.Blog.Controllers;

[Area("Blog")]
public sealed class SettingsController : Controller
{
  public IActionResult Index() => View();
}
