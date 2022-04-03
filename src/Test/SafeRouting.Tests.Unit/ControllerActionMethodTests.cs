namespace SafeRouting.Tests.Unit;

[UsesVerify]
public sealed class ControllerActionMethodTests
{
  [Fact]
  public Task ActionAreaAttributesOverrideController()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [Area(""Foo"")]
      public sealed class ProductsController : Controller
      {
        [Area(""Bar"")]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task ActionNameAttributesRenameActions()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [ActionName(""Renamed"")]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task AreaAttributesAreConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [Area(""Foo"")]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task AsyncMethodNameSuffixesAreTrimmed()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System.Threading.Tasks;

      public sealed class ProductsController : Controller
      {
        public Task<IActionResult> IndexAsync() => Task.FromResult((IActionResult)View());
      }
    ");
  }

  [Fact]
  public Task AsyncMethodsNamedAsyncAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System.Threading.Tasks;

      public sealed class ProductsController : Controller
      {
        public Task<IActionResult> Async() => Task.FromResult((IActionResult)View());
      }
    ");
  }

  [Fact]
  public Task ExcludedMethodsAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        [ExcludeFromRouteGenerator]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task GenericMethodsAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index<T>() => View();
      }
    ");
  }

  [Fact]
  public Task InvalidMethodNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        [RouteGeneratorName(""%&*$#(."")]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task MethodsWithByRefParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult In(in int foo) => View();
        public IActionResult Out(out int foo) { foo = 2; return View(); }
        public IActionResult Ref(ref int foo) => View();
      }
    ");
  }

  [Fact]
  public Task MethodsWithDifferentNamesAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
        public IActionResult Product(int id) => View();
      }
    ");
  }

  [Fact]
  public Task MethodsWithSameNameButDifferentParametersAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
        public IActionResult Index(string someValue) => View();
      }
    ");
  }

  [Fact]
  public Task MethodsWithSameResultingSignatureAreCollapsed()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Index([FromServices] string someValue) => View();
      }
    ");
  }

  [Fact]
  public Task MethodsWithSameResultingSignatureButDifferentParameterPropertiesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Index([FromForm] string someValue) => View();
      }
    ");
  }

  [Fact]
  public Task NonActionAttributeMethodsAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        [NonAction]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task PrivateMethodsAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        private IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task ReservedMethodNamesAreEscaped()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult @class() => View();
      }
    ");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameMethods()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        [RouteGeneratorName(""Renamed"")]
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task StaticMethodsAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public static void Index() { }
      }
    ");
  }
}
