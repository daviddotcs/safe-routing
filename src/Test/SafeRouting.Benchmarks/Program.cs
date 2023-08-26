using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

_ = BenchmarkRunner.Run<VersionComparison>();

[Config(typeof(Config))]
[MemoryDiagnoser]
public class VersionComparison
{
  public VersionComparison()
  {
    var languageVersion = LanguageVersion.Latest;
    var source = """
      using Microsoft.AspNetCore.Mvc;
      using SafeRouting;

      [Area("Foo")]
      [BindProperties]
      public sealed class ProductsController : Controller
      {
        [RouteGeneratorName("Renamed")]
        public string? MyProperty { get; set; }

        [Area("Bar")]
        public IActionResult Index() => View();

        public Task<IActionResult> FooAsync() => Task.FromResult((IActionResult)View());
      }

      public sealed class OtherController : Controller
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

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Index([FromServices] string someValue) => View();

        public IActionResult Index(string someValue) => View();
        public IActionResult Index(string someValue, string someOtherValue) => View();

        [ExcludeFromRouteGenerator]
        public IActionResult Excluded() => View();

        [RouteGeneratorName("%&*$#(.")]
        public IActionResult Diagnostic() => View();

        public IActionResult In(in int foo) => View();
        public IActionResult Out(out int foo) { foo = 2; return View(); }
        public IActionResult Ref(ref int foo) => View();
      }
      """;
    var path = "";
    var references = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)!
      .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
      .Select(x => MetadataReference.CreateFromFile(x))
      .ToArray();
    var nullableContextOptions = NullableContextOptions.Enable;
    var generators = new[]
    {
      string.Equals(Environment.GetEnvironmentVariable("version"), "new", StringComparison.OrdinalIgnoreCase)
        ? new SafeRouting.Generator.RouteGenerator().AsSourceGenerator()
        : new SafeRouting.BenchmarkGenerator.RouteGenerator().AsSourceGenerator()
    };

    var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);

    var syntaxTree = CSharpSyntaxTree.ParseText(
      source,
      path: path,
      options: parseOptions);

    var syntaxTrees = new List<SyntaxTree> { syntaxTree };

    //if (additionalSources is not null)
    //{
    //  foreach (var additionalSource in additionalSources)
    //  {
    //    syntaxTrees.Add(CSharpSyntaxTree.ParseText(
    //      additionalSource.Source,
    //      path: additionalSource.Path,
    //      options: additionalSource.ParseOptions ?? parseOptions));
    //  }
    //}

    this.compilation = CSharpCompilation.Create(
      assemblyName: "Tests",
      syntaxTrees: syntaxTrees,
      references: references,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));
    //var optionsProvider = options is null ? null : new FixedConfigOptionsProvider(options);

    this.driver = CSharpGeneratorDriver.Create(
      generators: generators,
      parseOptions: parseOptions,
      optionsProvider: /*optionsProvider*/ null);
  }

  [Benchmark]
  public (GeneratorDriver, Compilation, ImmutableArray<Diagnostic>) Run()
  {
    var generatorDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var compilationWithGeneratedCode, out var generatorDiagnostics);

    return (generatorDriver, compilationWithGeneratedCode, generatorDiagnostics);
  }

  private readonly CSharpGeneratorDriver driver;
  private readonly CSharpCompilation compilation;

  private sealed class Config : ManualConfig
  {
    public Config()
    {
      var baseJob = Job.Default;

      AddJob(baseJob.WithEnvironmentVariable("version", "new").WithId("new"));
      AddJob(baseJob.WithEnvironmentVariable("version", "original").WithId("original"));
    }
  }
}
