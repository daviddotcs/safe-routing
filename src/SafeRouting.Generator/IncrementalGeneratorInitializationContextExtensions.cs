using Microsoft.CodeAnalysis;

namespace SafeRouting.Generator;

public static class IncrementalGeneratorInitializationContextExtensions
{
  public static void ReportDiagnostics(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<Diagnostic> diagnostics)
  {
    context.RegisterSourceOutput(diagnostics, static (context, diagnostic) => context.ReportDiagnostic(diagnostic));
  }
}
