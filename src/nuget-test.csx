// Run with dotnet-script https://github.com/filipw/dotnet-script

#load "lib.csx"

using System;

#nullable enable

if (Args.Count != 1)
{
  ConsoleHelper.WriteLine($"ERROR: This script must be run with a single argument which is the version number of the build. E.g.;{Environment.NewLine}dotnet script nuget-test.csx -- 1.0.0", isError: true, ConsoleColor.Red);
  Environment.Exit(1);
}

var version = Args[0];

Console.WriteLine($"Build version: {version}");

Steps.SetCurrentDirectory();

Steps.CreateNugetTestProject(version);
Steps.RestoreNugetTestProject();
Steps.BuildNugetTestProject();
Steps.RunNugetTestProject();
