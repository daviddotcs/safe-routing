// Run with dotnet-script https://github.com/filipw/dotnet-script

#load "lib.csx"

using System;

#nullable enable

if (Args.Count != 1)
{
  ConsoleHelper.WriteLine($"ERROR: This script must be run with a single argument which is the version number of the build. E.g.;{Environment.NewLine}dotnet script build.csx -- 1.0.0", isError: true, ConsoleColor.Red);
  Environment.Exit(1);
}

var version = Args[0];

Console.WriteLine($"Build version: {version}");

Steps.SetCurrentDirectory();

Steps.UpdateReadme();
Steps.UpdateCopyright();
Steps.EnforceCleanWorkingDirectory();
Steps.CreateNugetPackage(version);
Steps.CreateNugetTestProject(version);
Steps.RestoreNugetTestProject();
Steps.BuildNugetTestProject();
Steps.RunNugetTestProject();
Steps.AddGitTag(version);

var packageFileInfo = new FileInfo($"{Constants.ArtifactsPath}/{Constants.NugetPackageName}.{version}.nupkg");
ConsoleHelper.WriteLine($"Successfully packaged {version} to:{Environment.NewLine}{packageFileInfo.FullName}", color: ConsoleColor.Green);
