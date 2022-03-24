using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SafeRouting.Generator
{
  internal static class RoslynSupport
  {
    public static bool TryGetOptionalStringArgumentAttribute(this AttributeData attribute, [MaybeNullWhen(false)] out string argumentValue)
    {
      argumentValue = null;

      foreach (var argument in attribute.ConstructorArguments)
      {
        if (argument.Value is string stringValue)
        {
          argumentValue = stringValue;
          return true;
        }
      }

      return false;
    }

    public static MvcBindingSourceInfo ParseBindingSourceAttribute(this AttributeData attribute, MvcBindingSourceType sourceType)
    {
      var name = default(string?);

      foreach (var namedArgument in attribute.NamedArguments)
      {
        switch (namedArgument.Key)
        {
          case "Name":
            if (namedArgument.Value.Value is string nameValue)
            {
              name = nameValue;
            }
            break;
        }
      }

      return new MvcBindingSourceInfo(sourceType, name);
    }

    public static bool ParseRazorPageName(string methodName, [MaybeNullWhen(false)] out string name, out string? handler)
    {
      var methodNameMatch = RazorPageMethodNameRegex.Match(methodName);
      if (!methodNameMatch.Success)
      {
        name = null;
        handler = null;
        return false;
      }

      handler = methodNameMatch.Groups["handler"].Value;
      if (string.IsNullOrEmpty(handler))
      {
        handler = null;
      }

      name = methodNameMatch.Groups["name"].Value;

      return true;
    }

    public static IEnumerable<INamedTypeSymbol> EnumerateSelfAndBaseTypes(this INamedTypeSymbol symbol)
    {
      while (symbol != null)
      {
        yield return symbol;
        symbol = symbol.BaseType!;
      }
    }

    public static SymbolDisplayFormat UniqueClassMemberSymbolDisplayFormat { get; } = new SymbolDisplayFormat(
      globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
      memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
      delegateStyle: SymbolDisplayDelegateStyle.NameAndParameters,
      extensionMethodStyle: SymbolDisplayExtensionMethodStyle.StaticMethod,
      parameterOptions: SymbolDisplayParameterOptions.IncludeType,
      propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
      localOptions: SymbolDisplayLocalOptions.None,
      kindOptions: SymbolDisplayKindOptions.None,
      miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static SymbolDisplayFormat UniqueClassMemberWithNullableAnnotationsSymbolDisplayFormat { get; } = UniqueClassMemberSymbolDisplayFormat
      .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static SymbolDisplayFormat EscapedIdentifierSymbolDisplayFormat { get; } = new SymbolDisplayFormat(
      miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public static SymbolDisplayFormat FullyQualifiedWithAnnotationsFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
      .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static Regex RazorPageMethodNameRegex { get; } = new Regex(@"^On(?<name>(?<verb>Delete|Get|Head|Options|Patch|Post|Put)(?<handler>.*?))(Async)?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
  }
}
