using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace SafeRouting.Generator
{
  [Generator(LanguageNames.CSharp)]
  public sealed class RouteGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      var supportedLanguageVersionProvider = context.ParseOptionsProvider
        .Select(static (x, _) => (x as CSharpParseOptions)?.LanguageVersion >= LanguageVersion.CSharp8);

      context.RegisterSourceOutput(supportedLanguageVersionProvider, static (context, isSupportedLanguageVersion) =>
      {
        if (!isSupportedLanguageVersion)
        {
          context.ReportDiagnostic(Diagnostics.CreateUnsupportedLanguageVersionDiagnostic());
        }
      });

      var optionsProvider = context.AnalyzerConfigOptionsProvider
        .Select(static (x, _) => Parser.GetOptions(x));

      context.RegisterSourceOutput(optionsProvider, static (context, value) =>
      {
        foreach (var diagnostic in value.Diagnostics)
        {
          context.ReportDiagnostic(diagnostic);
        }
      });

      IncrementalValuesProvider<CandidateClassInfo> candidateClassProvider = context.SyntaxProvider
        .CreateSyntaxProvider(static (x, _) => Parser.IsCandidateNode(x), Parser.TransformCandidateClassNode)
        .Where(static x => x is not null)!;

      var controllerClassProvider = candidateClassProvider
        .Where(static x => x.IsController)
        .Collect()
        .Combine(optionsProvider);

      context.RegisterSourceOutput(controllerClassProvider, ProduceControllerSourceOutput);

      var pageClassProvider = candidateClassProvider
        .Where(static x => x.IsPage)
        .Collect()
        .Combine(optionsProvider);

      context.RegisterSourceOutput(pageClassProvider, ProducePageSourceOutput);
    }

    private static void ProduceControllerSourceOutput(SourceProductionContext context, (ImmutableArray<CandidateClassInfo> Left, GeneratorOptions Right) values)
    {
      var (candidateClasses, options) = values;

      var controllers = candidateClasses
        .Distinct(CandidateClassInfoEqualityComparer.Default)
        .Select(x => Parser.GetControllerInfo(x, context))
        .OfType<ControllerInfo>()
        .ToArray();

      if (controllers.Length == 0)
      {
        return;
      }

      var controllerNames = new HashSet<string>(StringComparer.Ordinal);
      var emitControllers = new List<ControllerInfo>();

      foreach (var controller in controllers)
      {
        if (!controllerNames.Add($"{controller.Area} {controller.Name}"))
        {
          context.ReportDiagnostic(Diagnostics.CreateConflictingControllerDiagnostic(controller.Name, controller.TypeDeclarationSyntax.GetLocation()));
          continue;
        }
        
        emitControllers.Add(controller);
      }

      var source = Emitter.Emit(emitControllers, options, context.CancellationToken);

      context.AddSource("ControllerRoutes.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void ProducePageSourceOutput(SourceProductionContext context, (ImmutableArray<CandidateClassInfo> Left, GeneratorOptions Right) values)
    {
      var (candidateClasses, options) = values;

      var pages = candidateClasses
        .Distinct(CandidateClassInfoEqualityComparer.Default)
        .Select(x => Parser.GetPageInfo(x, context))
        .OfType<PageInfo>()
        .ToArray();

      if (pages.Length == 0)
      {
        return;
      }

      var pageIdentifiers = new HashSet<string>(StringComparer.Ordinal);
      var emitPages = new List<PageInfo>();

      foreach (var page in pages)
      {
        if (!pageIdentifiers.Add($"{page.Area}_{page.PageNamespace}_{page.Name}"))
        {
          context.ReportDiagnostic(Diagnostics.CreateConflictingPageClassDiagnostic(page.FullyQualifiedTypeName, page.TypeDeclarationSyntax.GetLocation()));
          continue;
        }

        emitPages.Add(page);
      }

      var source = Emitter.Emit(emitPages, options, context.CancellationToken);

      context.AddSource("PageRoutes.g.cs", SourceText.From(source, Encoding.UTF8));
    }
  }
}
