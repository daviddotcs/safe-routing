using System.Runtime.CompilerServices;

namespace SafeRouting.Tests.Unit;

public static class ModuleInitializer
{
  [ModuleInitializer]
  public static void Init() => VerifySourceGenerators.Initialize();
}
