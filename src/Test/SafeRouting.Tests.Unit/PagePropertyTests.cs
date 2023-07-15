namespace SafeRouting.Tests.Unit;

[UsesVerify]
public sealed class PagePropertyTests
{
  [Fact]
  public Task BindPropertiesAttributesAreConsidered()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? SomeProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task BindPropertiesAttributesSupportingGetAreIncludedInMethodSignature()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties(SupportsGet = true)]
      public sealed class EditModel : PageModel
      {
        public string? SomeProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task BindPropertyAttributeNamesAreConsidered()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [BindProperty(Name = "RenamedBindProperty")]
        public string? BindProperty { get; set; }

        [FromForm(Name = "RenamedFromForm")]
        public string? FromForm { get; set; }

        [FromHeader(Name = "RenamedFromHeader")]
        public string? FromHeader { get; set; }

        [FromQuery(Name = "RenamedFromQuery")]
        public string? FromQuery { get; set; }

        [FromRoute(Name = "RenamedFromRoute")]
        public string? FromRoute { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task ExcludedPropertiesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        [ExcludeFromRouteGenerator]
        [BindProperty]
        public string? BindProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task InvalidPropertyNamesProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        [RouteGeneratorName("%&*$#(.")]
        [FromForm]
        public string? FromForm { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task PrivatePropertiesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [BindProperty]
        private string? BindProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task PropertyBindingAttributesAreConsidered()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [BindProperty]
        public string? BindProperty { get; set; }

        [FromBody]
        public string? FromBody { get; set; }

        [FromForm]
        public string? FromForm { get; set; }

        [FromHeader]
        public string? FromHeader { get; set; }

        [FromQuery]
        public string? FromQuery { get; set; }

        [FromRoute]
        public string? FromRoute { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task PropertiesWithoutPublicGettersAndSettersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [BindProperty]
        public string? Excluded1 { get; }

        [BindProperty]
        public string? Excluded2 { private get; set; }

        [BindProperty]
        public string? Excluded3 { get; private set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameProperties()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        [RouteGeneratorName("Renamed")]
        public string? BindProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task GetBoundPropertiesAreAppendedToMethodSignature()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [BindProperty(SupportsGet = true)]
        public string? BindValue { get; set; }

        [FromQuery]
        public string? QueryValue { get; set; }

        [FromRoute]
        public string? RouteValue { get; set; }

        public void OnGet(int x)
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task RouteBoundPropertiesArentAppendedWhenConflictingWithExistingParameters()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        [FromRoute]
        public string? A { get; set; }

        [FromRoute]
        public string? B { get; set; }

        [FromRoute(Name = "C")]
        public string? SomeProperty { get; set; }

        [RouteGeneratorName("D")]
        [FromRoute]
        public string? OtherProperty { get; set; }

        public void OnGet(string? a, [FromRoute(Name = "B")] string? x, string? c, string? d, int y)
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task StaticPropertiesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [BindProperty]
        public static string? BindProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }

  [Fact]
  public Task UnboundPropertiesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public string? BindProperty { get; set; }

        public void OnGet()
        {
        }
      }
      """, pathSegments: new[] { "Project", "Pages", "Products", "Edit.cshtml.cs" });
  }
}
