using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.IntegrationTests.Controllers;

public sealed class AccountsController : Controller
{
  [FromHeader]
  public string? CustomHeader { get; set; }

  public IActionResult Index() => View();

  public IActionResult List(int page) => View(page);

  [Area("Blog")]
  public IActionResult BlogAccount() => View();
}
