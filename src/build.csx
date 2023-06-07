// Run with dotnet-script https://github.com/filipw/dotnet-script

#load "lib.csx"

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#nullable enable

const string NugetPackageName = "SafeRouting";
const string GeneratorProject = "SafeRouting.Generator";
const string IntegrationTestsProject = "SafeRouting.Tests.Integration";
const string NugetIntegrationTestsProject = "SafeRouting.Tests.NugetIntegration";

// Set current directory to the src folder so the build script can be invoked from any directory
var sourceDirectory = new FileInfo(GetCallerFilePath()).Directory!;
Environment.CurrentDirectory = sourceDirectory.FullName;

if (Args.Count != 1)
{
  WriteLine($"ERROR: This script must be run with a single argument which is the version number of the build. E.g.;{Environment.NewLine}dotnet script build.csx -- 1.0.0", isError: true, ConsoleColor.Red);
  Environment.Exit(1);
}

var version = Args[0];

Console.WriteLine($"Build version: {version}");


Console.WriteLine();
Console.WriteLine("Updating README.md...");
Console.WriteLine();
var markdownContent = AddMarkdownTableOfContents("README.source.md");
File.WriteAllText("../README.md", markdownContent);
WriteLine("Done", color: ConsoleColor.DarkGray);

Console.WriteLine();
Console.WriteLine("Updating copyright...");
Console.WriteLine();
UpdateCopyright();
WriteLine("Done", color: ConsoleColor.DarkGray);

Console.WriteLine();
Console.WriteLine("Checking working directory...");
Console.WriteLine();
if (!IsWorkingDirectoryClean())
{
  WriteLine($"ERROR: The working directory must be clean first. Commit or stash any changes.", isError: true, ConsoleColor.Red);
  Environment.Exit(1);
}
WriteLine("Done", color: ConsoleColor.DarkGray);

Console.WriteLine();
Console.WriteLine("Creating nuget package...");
Console.WriteLine();
if (RunCommand("dotnet", $"pack ./{GeneratorProject} -c Release -o ./artifacts /p:ContinuousIntegrationBuild=true -p:Version={version}") != 0)
{
  Environment.Exit(1);
}

Console.WriteLine();
Console.WriteLine("Rewriting nuget integration test project for new package...");
Console.WriteLine();
RewriteNugetIntegrationsTestProject(GeneratorProject, IntegrationTestsProject, NugetIntegrationTestsProject, NugetPackageName, version);
WriteLine("Done", color: ConsoleColor.DarkGray);

Console.WriteLine();
Console.WriteLine("Restoring packages for nuget integration tests...");
Console.WriteLine();
if (RunCommand("dotnet", $"restore ./Test/{NugetIntegrationTestsProject} --packages ./packages --configfile \"nuget.integration-tests.config\"") != 0)
{
  Environment.Exit(1);
}

Console.WriteLine();
Console.WriteLine("Building nuget integration tests...");
Console.WriteLine();
if (RunCommand("dotnet", $"build ./Test/{NugetIntegrationTestsProject} -c Release --packages ./packages --no-restore") != 0)
{
  Environment.Exit(1);
}

Console.WriteLine();
Console.WriteLine("Running nuget integration tests...");
Console.WriteLine();
if (RunCommand("dotnet", $"test ./Test/{NugetIntegrationTestsProject} -c Release --no-build --no-restore") != 0)
{
  Environment.Exit(1);
}

Console.WriteLine();
Console.WriteLine("Adding git tag...");
Console.WriteLine();
RunCommand("git", $"tag v{version}");

var packageFileInfo = new FileInfo($"./artifacts/{NugetPackageName}.{version}.nupkg");
WriteLine($"Successfully packaged {version} to:{Environment.NewLine}{packageFileInfo.FullName}", color: ConsoleColor.Green);
