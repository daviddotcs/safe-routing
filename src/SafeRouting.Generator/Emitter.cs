﻿using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Text;

namespace SafeRouting.Generator
{
  internal static class Emitter
  {
    public static string Emit(IEnumerable<IMvcObjectInfo> items, GeneratorOptions options, CancellationToken cancellationToken)
    {
      var builder = new StringBuilder();
      using var stringWriter = new StringWriter(builder);
      using var writer = new IndentedTextWriter(stringWriter, "  ");

      writer.WriteLine("// <auto-generated/>");
      writer.WriteLine();
      writer.WriteLine("#pragma warning disable");
      writer.WriteLine("#nullable enable");
      writer.WriteLine();
      writer.WriteLine($"namespace {options.GeneratedNamespace}");
      writer.WriteLine("{");
      writer.Indent++;

      var itemIndex = 0;

      foreach (var item in items.OrderBy(x => x.OutputClassName))
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (itemIndex++ != 0)
        {
          writer.WriteLine();
        }

        var classNamespace = $"{(item.Area is null ? null : $"Areas.{item.Area}.")}{item.Noun}s";
        var supportNamespace = $"Support.{(item.Area is null ? null : $"{item.Area}_")}{item.Noun}s_{item.OutputClassName}";

        writer.WriteLine($"namespace {classNamespace}");
        writer.WriteLine("{");
        writer.Indent++;

        WriteMethodClass(writer, options, item, supportNamespace);

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
        writer.WriteLine($"namespace {supportNamespace}");
        writer.WriteLine("{");
        writer.Indent++;

        var namespaceItemIndex = 0;

        if (item.Properties.Count > 0)
        {
          WritePropertyDataClass(writer, options, item);
          namespaceItemIndex++;
        }

        foreach (var method in item.Methods)
        {
          if (namespaceItemIndex++ != 0)
          {
            writer.WriteLine();
          }

          WriteRouteValuesClass(writer, options, item, method);
        }

        writer.Indent--;
        writer.WriteLine("}");
      }

      writer.Indent--;
      writer.WriteLine("}");

      return builder.ToString();
    }

    private static void WriteMethod(IndentedTextWriter writer, IMvcObjectInfo item, IMvcMethodInfo method, string uniqueName)
    {
      var parameters = method.GetUrlParameters().ToArray();
      var returnType = $"{uniqueName}RouteValues";

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Generates route values for <see cref=\"{CSharpSupport.EscapeXmlDocType($"{item.FullyQualifiedTypeName}.{method.FullyQualifiedMethodDeclaration}")}\"/>.");
      writer.WriteLine("/// </summary>");
      writer.Write($"public static {returnType} {method.EscapedName}(");

      if (parameters.Length > 0)
      {
        WriteMethodParameters(writer, parameters);
      }

      // Start of method block
      writer.WriteLine(")");
      writer.WriteLine("{");
      writer.Indent++;

      writer.WriteLine($"return new {returnType}(new global::Microsoft.AspNetCore.Routing.RouteValueDictionary()");
      writer.WriteLine("{");
      writer.Indent++;

      writer.Write("[\"area\"] = ");
      if ((method.Area ?? item.Area) is string area)
      {
        CSharpSupport.ToStringLiteralExpression(area).WriteTo(writer);
      }
      else
      {
        writer.Write("\"\"");
      }

      foreach (var parameter in parameters)
      {
        writer.WriteLine(",");
        writer.Write("[");
        CSharpSupport.ToStringLiteralExpression(parameter.RouteKey).WriteTo(writer);
        writer.Write($"] = {parameter.EscapedName}");
      }

      writer.WriteLine();
      writer.Indent--;
      writer.WriteLine("});");

      // End of method block
      writer.Indent--;
      writer.WriteLine("}");
    }
    private static void WriteMethodClass(IndentedTextWriter writer, GeneratorOptions options, IMvcObjectInfo item, string supportNamespace)
    {
      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Generates route values for <see cref=\"{CSharpSupport.EscapeXmlDocType(item.FullyQualifiedTypeName)}\"/>.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine(CSharpSupport.GetGeneratedCodeAttribute());
      writer.WriteLine(CSharpSupport.GetExcludeFromCodeCoverageAttribute());
      writer.WriteLine($"{options.GeneratedAccessModifier} static class {CSharpSupport.EscapeIdentifier(item.OutputClassName)}");
      writer.WriteLine("{");
      writer.Indent++;

      var methodIndex = 0;

      foreach (var method in item.Methods)
      {
        if (methodIndex++ != 0)
        {
          writer.WriteLine();
        }

        WriteMethod(writer, item, method, $"{supportNamespace}.{method.UniqueName}");
      }

      writer.Indent--;
      writer.WriteLine("}");
    }
    private static void WriteMethodParameters(IndentedTextWriter writer, MvcMethodParameterInfo[] parameters)
    {
      var indentLevel = writer.Indent;
      var annotationsEnabled = true;
      var parameterIndex = 0;

      foreach (var parameter in parameters)
      {
        if (parameterIndex++ != 0)
        {
          writer.Write(", ");
        }

        if (parameter.Type.AnnotationsEnabled != annotationsEnabled)
        {
          annotationsEnabled = parameter.Type.AnnotationsEnabled;
          writer.WriteLine();
          writer.Indent = 0;
          writer.WriteLine(annotationsEnabled ? "#nullable restore" : "#nullable disable");
          writer.Indent = indentLevel + 1;
        }

        writer.Write($"{parameter.Type.FullyQualifiedName} {parameter.EscapedName}");

        if (parameter.DefaultValueExpression is not null)
        {
          writer.Write(" = ");
          parameter.DefaultValueExpression.WriteTo(writer);
        }
      }

      if (!annotationsEnabled)
      {
        writer.Indent = 0;
        writer.WriteLine();
        writer.WriteLine("#nullable restore");
      }

      writer.Indent = indentLevel;
    }
    private static void WriteParameterDataClass(IndentedTextWriter writer, GeneratorOptions options, IMvcObjectInfo item, IMvcMethodInfo method)
    {
      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Represents route keys for parameters to <see cref=\"{CSharpSupport.EscapeXmlDocType($"{item.FullyQualifiedTypeName}.{method.FullyQualifiedMethodDeclaration}")}\"/>.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine(CSharpSupport.GetGeneratedCodeAttribute());
      writer.WriteLine(CSharpSupport.GetExcludeFromCodeCoverageAttribute());
      writer.WriteLine($"{options.GeneratedAccessModifier} sealed class ParameterData");
      writer.WriteLine("{");
      writer.Indent++;

      var parameterIndex = 0;
      foreach (var parameter in method.Parameters)
      {
        if (parameterIndex++ != 0)
        {
          writer.WriteLine();
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Route key for the <c>{parameter.OriginalName}</c> parameter in <see cref=\"{CSharpSupport.EscapeXmlDocType($"{item.FullyQualifiedTypeName}.{method.FullyQualifiedMethodDeclaration}")}\"/>.");
        writer.WriteLine("/// </summary>");
        WriteRouteKeyProperty(writer,
          scopeType: "ParameterData",
          valueType: parameter.Type,
          propertyName: parameter.PropertyName,
          routeKeyName: parameter.RouteKey);
      }

      writer.Indent--;
      writer.WriteLine("}");
    }
    private static void WritePropertyDataClass(IndentedTextWriter writer, GeneratorOptions options, IMvcObjectInfo item)
    {
      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Represents route keys for the properties of <see cref=\"{CSharpSupport.EscapeXmlDocType(item.FullyQualifiedTypeName)}\"/>.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine(CSharpSupport.GetGeneratedCodeAttribute());
      writer.WriteLine(CSharpSupport.GetExcludeFromCodeCoverageAttribute());
      writer.WriteLine($"{options.GeneratedAccessModifier} sealed class PropertyData");
      writer.WriteLine("{");
      writer.Indent++;

      var propertyIndex = 0;
      foreach (var property in item.Properties)
      {
        if (propertyIndex++ != 0)
        {
          writer.WriteLine();
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Route key for the property <see cref=\"{CSharpSupport.EscapeXmlDocType(item.FullyQualifiedTypeName)}.{property.EscapedOriginalName}\"/>.");
        writer.WriteLine("/// </summary>");
        WriteRouteKeyProperty(writer,
          scopeType: "PropertyData",
          valueType: property.Type,
          propertyName: property.EscapedName,
          routeKeyName: property.RouteKey);
      }

      writer.Indent--;
      writer.WriteLine("}");
    }
    private static void WriteRouteKeyProperty(IndentedTextWriter writer, string scopeType, TypeInfo valueType, string propertyName, string routeKeyName)
    {
      var indentLevel = writer.Indent;

      if (!valueType.AnnotationsEnabled)
      {
        writer.Indent = 0;
        writer.WriteLine("#nullable disable");
        writer.Indent = indentLevel;
      }

      writer.Write($"public global::{GeneratorSupport.RootNamespace}.RouteKey<{scopeType}, {valueType.FullyQualifiedName}> {propertyName}");
      writer.Write(" { get; } = new global::");
      writer.Write($"{GeneratorSupport.RootNamespace}.RouteKey<{scopeType}, {valueType.FullyQualifiedName}>(");
      CSharpSupport.ToStringLiteralExpression(routeKeyName).WriteTo(writer);
      writer.WriteLine(");");

      if (!valueType.AnnotationsEnabled)
      {
        writer.Indent = 0;
        writer.WriteLine("#nullable restore");
        writer.Indent = indentLevel;
      }
    }
    private static void WriteRouteKeyIndexer(IndentedTextWriter writer, string scopeType, TypeInfo valueType)
    {
      var indentLevel = writer.Indent;

      if (!valueType.AnnotationsEnabled)
      {
        writer.Indent = 0;
        writer.WriteLine("#nullable disable");
        writer.Indent = indentLevel;
      }

      writer.Write($"public {valueType.FullyQualifiedName} this[global::{GeneratorSupport.RootNamespace}.RouteKey<{scopeType}, {valueType.FullyQualifiedName}> key] ");
      writer.WriteLine("{ set => RouteValues[key.Name] = value; }");

      if (!valueType.AnnotationsEnabled)
      {
        writer.Indent = 0;
        writer.WriteLine("#nullable restore");
      }

      writer.Indent = indentLevel;
    }
    private static void WriteRouteValuesClass(IndentedTextWriter writer, GeneratorOptions options, IMvcObjectInfo item, IMvcMethodInfo method)
    {
      var methodClassName = $"{method.UniqueName}RouteValues";

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Represents route values for routes to <see cref=\"{CSharpSupport.EscapeXmlDocType($"{item.FullyQualifiedTypeName}.{method.FullyQualifiedMethodDeclaration}")}\"/>.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine(CSharpSupport.GetGeneratedCodeAttribute());
      writer.WriteLine(CSharpSupport.GetExcludeFromCodeCoverageAttribute());
      writer.WriteLine($"{options.GeneratedAccessModifier} sealed class {methodClassName} : global::{GeneratorSupport.RootNamespace}.I{item.Noun}RouteValues");
      writer.WriteLine("{");
      writer.Indent++;

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Initialises a new instance of the <see cref=\"{methodClassName}\"/> class.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine($"/// <param name=\"routeValues\">The initial values for the route.</param>");
      writer.WriteLine($"public {methodClassName}(global::Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues)");
      writer.WriteLine("{");
      writer.Indent++;
      writer.WriteLine("RouteValues = routeValues;");
      writer.Indent--;
      writer.WriteLine("}");
      writer.WriteLine();

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// The name of the {item.Noun.ToLowerInvariant()} for the route.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine($"public string {item.Noun}Name => \"{item.RouteValue}\";");

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// The name of the {item.DivisionName.ToLowerInvariant()} for the route.");
      writer.WriteLine("/// </summary>");
      writer.Write($"public string{(method.DivisionRouteValue is null ? "?" : null)} {item.DivisionName}Name => ");
      if (method.DivisionRouteValue is null)
      {
        writer.WriteLine("null;");
      }
      else
      {
        CSharpSupport.ToStringLiteralExpression(method.DivisionRouteValue).WriteTo(writer);
        writer.WriteLine(";");
      }

      writer.WriteLine("/// <summary>");
      writer.WriteLine("/// Values for the route.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine("public global::Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues { get; }");

      if (item.Properties.Count > 0)
      {
        WriteRouteValuesClassMembers(writer, "PropertyData", item.Properties.Select(x => x.Type), item.FullyQualifiedTypeName, MemberType.Property);
      }

      if (method.Parameters.Count > 0)
      {
        WriteRouteValuesClassMembers(writer, $"{method.UniqueName}.ParameterData", method.Parameters.Select(x => x.Type), $"{item.FullyQualifiedTypeName}.{method.FullyQualifiedMethodDeclaration}", MemberType.Parameter);
      }

      writer.Indent--;
      writer.WriteLine("}");

      if (method.Parameters.Count > 0)
      {
        writer.WriteLine();
        writer.WriteLine($"namespace {method.UniqueName}");
        writer.WriteLine("{");
        writer.Indent++;

        WriteParameterDataClass(writer, options, item, method);

        writer.Indent--;
        writer.WriteLine("}");
      }
    }
    private static void WriteRouteValuesClassMembers(IndentedTextWriter writer, string memberDataClassName, IEnumerable<TypeInfo> memberTypes, string fullyQualifiedSourceIdentifier, MemberType memberType)
    {
      writer.WriteLine();
      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// {memberType.TitleCasePluralNoun} of <see cref=\"{CSharpSupport.EscapeXmlDocType(fullyQualifiedSourceIdentifier)}\"/> which can be used in the route.");
      writer.WriteLine("/// </summary>");
      writer.Write($"public {memberDataClassName} {memberType.TitleCasePluralNoun}");
      writer.Write(" { get; } = new ");
      writer.WriteLine($"{memberDataClassName}();");

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Removes a {memberType.TitleCaseNoun.ToLowerInvariant()} value from the route.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine("/// <typeparam name=\"T\">The type of values applicable to the key.</typeparam>");
      writer.WriteLine("/// <param name=\"key\">The key for the route.</param>");
      writer.WriteLine("/// <returns><see langword=\"true\"/> if the element is successfully found and removed; otherwise <see langword=\"false\"/>.</returns>");
      writer.WriteLine($"public bool Remove<T>(global::{GeneratorSupport.RootNamespace}.RouteKey<{memberDataClassName}, T> key) => RouteValues.Remove(key.Name);");

      writer.WriteLine("/// <summary>");
      writer.WriteLine($"/// Sets a {memberType.TitleCaseNoun.ToLowerInvariant()} value for the route.");
      writer.WriteLine("/// </summary>");
      writer.WriteLine("/// <typeparam name=\"T\">The type of values applicable to the key.</typeparam>");
      writer.WriteLine("/// <param name=\"key\">The key for the route.</param>");
      writer.WriteLine("/// <param name=\"value\">The value for the route.</param>");
      writer.WriteLine($"public void Set<T>(global::{GeneratorSupport.RootNamespace}.RouteKey<{memberDataClassName}, T> key, T value) => RouteValues[key.Name] = value;");

      var parameterTypes = ConsolidateTypes(memberTypes);

      foreach (var parameterType in parameterTypes.OrderBy(x => x.FullyQualifiedName))
      {
        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Sets a {memberType.TitleCaseNoun.ToLowerInvariant()} value for the route.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine("/// <param name=\"key\">The key for the route.</param>");
        WriteRouteKeyIndexer(writer, scopeType: memberDataClassName, valueType: parameterType);
      }
    }
    private static List<TypeInfo> ConsolidateTypes(IEnumerable<TypeInfo> types)
    {
      // 1. Group all types by their names without nullable reference type annotations.
      // 2. If any within the group are from an annotation disabled context, take that and ignore the rest.
      // 3. If all remaining types are identical, return that, otherwise fall back to a nullable disabled context.

      var results = new List<TypeInfo>();

      foreach (var typeGroup in types.GroupBy(x => x.FullyQualifiedNameSansAnnotations, StringComparer.Ordinal))
      {
        var resultType = default(TypeInfo);

        foreach (var type in typeGroup)
        {
          if (!type.AnnotationsEnabled)
          {
            resultType = type;
            break;
          }

          if (resultType is null)
          {
            resultType = type;
            continue;
          }
          
          if (!string.Equals(resultType.FullyQualifiedName, type.FullyQualifiedName, StringComparison.Ordinal))
          {
            resultType = type with { FullyQualifiedName = type.FullyQualifiedNameSansAnnotations, AnnotationsEnabled = false };
          }
        }

        results.Add(resultType!);
      }

      return results;
    }

    private sealed class MemberType
    {
      private MemberType(string titleCaseNoun, string titleCasePluralNoun)
      {
        TitleCaseNoun = titleCaseNoun;
        TitleCasePluralNoun = titleCasePluralNoun;
      }

      public string TitleCaseNoun { get; }
      public string TitleCasePluralNoun { get; }

      public static MemberType Parameter { get; } = new("Parameter", "Parameters");
      public static MemberType Property { get; } = new("Property", "Properties");
    }
  }
}
