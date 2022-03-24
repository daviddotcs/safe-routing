namespace SafeRouting;

/// <summary>
/// Excludes the target from inclusion in generated routing code.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ExcludeFromRouteGeneratorAttribute : Attribute
{
}

/// <summary>
/// Renames the target in generated routing code.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class RouteGeneratorNameAttribute : Attribute
{
  /// <summary>
  /// Renames the target in generated routing code.
  /// </summary>
  /// <param name="name">The new name for the target in generated code. This must be a valid C# identifier.</param>
  public RouteGeneratorNameAttribute(string name)
  {
    Name = name;
  }

  /// <summary>
  /// The new name for the target in generated code.
  /// </summary>
  public string Name { get; }
}
