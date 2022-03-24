namespace SafeRouting.Tests;

[UsesVerify]
public sealed class ControllerPropertyTests
{
  [Fact]
  public Task BindPropertiesAttributesAreConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        public string SomeProperty { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task BindPropertiesAttributesAreInherited()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public abstract class ProductsControllerBaseBase : Controller
      {
        public string? A { get; set; }
      }

      [BindProperties]
      public abstract class ProductsControllerBase : ProductsControllerBaseBase
      {
        public string? B { get; set; }
      }

      public sealed class ProductsController : ProductsControllerBase
      {
        public string? C { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task BindPropertyAttributeNamesAreConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [BindProperty(Name = ""RenamedBindProperty"")]
        public string BindProperty { get; set; }

        [FromForm(Name = ""RenamedFromForm"")]
        public string FromForm { get; set; }

        [FromHeader(Name = ""RenamedFromHeader"")]
        public string FromHeader { get; set; }

        [FromQuery(Name = ""RenamedFromQuery"")]
        public string FromQuery { get; set; }

        [FromRoute(Name = ""RenamedFromRoute"")]
        public string FromRoute { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task ExcludedPropertiesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        [ExcludeFromRouteGenerator]
        public string MyProperty { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task InvalidPropertyNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        [RouteGeneratorName(""%&*$#(."")]
        [FromForm]
        public string FromForm { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task PrivatePropertiesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [BindProperty]
        private string BindProperty { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task PropertyBindingAttributesAreConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [BindProperty]
        public string BindProperty { get; set; }

        [FromBody]
        public string FromBody { get; set; }

        [FromForm]
        public string FromForm { get; set; }

        [FromHeader]
        public string FromHeader { get; set; }

        [FromQuery]
        public string FromQuery { get; set; }

        [FromRoute]
        public string FromRoute { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task PropertiesWithoutPublicGettersAndSettersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [BindProperty]
        public string? Excluded1 { get; }

        [BindProperty]
        public string? Excluded2 { private get; set; }

        [BindProperty]
        public string? Excluded3 { get; private set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameProperties()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [BindProperties]
      public sealed class ProductsController : Controller
      {
        [RouteGeneratorName(""Renamed"")]
        public string MyProperty { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task StaticPropertiesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [BindProperty]
        public static string BindProperty { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task UnboundPropertiesAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public string SomeProperty { get; set; }

        public IActionResult Index() => View();
      }
    ");
  }
}
