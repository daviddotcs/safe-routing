using Microsoft.CodeAnalysis;

namespace SafeRouting.Generator;

public static class IncrementalGeneratorInitializationContextExtensions
{
  public static void ReportDiagnostics(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<Diagnostic> diagnostics)
  {
    context.RegisterSourceOutput(diagnostics, static (context, diagnostic) => context.ReportDiagnostic(diagnostic));
  }

  public static IncrementalValuesProvider<T> Filter<T>(this IncrementalValuesProvider<T> source, IncrementalValueProvider<bool> conditionalProvider)
  {
    return source
      .Combine(conditionalProvider)
      .Where(static x => x.Right)
      .Select(static (x, _) => x.Left);
  }
}
