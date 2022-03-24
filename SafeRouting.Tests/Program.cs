using System.Runtime.CompilerServices;

namespace SafeRouting.Tests;

public static class ModuleInitializer
{
  [ModuleInitializer]
  public static void Init() => VerifySourceGenerators.Enable();
}