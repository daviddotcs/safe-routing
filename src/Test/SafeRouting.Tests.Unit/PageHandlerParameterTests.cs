namespace SafeRouting.Tests.Unit;

[UsesVerify]
public sealed class PageHandlerParameterTests
{
  [Fact]
  public Task CancellationTokenParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using System.Threading;

      public sealed class EditModel : PageModel
      {
        public void OnGet(CancellationToken cancellationToken)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task ExcludedParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        public void OnGet([ExcludeFromRouteGenerator] string myValue)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task InvalidParameterNamesProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        public void OnGet([RouteGeneratorName("%&*$#(.")] string myValue)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task NonUrlBoundParametersAreExcludedFromSignature()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet([FromBody] string fromBody, [FromForm] string fromForm, [FromHeader] string fromHeader)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task RegularParametersAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet(string myValue)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameParameters()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        public void OnGet([RouteGeneratorName("Foo")] string myValue)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task ServiceBoundParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using Microsoft.Extensions.DependencyInjection;

      public sealed class EditModel : PageModel
      {
        public void OnGet([FromServices] string fromServices, [FromKeyedServices("foo")] object fromKeyedServices)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"), additionalSources: new[] { TestHelper.GetFromKeyedServicesAttributeAdditionalSource() });
  }
}
