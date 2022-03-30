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
          context.ReportDiagnostic(Parser.CreateUnsupportedLanguageVersionDiagnostic());
        }
      });

      var optionsProvider = context.AnalyzerConfigOptionsProvider
        .Select(static (x, _) => Parser.GetOptions(x));

      context.RegisterSourceOutput(optionsProvider, static (context, value) =>
      {
        foreach (var error in value.OptionErrors)
        {
          context.ReportDiagnostic(Parser.CreateInvalidOptionDiagnostic(error.Key, error.Value));
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
        .GroupBy(x => x.TypeDeclarationSyntax)
        .Select(x => Parser.GetControllerInfo(x.First(), context))
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
          context.ReportDiagnostic(Parser.CreateConflictingControllerDiagnostic(controller.Name, controller.TypeDeclarationSyntax.GetLocation()));
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
        .GroupBy(x => x.TypeDeclarationSyntax)
        .Select(x => Parser.GetPageInfo(x.First(), context))
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
          context.ReportDiagnostic(Parser.CreateConflictingPageClassDiagnostic(page.FullyQualifiedTypeName, page.TypeDeclarationSyntax.GetLocation()));
          continue;
        }

        emitPages.Add(page);
      }

      var source = Emitter.Emit(emitPages, options, context.CancellationToken);

      context.AddSource("PageRoutes.g.cs", SourceText.From(source, Encoding.UTF8));
    }
  }
}
