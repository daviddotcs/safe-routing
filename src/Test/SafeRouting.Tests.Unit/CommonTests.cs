using Microsoft.CodeAnalysis.CSharp;

namespace SafeRouting.Tests.Unit;

public sealed class CommonTests
{
  [Fact]
  public Task EmptySourceProducesCommonOutput()
  {
    return TestHelper.Verify("");
  }

  [Theory]
  [InlineData(LanguageVersion.CSharp10)]
  [InlineData(LanguageVersion.CSharp11)]
  [InlineData(LanguageVersion.CSharp12)]
  public Task GlobalUsingsGeneratedForSupportedLanguageVersions(LanguageVersion version)
  {
    return TestHelper.Verify("", languageVersion: version, parameters: [version]);
  }

  [Theory]
  [InlineData(LanguageVersion.CSharp8)]
  [InlineData(LanguageVersion.CSharp9)]
  public Task GlobalUsingsNotGeneratedForUnsupportedLanguageVersions(LanguageVersion version)
  {
    return TestHelper.Verify("", languageVersion: version, parameters: [version]);
  }

  [Fact]
  public Task InternalAccessModifierOptionProducesInternalClasses()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_access_modifier"] = "internal" });
  }

  [Fact]
  public Task InvalidAccessModifierOptionProducesDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_access_modifier"] = "invalid access modifier" });
  }

  [Fact]
  public Task InvalidNamespaceOptionProducesDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_namespace"] = "x.1nvalid Namespace,[]!" });
  }

  [Fact]
  public Task InvalidParameterCaseOptionProducesDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class ProductsController : Controller
      {
        [FromRoute]
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      public sealed class EditModel : PageModel
      {
        [FromRoute]
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_parameter_case"] = "invalid" });
  }

  [Fact]
  public Task NamespaceOptionChangesNamespace()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_namespace"] = "Test.Namespace" });
  }

  [Fact]
  public Task ParameterCaseOptionProducesPascalCaseParameters()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class ProductsController : Controller
      {
        [FromRoute]
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      public sealed class EditModel : PageModel
      {
        [FromRoute]
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_parameter_case"] = "pascal" });
  }

  [Fact]
  public Task PublicAccessModifierOptionProducesPublicClasses()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_access_modifier"] = "public" });
  }

  [Fact]
  public Task StandardControllerAndPageModelProduceFullOutput()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using System.Collections.Generic;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }
        public Dictionary<string, object>? Foo { get; set; }

        public IActionResult Index(int id, Dictionary<string, object> bar)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }
        public Dictionary<string, object>? Foo { get; set; }

        public void OnGet(string name, Dictionary<string, object> bar)
        {
        }
      }
      """, path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"));
  }

  [Fact]
  public Task StandardParameterCaseOptionProducesCamelCaseParameters()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;

      public sealed class ProductsController : Controller
      {
        [FromRoute]
        public string? Name { get; set; }

        public IActionResult Index(int id)
        {
          return View();
        }
      }

      public sealed class EditModel : PageModel
      {
        [FromRoute]
        public string? Title { get; set; }

        public void OnGet(string name)
        {
        }
      }
      """,
    path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
    options: new TestConfigOptions { ["safe_routing.generated_parameter_case"] = "standard" });
  }

  [Theory]
  [InlineData(LanguageVersion.CSharp8)]
  [InlineData(LanguageVersion.CSharp9)]
  [InlineData(LanguageVersion.CSharp10)]
  [InlineData(LanguageVersion.CSharp11)]
  [InlineData(LanguageVersion.CSharp12)]
  public Task SupportedLanguageVersionsBuild(LanguageVersion version)
  {
    return TestHelper.Verify("", languageVersion: version, parameters: [version]);
  }

  [Theory]
  [InlineData(LanguageVersion.CSharp1)]
  [InlineData(LanguageVersion.CSharp2)]
  [InlineData(LanguageVersion.CSharp3)]
  [InlineData(LanguageVersion.CSharp4)]
  [InlineData(LanguageVersion.CSharp5)]
  [InlineData(LanguageVersion.CSharp6)]
  [InlineData(LanguageVersion.CSharp7)]
  [InlineData(LanguageVersion.CSharp7_1)]
  [InlineData(LanguageVersion.CSharp7_2)]
  [InlineData(LanguageVersion.CSharp7_3)]
  public Task UnsupportedLanguageVersionsProduceDiagnostic(LanguageVersion version)
  {
    var source = """
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Mvc.RazorPages;
      using System.Collections.Generic;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string? Name { get; set; }
        public Dictionary<string, object>? Foo { get; set; }

        public IActionResult Index(int id, Dictionary<string, object> bar)
        {
          return View();
        }
      }

      [BindProperties]
      public sealed class EditModel : PageModel
      {
        public string? Title { get; set; }
        public Dictionary<string, object>? Foo { get; set; }

        public void OnGet(string name, Dictionary<string, object> bar)
        {
        }
      }
      """;

    return TestHelper.Verify(source,
      path: TestHelper.MakePath("Project", "Pages", "Products", "Edit.cshtml.cs"),
      languageVersion: version,
      nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Disable,
      parameters: [version],
      testCompilation: false);
  }
}
