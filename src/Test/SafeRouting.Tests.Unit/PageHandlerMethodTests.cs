namespace SafeRouting.Tests.Unit;

[UsesVerify]
public sealed class PageHandlerMethodTests
{
  [Fact]
  public Task AsyncMethodNameSuffixesAreTrimmed()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using System.Threading.Tasks;

      public sealed class EditModel : PageModel
      {
        public Task OnGetAsync() => Task.CompletedTask;
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task ExcludedHandlersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        [ExcludeFromRouteGenerator]
        public void OnGet()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task GenericMethodsAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet<T>()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task InvalidMethodNamesProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        [RouteGeneratorName("%&*$#(.")]
        public void OnGet()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task MethodsWithByRefParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGetIn(in int foo) { }
        public void OnGetOut(out int foo) { foo = 2; }
        public void OnGetRef(ref int foo) { }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task MethodsWithDifferentHandlerNamesAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet() { }
        public void OnGetIn() { }
        public void OnGetOut() { }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task MethodsWithSameNameProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public void OnGet() { }
        public void OnGet(string foo) { }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task NonHandlerAttributeMethodsAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        [NonHandler]
        public void OnGet()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task PrivateMethodsAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        private void OnGet()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameMethods()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using SafeRouting;

      public sealed class EditModel : PageModel
      {
        [RouteGeneratorName("Renamed")]
        public void OnGet()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }

  [Fact]
  public Task StaticMethodsAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class EditModel : PageModel
      {
        public static void OnGet()
        {
        }
      }
      """, path: @"C:\Project\Pages\Products\Edit.cshtml.cs");
  }
}
