﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SafeRouting.Tests.Unit;

internal static class TestHelper
{
  public static Task Verify(string source, string path = "", LanguageVersion languageVersion = LanguageVersion.Latest, NullableContextOptions nullableContextOptions = NullableContextOptions.Enable, TestConfigOptions? options = null, object?[]? parameters = null, bool testCompilation = true, AdditionalSource[]? additionalSources = null)
  {
    var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);

    var syntaxTrees = CreateSyntaxTrees(source, path, parseOptions, additionalSources);
    var compilation = CreateCompilation(syntaxTrees, nullableContextOptions);
    var driver = CreateDriver(options, parseOptions);

    var verifySettings = new VerifySettings();
    verifySettings.UseDirectory("Snapshots");

    if (parameters is not null)
    {
      verifySettings.UseParameters(parameters);
    }

    var generatorDriver = testCompilation
      ? RunGeneratorsAndTestCompilation(driver, compilation, path, additionalSources, verifySettings)
      : driver.RunGenerators(compilation);

    return Verifier.Verify(generatorDriver, verifySettings);
  }

  public static string MakePath(params string[] pathSegments)
  {
    return Path.Combine(pathSegments.Prepend(PathRoot).ToArray());
  }

  public static AdditionalSource GetFromKeyedServicesAttributeAdditionalSource()
  {
    return new("""
      #if !NET8_0_OR_GREATER
      using System;

      namespace Microsoft.Extensions.DependencyInjection
      {
        [AttributeUsage(AttributeTargets.Parameter)]
        public class FromKeyedServicesAttribute : Attribute
        {
          public FromKeyedServicesAttribute(object key) => Key = key;

          public object Key { get; }
        }
      }
      #endif
      """);
  }

  private static string PathRoot { get; } = Path.GetPathRoot(Environment.CurrentDirectory)!;

  // There's probably a less heavy-handed way of providing required ASP.NET Core assemblies
  private static PortableExecutableReference[] References { get; } =
    (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)!
      .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
      .Select(x => MetadataReference.CreateFromFile(x))
      .ToArray();

  private static ISourceGenerator[] Generators { get; } = new[]
  {
    new SafeRouting.Generator.RouteGenerator().AsSourceGenerator()
  };

  private static List<SyntaxTree> CreateSyntaxTrees(string source, string path, CSharpParseOptions parseOptions, AdditionalSource[]? additionalSources)
  {
    var syntaxTree = CSharpSyntaxTree.ParseText(
      source,
      path: path,
      options: parseOptions);

    var syntaxTrees = new List<SyntaxTree>(1 + (additionalSources?.Length ?? 0)) { syntaxTree };

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

    return syntaxTrees;
  }

  private static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees, NullableContextOptions nullableContextOptions)
  {
    var compilation = CSharpCompilation.Create(
      assemblyName: "Tests",
      syntaxTrees: syntaxTrees,
      references: References,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));

    return compilation;
  }

  private static CSharpGeneratorDriver CreateDriver(TestConfigOptions? options, CSharpParseOptions parseOptions)
  {
    var optionsProvider = options is null ? null : new FixedConfigOptionsProvider(options);

    var driver = CSharpGeneratorDriver.Create(
      generators: Generators,
      parseOptions: parseOptions,
      optionsProvider: optionsProvider);

    return driver;
  }

  private static GeneratorDriver RunGeneratorsAndTestCompilation(CSharpGeneratorDriver driver, CSharpCompilation compilation, string path, AdditionalSource[]? additionalSources, VerifySettings verifySettings)
  {
    var generatorDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var compilationWithGeneratedCode, out var generatorDiagnostics);

    using var stream = new MemoryStream();
    var emitResult = compilationWithGeneratedCode.Emit(stream);

    Assert.True(emitResult.Success, $"C# compilation failed with diagnostics:{Environment.NewLine}{string.Join(Environment.NewLine, emitResult.Diagnostics.Select(x => CSharpDiagnosticFormatter.Instance.Format(x, formatter: null)))}");
    Assert.Empty(emitResult.Diagnostics.Where(x => x.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error));

    if ((path.Length > 0 || (additionalSources?.Any(x => x.Path.Length > 0) ?? false)) && generatorDiagnostics.Length > 0)
    {
      verifySettings.UniqueForOSPlatform();
    }

    return generatorDriver;
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

  public override ICollection<string> Keys => InternalDictionary.Keys;
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
