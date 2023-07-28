namespace SafeRouting.Tests.Unit;

[UsesVerify]
public sealed class PageTests
{
  [Fact]
  public Task AbstractPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public abstract class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task ConflictingPageNamesProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      namespace A
      {
        public sealed class EditModel : PageModel
        {
          public void OnGet()
          {
          }
        }
      }

      namespace B
      {
        public sealed class EditModel : PageModel
        {
          public void OnGet()
          {
          }
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task DeeplyNestedPagesHaveFolderNamesSeparatedWithUnderscores()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Foo", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task EmptyPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
      }
      """);
  }

  [Fact]
  public Task EscapedPageNamesAreHandled()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class @class : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task ExcludedPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [ExcludeFromRouteGenerator]
      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  /// <remarks>
  /// Generic page models are supported by ASP.NET Core, so this should be considered.
  /// </remarks>
  [Fact]
  public Task GenericPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel<T> : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task InheritedMembersAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public abstract class EditModelBase : PageModel
      {
        [BindProperty]
        public string? MyProperty { get; set; }

        public void OnGet()
        {
        }
      }

      public sealed class EditModel : EditModelBase
      {
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  /// <remarks>
  /// Internal page models are supported by ASP.NET Core, so this should be considered.
  /// </remarks>
  [Fact]
  public Task InternalPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      internal sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task InvalidPageNamesProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [RouteGeneratorName("%&*$#(.")]
      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task MultiplePagesInSeparateFilesAreIncluded()
  {
    var additionalSources = new[]
    {
      new AdditionalSource("""
        using Microsoft.AspNetCore.Mvc.RazorPages;

        public sealed class ViewModel : PageModel
        {
          public void OnGet()
          {
          }
        }
        """, Path: TestHelper.MakePath("Project", "Pages", "Products", "View.cshtml.cs"))
    };

    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"), additionalSources: additionalSources);
  }

  /// <remarks>
  /// Nested page models are supported by ASP.NET Core, so this should be considered.
  /// </remarks>
  [Fact]
  public Task NestedPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class Foo
      {
        public sealed class EditModel : PageModel
        {
          public void OnGet()
          {
          }
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task NonPageModelClassesAreExcluded()
  {
    return TestHelper.Verify("""
      public sealed class EditModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task NonPublicPagesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      internal sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task PagesDeclaredAsPartialAreExcludedIfCshtmlDotCsFileClassDoesntInheritPageModel()
  {
    var additionalSources = new[]
    {
      new AdditionalSource("""
        using Microsoft.AspNetCore.Mvc.RazorPages;

        public partial class IndexModel : PageModel
        {
          public void OnPost()
          {
          }
        }
        """)
    };

    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public partial class IndexModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Index.cshtml.cs"), additionalSources: additionalSources);
  }

  [Fact]
  public Task PagesDeclaredAsPartialAreFullyIncluded()
  {
    var additionalSources = new[]
    {
      new AdditionalSource("""
        using Microsoft.AspNetCore.Mvc.RazorPages;

        public partial class IndexModel
        {
          public void OnPost()
          {
          }
        }
        """)
    };

    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public partial class IndexModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Index.cshtml.cs"), additionalSources: additionalSources);
  }

  [Fact]
  public Task PagesInNonCshtmlDotCsFilesAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class IndexModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Index.cs"));
  }

  [Fact]
  public Task PagesInPagesDirectoryHaveNothingAddedToClassName()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class IndexModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Index.cshtml.cs"));
  }

  [Fact]
  public Task PagesInPagesDirectoryOnRootAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class IndexModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Pages", "Index.cshtml.cs"));
  }

  [Fact]
  public Task PagesOutsideOfAPagesDirectoryAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class IndexModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Index.cshtml.cs"));
  }

  [Fact]
  public Task PagesWithAGetHandlerAndFilePathAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task PagesWithAreaFilePathsAreConsidered()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Areas", "AreaName", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task PagesWithoutFilePathAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """);
  }

  [Fact]
  public Task PagesWithoutHandlerMethodsAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class IndexModel : PageModel
      {
        public void DoNothing()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Index.cshtml.cs"));
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameClasses()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [RouteGeneratorName("Renamed")]
      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task StaticPagesAreExcluded()
  {
    return TestHelper.Verify("""
      public static class EditModel
      {
        public static void OnGet()
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }
}
