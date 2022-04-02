using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace SafeRouting.Generator
{
  internal static class CSharpSupport
  {
    public static string CamelToPascalCase(string value)
      => value.Length <= 1
        ? value.ToUpperInvariant()
        : value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);

    public static bool IdentifierRequiresEscaping(string identifier)
    {
      var kind = SyntaxFacts.GetKeywordKind(identifier);
      
      return kind == SyntaxKind.None
        ? SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
        : SyntaxFacts.IsReservedKeyword(kind);
    }

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

    public static string EscapeIdentifier(string identifier)
      => IdentifierRequiresEscaping(identifier)
        ? $"@{identifier}"
        : identifier;

    public static string EscapeStringLiteral(string value)
      => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    public static string EscapeXmlDocType(string value)
      => value.Replace('<', '{').Replace('>', '}');

    public static string GetGeneratedCodeAttribute()
      => $"[global::System.CodeDom.Compiler.GeneratedCode(\"{AssemblyName.Name}\", \"{AssemblyName.Version}\")]";

    public static string GetExcludeFromCodeCoverageAttribute()
      => "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

    private static AssemblyName AssemblyName { get; } = Assembly.GetAssembly(typeof(GeneratorSupport)).GetName();
  }
}
