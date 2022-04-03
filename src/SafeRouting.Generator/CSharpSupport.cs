using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    public static string EscapeIdentifier(string identifier)
      => IdentifierRequiresEscaping(identifier)
        ? $"@{identifier}"
        : identifier;

    public static string EscapeXmlDocType(string value)
      => value.Replace('<', '{').Replace('>', '}');

    public static string GetGeneratedCodeAttribute()
      => $"[global::System.CodeDom.Compiler.GeneratedCode(\"{AssemblyName.Name}\", \"{AssemblyName.Version}\")]";

    public static string GetExcludeFromCodeCoverageAttribute()
      => "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

    public static LiteralExpressionSyntax ToStringLiteralExpression(string value)
      => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));

    private static readonly AssemblyName AssemblyName = Assembly.GetAssembly(typeof(GeneratorSupport)).GetName();
  }
}
