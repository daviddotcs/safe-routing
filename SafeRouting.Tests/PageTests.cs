namespace SafeRouting.Tests;

[UsesVerify]
public sealed class PageTests
{
  [Fact]
  public Task AbstractPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public abstract class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task ConflictingPageNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

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
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task EmptyPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
      }
    ");
  }

  [Fact]
  public Task ExcludedPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [ExcludeFromRouteGenerator]
      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  /// <remarks>
  /// Generic page models are supported by ASP.NET Core, so this should be considered.
  /// </remarks>
  [Fact]
  public Task GenericPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel<T> : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task InheritedMembersAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public abstract class EditModelBase : PageModel
      {
        [BindProperty]
        public string MyProperty { get; set; }

        public void OnGet()
        {
        }
      }

      public sealed class EditModel : EditModelBase
      {
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  /// <remarks>
  /// Internal page models are supported by ASP.NET Core, so this should be considered.
  /// </remarks>
  [Fact]
  public Task InternalPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      internal sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task InvalidPageNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [RouteGeneratorName(""%&*$#(."")]
      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  /// <remarks>
  /// Nested page models are supported by ASP.NET Core, so this should be considered.
  /// </remarks>
  [Fact]
  public Task NestedPagesAreExcluded()
  {
    return TestHelper.Verify(@"
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
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task NonPageModelClassesAreExcluded()
  {
    return TestHelper.Verify(@"
      public sealed class EditModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task NonPublicPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      internal sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task PagesWithAGetHandlerAndFilePathAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task PagesWithAreaFilePathsAreConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Areas\AreaName\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task PagesWithoutFilePathAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameClasses()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      [RouteGeneratorName(""Renamed"")]
      public sealed class EditModel : PageModel
      {
        public void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task StaticPagesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public static class EditModel
      {
        public static void OnGet()
        {
        }
      }
    ", path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }
}