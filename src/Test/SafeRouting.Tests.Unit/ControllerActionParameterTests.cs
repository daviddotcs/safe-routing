namespace SafeRouting.Tests.Unit;

[UsesVerify]
public sealed class ControllerActionParameterTests
{
  [Fact]
  public Task ActionParametersAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(string someValue) => View(someValue);
      }
      """);
  }

  [Fact]
  public Task CancellationTokenParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using System.Threading;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(CancellationToken token) => View();
      }
      """);
  }

  [Fact]
  public Task DefaultValuesAreIncluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using System;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(
          DateTime a = default(DateTime),
          int b = 5,
          string c = "he\"llo\\",
          DayOfWeek d = DayOfWeek.Wednesday,
          bool e = false,
          bool f = true,
          string g = nameof(ProductsController),
          char h = '?',
          char i = (char)42,
          char j = '\'',
          char k = '\\',
          int l = sizeof(double),
          byte m = unchecked((byte)~0x1^2),
          string n =

            /* delimited comment 1 */
            // single-line comment 1

            $"_{nameof(ProductsController)}_" + "z_" /* delimited comment inside expression */ + nameof(ProductsController) // single-line comment inside expression
              + $"{nameof(ProductsController) + nameof(ProductsController)}"

            /* delimited comment 2 */
            // single-line comment 2

        ) => View();
      }
      """);
  }

  [Fact]
  public Task EscapedParameterNamesAreHandled()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(string @class) => View();
      }
      """);
  }

  [Fact]
  public Task ExcludedParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([ExcludeFromRouteGenerator] string fromServices) => View();
      }
      """);
  }

  [Fact]
  public Task InvalidParameterNamesProduceDiagnostic()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(
          [RouteGeneratorName("%&*$#(.")] string a,
          [RouteGeneratorName("")] string b,
          [RouteGeneratorName("3")] string c,
          [RouteGeneratorName("@")] string d
        ) => View();
      }
      """);
  }

  [Fact]
  public Task NonUrlBoundParametersAreExcludedFromSignature()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([FromBody] string fromBody, [FromForm] string fromForm, [FromHeader] string fromHeader) => View();
      }
      """);
  }

  [Fact]
  public Task NullableContextDisabledInlineIsRespected()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using System.Collections.Generic;

      public sealed class ProductsController : Controller
      {
      #nullable disable
        public IActionResult A(IEnumerable<string> x, string y) => View(new { x, y });
      #nullable restore
      }
      """);
  }

  [Fact]
  public Task NullableContextDisabledIsRespected()
  {
    return TestHelper.Verify("""
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
      """, nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Disable);
  }

  [Fact]
  public Task NullableRefenceTypeAnnotationsAreRespected()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult A(string a, string? b) => View(new { a, b });
      }
      """);
  }

  [Fact]
  public Task NullableRefenceTypePermutationsAreHandledForIndexers()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using System.Collections.Generic;

      public sealed class ProductsController : Controller
      {
        public IActionResult T01(IEnumerable<string> a, IEnumerable<string> b) => View(new { a, b });
        public IActionResult T02(IEnumerable<string> a, IEnumerable<string>? b) => View(new { a, b });
        public IActionResult T03(IEnumerable<string> a, IEnumerable<string?> b) => View(new { a, b });
        public IActionResult T04(IEnumerable<string> a, IEnumerable<string?>? b) => View(new { a, b });
        public IActionResult T05(IEnumerable<string> a,
      #nullable disable
          IEnumerable<string> b
      #nullable restore
          ) => View(new { a, b });
        public IActionResult T06(IEnumerable<string>? a, IEnumerable<string> b) => View(new { a, b });
        public IActionResult T07(IEnumerable<string>? a, IEnumerable<string>? b) => View(new { a, b });
        public IActionResult T08(IEnumerable<string>? a, IEnumerable<string?> b) => View(new { a, b });
        public IActionResult T09(IEnumerable<string>? a, IEnumerable<string?>? b) => View(new { a, b });
        public IActionResult T10(IEnumerable<string>? a,
      #nullable disable
          IEnumerable<string> b
      #nullable restore
          ) => View(new { a, b });
        public IActionResult T11(IEnumerable<string?> a, IEnumerable<string> b) => View(new { a, b });
        public IActionResult T12(IEnumerable<string?> a, IEnumerable<string>? b) => View(new { a, b });
        public IActionResult T13(IEnumerable<string?> a, IEnumerable<string?> b) => View(new { a, b });
        public IActionResult T14(IEnumerable<string?> a, IEnumerable<string?>? b) => View(new { a, b });
        public IActionResult T15(IEnumerable<string?> a,
      #nullable disable
          IEnumerable<string> b
      #nullable restore
          ) => View(new { a, b });
        public IActionResult T16(IEnumerable<string?>? a, IEnumerable<string> b) => View(new { a, b });
        public IActionResult T17(IEnumerable<string?>? a, IEnumerable<string>? b) => View(new { a, b });
        public IActionResult T18(IEnumerable<string?>? a, IEnumerable<string?> b) => View(new { a, b });
        public IActionResult T19(IEnumerable<string?>? a, IEnumerable<string?>? b) => View(new { a, b });
        public IActionResult T20(IEnumerable<string?>? a,
      #nullable disable
          IEnumerable<string> b
      #nullable restore
          ) => View(new { a, b });
        public IActionResult T21(
      #nullable disable
          IEnumerable<string> a
      #nullable restore
          , IEnumerable<string> b) => View(new { a, b });
        public IActionResult T22(
      #nullable disable
          IEnumerable<string> a
      #nullable restore
          , IEnumerable<string>? b) => View(new { a, b });
        public IActionResult T23(
      #nullable disable
          IEnumerable<string> a
      #nullable restore
          , IEnumerable<string?> b) => View(new { a, b });
        public IActionResult T24(
      #nullable disable
          IEnumerable<string> a
      #nullable restore
          , IEnumerable<string?>? b) => View(new { a, b });
        public IActionResult T25(
      #nullable disable
          IEnumerable<string> a, IEnumerable<string> b
      #nullable restore
          ) => View(new { a, b });
      }
      """);
  }

  [Fact]
  public Task RouteGeneratorNameAttributesRenameParameters()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index(
          [RouteGeneratorName("A")] string a,
          [RouteGeneratorName("b")] string b,
          [RouteGeneratorName("RenamedC")] string c,
          [RouteGeneratorName("camelCaseRenamedD")] string d,
          [RouteGeneratorName("class")] string e,
          [RouteGeneratorName("\u16EF")] string f, // Nl category
          [RouteGeneratorName("_")] string g,
          [RouteGeneratorName("e\u0300")] string h, // Mn category
          [RouteGeneratorName("a\u0903b")] string i, // Mc category
          [RouteGeneratorName("j0")] string j, // Nd category
          [RouteGeneratorName("k\u203Fa")] string k, // Pc category
          [RouteGeneratorName("l\u00ADa")] string l // Cf category
        ) => View();
      }
      """);
  }

  [Fact]
  public Task ServiceBoundParametersAreExcluded()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.Extensions.DependencyInjection;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([FromServices] string fromServices, [FromKeyedServices("foo")] object fromKeyedServices) => View();
      }
      """, additionalSources: new[] { TestHelper.GetFromKeyedServicesAttributeAdditionalSource() });
  }

  [Fact]
  public Task SubsequentBindingAttributesAreIgnored()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([FromQuery, FromBody, FromForm, FromHeader] string includedParameter, [FromBody, FromQuery, FromRoute] string excludedParameter) => View();
      }
      """);
  }

  [Fact]
  public Task UrlBoundParametersAreIncludedInSignature()
  {
    return TestHelper.Verify("""
      using Microsoft.AspNetCore.Mvc;

      public sealed class ProductsController : Controller
      {
        public IActionResult Index([FromQuery] string fromQuery, [FromRoute] string fromRoute) => View();
      }
      """);
  }
}
