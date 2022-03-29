namespace SafeRouting.Tests;

[UsesVerify]
public sealed class ControllerTests
{
  [Fact]
  public Task AbstractControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public abstract class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task AreaAttributesAreConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [Area(""Foo"")]
      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task AreaAttributesAreInherited()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [Area(""Bar"")]
      public abstract class ProductsControllerBaseBase : Controller
      {
        public IActionResult Index() => View();
      }

      [Area(""Foo"")]
      public abstract class ProductsControllerBase : ProductsControllerBaseBase
      {
      }

      public sealed class ProductsController : ProductsControllerBase
      {
      }
    ");
  }

  [Fact]
  public Task ConflictingControllerNamesInDifferentAreasAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      namespace a
      {
        public sealed class ProductsController : Controller
        {
          public IActionResult Index() => View();
        }
      }

      namespace B
      {
        [Area(""Other"")]
        public sealed class ProductsController : Controller
        {
          public IActionResult Index() => View();
        }
      }
    ");
  }

  [Fact]
  public Task ConflictingControllerNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      namespace a
      {
        public sealed class ProductsController : Controller
        {
          public IActionResult Index() => View();
        }
      }

      namespace B
      {
        public sealed class ProductsController : Controller
        {
          public IActionResult Index() => View();
        }
      }
    ");
  }

  [Fact]
  public Task ControllerAttributeIsInherited()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [Controller]
      public abstract class ProductsControllerBase
      {
        public void Index() { }
      }

      public sealed class ProductsController : ProductsControllerBase
      {
      }
    ");
  }

  [Fact]
  public Task ControllersNamedControllerAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class Controller : Microsoft.AspNetCore.Mvc.Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task ControllersWithAnActionMethodAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task ControllersWithoutControllerSuffixAreNamedAsIs()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class Products : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task EmptyControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
      }
    ");
  }

  [Fact]
  public Task ExcludedAncestorControllersArentConsidered()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [ExcludeFromRouteGenerator]
      public abstract class BaseController : Controller
      {
      }

      public sealed class ProductsController : BaseController
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task ExcludedControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [ExcludeFromRouteGenerator]
      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task GenericControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController<T> : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task InheritedMembersAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public abstract class ProductsControllerBase : Controller
      {
        [BindProperty]
        public string? MyProperty { get; set; }

        public IActionResult Index() => View();
      }

      public sealed class ProductsController : ProductsControllerBase
      {
      }
    ");
  }

  [Fact]
  public Task InvalidControllerNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [RouteGeneratorName(""%&*$#(."")]
      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task MultipleControllersAreHandled()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class AController : Controller
      {
        public IActionResult Index() => View();
      }

      public sealed class BController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task NestedControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class Foo
      {
        public sealed class ProductsController : Controller
        {
          public IActionResult Index() => View();
        }
      }
    ");
  }

  [Fact]
  public Task NonControllerClassesAreExcluded()
  {
    return TestHelper.Verify(@"
      public sealed class ProductsController
      {
        public void Index() { }
      }
    ");
  }

  [Fact]
  public Task NonControllerClassesWithControllerAttributeAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [Controller]
      public sealed class ProductsController
      {
        public void Index() { }
      }
    ");
  }

  [Fact]
  public Task NonControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [NonController]
      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task NonPublicControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      internal sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameClasses()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [RouteGeneratorName(""Renamed"")]
      public sealed class ProductsController : Controller
      {
        public IActionResult Index() => View();
      }
    ");
  }

  [Fact]
  public Task StaticControllersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      [Controller]
      public static class ProductsController
      {
        public static void Index() { }
      }
    ");
  }
}
