using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SafeRouting.Demo.Pages;

public sealed class ProductModel : PageModel
{
  public void OnGet() { }

  public IActionResult OnGetRedirectToController() => Routes.Controllers.Accounts.Index().Redirect(this);

  public IActionResult OnGetRedirectToPage() => Routes.Pages.Index.Get().Redirect(this);
}
