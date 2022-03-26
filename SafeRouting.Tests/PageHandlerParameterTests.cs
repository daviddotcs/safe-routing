namespace SafeRouting.Tests;

[UsesVerify]
public sealed class PageHandlerParameterTests
{
  [Fact]
  public Task CancellationTokenParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using System.Threading;

      public sealed class EditModel : PageModel
      {
        public void OnGet(CancellationToken cancellationToken)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task ExcludedParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        public void OnGet([ExcludeFromRouteGenerator] string myValue)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task InvalidParameterNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        public void OnGet([RouteGeneratorName(""%&*$#(."")] string myValue)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task NonUrlBoundParametersAreExcludedFromSignature()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet([FromBody] string fromBody, [FromForm] string fromForm, [FromHeader] string fromHeader)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task RegularParametersAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet(string myValue)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameParameters()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        public void OnGet([RouteGeneratorName(""Foo"")] string myValue)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task ServiceBoundParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet([FromServices] string fromServices)
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }
}
