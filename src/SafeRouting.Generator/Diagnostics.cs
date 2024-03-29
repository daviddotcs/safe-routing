﻿using Microsoft.CodeAnalysis;

namespace SafeRouting.Generator;

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

  private static readonly DiagnosticDescriptor ConflictingMethodsDescriptor = new(
    id: "CSR0001",
    title: "Conflicting methods",
    messageFormat: "The class '{0}' contains multiple methods which map to the route method '{1}'",
    category: typeof(RouteGenerator).FullName,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor InvalidOptionsDescriptor = new(
    id: "CSR0002",
    title: "Invalid options",
#pragma warning disable RS1032 // Define diagnostic message correctly - errorText is expected to end in a period
    messageFormat: "Value for the option '{0}' is invalid. {1}",
#pragma warning restore RS1032
    category: typeof(RouteGenerator).FullName,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor InvalidIdentifierDescriptor = new(
    id: "CSR0003",
    title: "Invalid identifier",
    messageFormat: "The text '{0}' is not a valid C# identifier",
    category: typeof(RouteGenerator).FullName,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor ConflictingControllerDescriptor = new(
    id: "CSR0004",
    title: "Conflicting Controller",
    messageFormat: "The controller '{0}' conflicts with another controller of the same name",
    category: typeof(RouteGenerator).FullName,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor ConflictingPageClassDescriptor = new(
    id: "CSR0005",
    title: "Conflicting PageModel Class",
    messageFormat: "The page class '{0}' conflicts with another page class with the same resulting name",
    category: typeof(RouteGenerator).FullName,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor UnsupportedLanguageVersionDescriptor = new(
    id: "CSR0006",
    title: "Unsupported Language Version",
    messageFormat: "C# 8 or later is required for route generation",
    category: typeof(RouteGenerator).FullName,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);
}
