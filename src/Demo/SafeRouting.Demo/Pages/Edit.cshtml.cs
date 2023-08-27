using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SafeRouting.Demo.Pages;

#region EditModel

public sealed class EditModel : PageModel
{
  [FromRoute]
  public int Id { get; set; }

  public void OnGet()
  {
    // ...
  }

  public void OnPost()
  {
    // ...
  }
}

#endregion
