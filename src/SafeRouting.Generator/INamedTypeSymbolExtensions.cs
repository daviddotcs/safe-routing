using Microsoft.CodeAnalysis;

namespace SafeRouting.Generator
{
  internal static class INamedTypeSymbolExtensions
  {
    public static IEnumerable<INamedTypeSymbol> EnumerateSelfAndBaseTypes(this INamedTypeSymbol symbol)
    {
      for (; symbol != null; symbol = symbol.BaseType!)
      {
        yield return symbol;
      }
    }
  }
}
