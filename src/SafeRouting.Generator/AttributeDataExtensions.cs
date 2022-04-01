using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace SafeRouting.Generator
{
  internal static class AttributeDataExtensions
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
  }
}
