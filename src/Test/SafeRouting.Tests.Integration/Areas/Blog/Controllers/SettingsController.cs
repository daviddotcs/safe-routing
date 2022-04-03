using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.Tests.Integration.Areas.Blog.Controllers;

[Area("Blog")]
public sealed class SettingsController : Controller
{
  public IActionResult Index() => View();
}
