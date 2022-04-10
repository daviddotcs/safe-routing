using Microsoft.AspNetCore.Mvc;

namespace SafeRouting.Demo.Controllers;

public sealed class AccountController : Controller
{
  public IActionResult Index() => View();

  public IActionResult RedirectToController() => Routes.Areas.Blog.Controllers.Post.Index().Redirect(this);

  public IActionResult RedirectToPage() => Routes.Areas.Blog.Pages.Index.Get().Redirect(this);

  [Area("Blog")]
  public IActionResult BlogAccount() => View();
}
