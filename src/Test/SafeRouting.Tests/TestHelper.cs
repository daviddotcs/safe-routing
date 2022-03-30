using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SafeRouting.Tests;

internal static class TestHelper
{
  public static Task Verify(string source, string path = "", LanguageVersion languageVersion = LanguageVersion.Latest, NullableContextOptions nullableContextOptions = NullableContextOptions.Enable, TestConfigOptions? options = null, object?[]? parameters = null, bool testCompilation = true, AdditionalSource[]? additionalSources = null)
  {
    var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);

    var syntaxTree = CSharpSyntaxTree.ParseText(
      source,
      path: path,
      options: parseOptions);

    var syntaxTrees = new List<SyntaxTree> { syntaxTree };

    if (additionalSources is not null)
    {
      foreach (var additionalSource in additionalSources)
      {
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(
          additionalSource.Source,
          path: additionalSource.Path,
          options: additionalSource.ParseOptions ?? parseOptions));
      }
    }

    // There's probably a less heavy-handed way of providing required ASP.NET Core assemblies
    var references = AppDomain.CurrentDomain.GetAssemblies()
      .Where(a => !a.IsDynamic)
      .Select(a => MetadataReference.CreateFromFile(a.Location));

    var compilation = CSharpCompilation.Create(
      assemblyName: "Tests",
      syntaxTrees: syntaxTrees,
      references: references,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));

    var optionsProvider = options is null ? null : new FixedConfigOptionsProvider(options);

    var driver = CSharpGeneratorDriver.Create(
      generators: new[]
      {
        new SafeRouting.Generator.RouteGenerator().AsSourceGenerator()
      },
      parseOptions: parseOptions,
      optionsProvider: optionsProvider);

    var generatorDriver = driver.RunGenerators(compilation);

    var verifySettings = new VerifySettings();
    verifySettings.UseDirectory("Snapshots");
    if (parameters is not null)
    {
      verifySettings.UseParameters(parameters);
    }

    if (testCompilation)
    {
      generatorDriver.RunGeneratorsAndUpdateCompilation(compilation, out var compilationWithGeneratedCode, out var generatorDiagnostics);

      using var stream = new MemoryStream();
      var emitResult = compilationWithGeneratedCode.Emit(stream);

      Assert.True(emitResult.Success, $"C# compilation failed with diagnostics:{Environment.NewLine}{string.Join(Environment.NewLine, emitResult.Diagnostics.Select(x => CSharpDiagnosticFormatter.Instance.Format(x)))}");
      Assert.Empty(emitResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Warning || x.Severity == DiagnosticSeverity.Error));
    }

    return Verifier.Verify(generatorDriver, verifySettings);
  }
}

internal sealed record AdditionalSource(string Source, string Path = "", CSharpParseOptions? ParseOptions = null);

internal sealed class FixedConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
  public FixedConfigOptionsProvider(AnalyzerConfigOptions options)
  {
    Options = options;
  }

  public override AnalyzerConfigOptions GlobalOptions => Options;
  public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Options;
  public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Options;

  private readonly AnalyzerConfigOptions Options;
}

internal sealed class TestConfigOptions : AnalyzerConfigOptions, IDictionary<string, string>
{
  public string this[string key] { get => InternalDictionary[key]; set => InternalDictionary[key] = value; }

  public ICollection<string> Keys => InternalDictionary.Keys;
  public ICollection<string> Values => InternalDictionary.Values;
  public int Count => InternalDictionary.Count;
  public bool IsReadOnly => false;

  public void Add(string key, string value) => InternalDictionary.Add(key, value);
  public void Add(KeyValuePair<string, string> item) => (InternalDictionary as IDictionary<string, string>).Add(item);
  public void Clear() => InternalDictionary.Clear();
  public bool Contains(KeyValuePair<string, string> item) => InternalDictionary.Contains(item);
  public bool ContainsKey(string key) => InternalDictionary.ContainsKey(key);
  public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => (InternalDictionary as IDictionary<string, string>).CopyTo(array, arrayIndex);
  public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => InternalDictionary.GetEnumerator();
  public bool Remove(string key) => InternalDictionary.Remove(key);
  public bool Remove(KeyValuePair<string, string> item) => (InternalDictionary as IDictionary<string, string>).Remove(item);
  public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => InternalDictionary.TryGetValue(key, out value);

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => InternalDictionary.GetEnumerator();

  private readonly Dictionary<string, string> InternalDictionary = new(KeyComparer);
}
