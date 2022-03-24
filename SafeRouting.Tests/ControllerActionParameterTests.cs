namespace SafeRouting.Tests;

[UsesVerify]
public sealed class ControllerActionParameterTests
{
  [Fact]
  public Task ActionParametersAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(string someValue) => View(someValue);
      }
    ");
  }

  [Fact]
  public Task CancellationTokenParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System.Threading;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(CancellationToken token) => View();
      }
    ");
  }

  [Fact]
  public Task DefaultValuesAreIncluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System;
      using System.Collections.Generic;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(
          DateTime a = default(DateTime),
          int b = 5,
          string c = ""he\""llo\\"",
          DayOfWeek d = DayOfWeek.Wednesday,
          bool e = false,
          string f = nameof(ProductsController),
          char g = '?',
          char h = (char)42,
          char i = '\'',
          char j = '\\') => View();
      }
    ");
  }

  [Fact]
  public Task ExcludedParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([ExcludeFromRouteGenerator] string fromServices) => View();
      }
    ");
  }

  [Fact]
  public Task InvalidParameterNamesProduceDiagnostic()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([RouteGeneratorName(""%&*$#(."")] string myValue) => View();
      }
    ");
  }

  [Fact]
  public Task NonUrlBoundParametersAreExcludedFromSignature()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([FromBody] string fromBody, [FromForm] string fromForm, [FromHeader] string fromHeader) => View();
      }
    ");
  }

  [Fact]
  public Task NullableContextDisabledInlineIsRespected()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System.Collections.Generic;

      public sealed class ProductsController : Controller
      {
#nullable disable
        public IActionResult A(IEnumerable<string> x, string y) => View(new { x, y });
#nullable restore
      }
    ");
  }

  [Fact]
  public Task NullableContextDisabledIsRespected()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System.Collections.Generic;

      public sealed class ProductsController : Controller
      {
        public IActionResult A(
          IEnumerable<string> x,
#nullable enable
          string y
#nullable restore
        ) => View(new { x, y });
      }
    ", nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Disable);
  }

  [Fact]
  public Task NullableRefenceTypeAnnotationsAreRespected()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using System.Collections.Generic;

      public sealed class ProductsController : Controller
      {
        public IActionResult A(IEnumerable<string> x, IEnumerable<string?>? y) => View(new { x, y });
      }
    ");
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameParameters()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([RouteGeneratorName(""Renamed"")] string myParameter) => View();
      }
    ");
  }

  [Fact]
  public Task ServiceBoundParametersAreExcluded()
  {
    return TestHelper.Verify(@"
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([FromServices] string fromServices) => View();
      }
    ");
  }
}