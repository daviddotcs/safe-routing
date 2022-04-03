using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SafeRouting.Generator;

/// <summary>
/// Rewrites expressions representing the default values of parameters so they
/// can be transplanted into the generated source. This includes collapsing
/// <c>nameof</c> operations into their resulting literal strings,
/// fully-qualifying any referenced types, and removing all trivia such as
/// comments and whitespace.
/// </summary>
public sealed class DefaultValueExpressionRewriter : CSharpSyntaxRewriter
{
  public DefaultValueExpressionRewriter(SemanticModel semanticModel, CancellationToken cancellationToken)
  {
    this.semanticModel = semanticModel;
    this.cancellationToken = cancellationToken;
  }

  public override SyntaxNode? Visit(SyntaxNode? node)
    => base.Visit(node)?.WithoutTrivia();

  public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
  {
    var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
    var fullyQualifiedName = symbolInfo.Symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    if (fullyQualifiedName is null)
    {
      return base.VisitIdentifierName(node);
    }

    return SyntaxFactory.IdentifierName(fullyQualifiedName);
  }

  public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
  {
    var operation = semanticModel.GetOperation(node, cancellationToken);

    if (operation?.Kind == OperationKind.NameOf
      && operation.ConstantValue.HasValue
      && operation.ConstantValue.Value is string stringValue)
    {
      return CSharpSupport.ToStringLiteralExpression(stringValue);
    }

    return base.VisitInvocationExpression(node);
  }

  public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    => default;

  private readonly SemanticModel semanticModel;
  private readonly CancellationToken cancellationToken;
}
