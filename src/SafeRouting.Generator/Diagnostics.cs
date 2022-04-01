using Microsoft.CodeAnalysis;

namespace SafeRouting.Generator
{
  internal static class Diagnostics
  {
    public static Diagnostic CreateConflictingMethodsDiagnostic(string className, string methodName, Location location)
      => Diagnostic.Create(ConflictingMethodsDescriptor, location, className, methodName);

    public static Diagnostic CreateInvalidOptionDiagnostic(string optionKey, string errorText)
      => Diagnostic.Create(InvalidOptionsDescriptor, location: null, optionKey, errorText);

    public static Diagnostic CreateInvalidIdentifierDiagnostic(string identifier, Location? location)
      => Diagnostic.Create(InvalidIdentifierDescriptor, location, identifier);

    public static Diagnostic CreateConflictingControllerDiagnostic(string controllerName, Location location)
      => Diagnostic.Create(ConflictingControllerDescriptor, location, controllerName);

    public static Diagnostic CreateConflictingPageClassDiagnostic(string pageClassName, Location location)
      => Diagnostic.Create(ConflictingPageClassDescriptor, location, pageClassName);

    public static Diagnostic CreateUnsupportedLanguageVersionDiagnostic()
      => Diagnostic.Create(UnsupportedLanguageVersionDescriptor, location: null);

    private static DiagnosticDescriptor ConflictingMethodsDescriptor => new("CSR0001", "Conflicting methods", "The class '{0}' contains multiple methods which map to the route method '{1}'.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor InvalidOptionsDescriptor => new("CSR0002", "Invalid options", "Value for the option '{0}' is invalid. {1}", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor InvalidIdentifierDescriptor => new("CSR0003", "Invalid identifier", "The text '{0}' is not a valid C# identifier.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor ConflictingControllerDescriptor => new("CSR0004", "Conflicting Controller", "The controller '{0}' conflicts with another controller of the same name.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor ConflictingPageClassDescriptor => new("CSR0005", "Conflicting PageModel Class", "The page class '{0}' conflicts with another page class with the same resulting name.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static DiagnosticDescriptor UnsupportedLanguageVersionDescriptor => new("CSR0006", "Unsupported Language Version", "C# 8 or later is required for route generation.", "Foundation", DiagnosticSeverity.Error, isEnabledByDefault: true);
  }
}
