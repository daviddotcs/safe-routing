using System.Reflection;
using System.Text.RegularExpressions;

namespace SafeRouting.Generator
{
  internal static class CSharpSupport
  {
    public static bool IsValidIdentifier(string value)
      => IdentifierRegex.IsMatch(value);
    public static string CamelToPascalCase(string value)
      => value.Length <= 1
        ? value.ToUpperInvariant()
        : value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
    public static string PascalToCamelCase(string value)
      => value.Length <= 1
        ? value.ToLowerInvariant()
        : value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
    public static string EscapeCharLiteral(char value)
      => value switch
      {
        '\'' => "\\'",
        '\\' => "\\\\",
        _ => value.ToString()
      };
    public static string EscapeStringLiteral(string value)
      => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    public static string EscapeXmlDocType(string value)
      => value.Replace('<', '{').Replace('>', '}');
    public static string GetGeneratedCodeAttribute()
      => $"[global::System.CodeDom.Compiler.GeneratedCode(\"{AssemblyName.Name}\", \"{AssemblyName.Version}\")]";
    public static string GetExcludeFromCodeCoverageAttribute()
      => "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

    private static Regex GetIdentifierRegex()
    {
      // Based on following link, without escape sequences or contextual keywords
      // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure#643-identifiers
      var formattingCharacter = "[\\p{Cf}]";
      var connectingCharacter = "[\\p{Pc}]";
      var decimalDigitCharacter = "[\\p{Nd}]";
      var combiningCharacter = "[\\p{Mn}\\p{Mc}]";
      var letterCharacter = "[\\p{L}\\p{Nl}]";
      var identifierPartCharacter = $"({letterCharacter}|{decimalDigitCharacter}|{connectingCharacter}|{combiningCharacter}|{formattingCharacter})";
      var underscoreCharacter = "_";
      var identifierStartCharacter = $"({letterCharacter}|{underscoreCharacter})";
      var basicIdentifier = $"{identifierStartCharacter}({identifierPartCharacter})*";
      var escapedIdentifier = $"@{basicIdentifier}";
      var availableIdentifier = basicIdentifier;
      var simpleIdentifier = $"({availableIdentifier}|{escapedIdentifier})";
      var identifier = $"^{simpleIdentifier}$";

      return new Regex(identifier, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
    }

    private static Regex IdentifierRegex { get; } = GetIdentifierRegex();
    private static AssemblyName AssemblyName { get; } = Assembly.GetAssembly(typeof(GeneratorSupport)).GetName();
  }
}
