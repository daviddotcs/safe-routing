// Run with dotnet-script https://github.com/filipw/dotnet-script

#load "lib.csx"

using System;

#nullable enable

// Set current directory to the src folder so the build script can be invoked from any directory
var sourceDirectory = new FileInfo(FileHelper.GetCallerFilePath()).Directory!;
Environment.CurrentDirectory = sourceDirectory.FullName;


Console.WriteLine();
Console.WriteLine("Updating README.md...");
Console.WriteLine();
MarkdownHelper.GenerateMarkdownFiles("README.source.md", "../README.md", "./Demo/SafeRouting.Demo");
ConsoleHelper.WriteLine("Done", color: ConsoleColor.DarkGray);

Console.WriteLine();
Console.WriteLine("Updating copyright...");
Console.WriteLine();
ProjectHelper.UpdateCopyright();
ConsoleHelper.WriteLine("Done", color: ConsoleColor.DarkGray);
