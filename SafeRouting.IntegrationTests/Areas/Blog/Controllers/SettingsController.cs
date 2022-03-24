using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.IntegrationTests.Areas.Blog.Controllers;

[Area("Blog")]
public sealed class SettingsController : Controller
{
  public IActionResult Index() => View();
}
