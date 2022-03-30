// Run with dotnet-script https://github.com/filipw/dotnet-script

#r "nuget: Markdig, 0.28.1"

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;

#nullable enable

const string NugetPackageName = "SafeRouting";
const string GeneratorProject = "SafeRouting.Generator";
const string IntegrationTestsProject = "SafeRouting.IntegrationTests";
const string NugetIntegrationTestsProject = "SafeRouting.NugetIntegrationTests";

var consoleLock = new object();

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
RewriteNugetIntegrationsTestProject();
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


// ----------------------------------------------------------------------
// Supporting Methods
// ----------------------------------------------------------------------

public string AddMarkdownTableOfContents(string sourceFile)
{
  var text = File.ReadAllText(sourceFile);

  var pipeline = new Markdig.MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .Build();

  var document = Markdig.Markdown.Parse(text, pipeline);
  if (document is null)
  {
    return text;
  }

  var headings = document
    .OfType<Markdig.Syntax.HeadingBlock>()
    .Where(x => x.Level > 1)
    .ToArray();

  if (headings.Length == 0)
  {
    return text;
  }

  var plainTextBuilder = new StringBuilder();
  using var plainTextWriter = new StringWriter(plainTextBuilder);
  var plainTextRenderer = new Markdig.Renderers.HtmlRenderer(plainTextWriter)
  {
    EnableHtmlForBlock = false,
    EnableHtmlForInline = false,
    EnableHtmlEscape = false
  };

  var tocBuilder = new StringBuilder();
  var firstHeadingLocation = -1;

  foreach (var heading in headings)
  {
    var attributes = Markdig.Renderers.Html.HtmlAttributesExtensions.TryGetAttributes(heading);
    if (attributes is null)
    {
      continue;
    }

    if (firstHeadingLocation == -1)
    {
      firstHeadingLocation = heading.Span.Start;
    }

    plainTextBuilder.Clear();
    plainTextRenderer.Render(heading);

    tocBuilder
      .Append(' ', (heading.Level - 2) * 4)
      .Append("- [")
      .Append(plainTextBuilder.ToString().Trim())
      .Append("](#")
      .Append(attributes.Id)
      .AppendLine(")");
  }

  if (firstHeadingLocation == -1)
  {
    return text;
  }

  tocBuilder.AppendLine();

  var resultBuilder = new StringBuilder(text);
  resultBuilder.Insert(firstHeadingLocation, tocBuilder.ToString());
  resultBuilder.Insert(0, Environment.NewLine);
  resultBuilder.Insert(0, "[//]: # (Generated file, do not edit manually. Source: " + sourceFile + ")");

  return resultBuilder.ToString();
}

bool IsWorkingDirectoryClean()
{
  var hasOutput = false;
  var command = "git";
  var arguments = "status --porcelain";
  WriteLine($"> {command} {arguments}{Environment.NewLine}");

  var cmd = new Process();
  cmd.StartInfo = new ProcessStartInfo
  {
    FileName = command,
    Arguments = arguments,
    CreateNoWindow = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    WindowStyle = ProcessWindowStyle.Hidden
  };
  cmd.OutputDataReceived += (_, e) =>
  {
    if (e.Data != null)
    {
      hasOutput = true;
      WriteLine(e.Data, isError: false, GetLineColor(e.Data) ?? ConsoleColor.DarkGray);
    }
  };
  cmd.ErrorDataReceived += (_, e) =>
  {
    if (e.Data != null)
    {
      hasOutput = true;
      WriteLine(e.Data, isError: true, ConsoleColor.Red);
    }
  };

  cmd.Start();
  cmd.BeginOutputReadLine();
  cmd.BeginErrorReadLine();
  cmd.WaitForExit();

  return !hasOutput && cmd.ExitCode == 0;
}

void RewriteNugetIntegrationsTestProject()
{
  var escapedGeneratorProject = GeneratorProject.Replace(".", "\\.");
  var commonProjectReferenceRegex = new Regex(@$"(?<indent>^\s*)<ProjectReference\s+Include=""\.\.\\\.\.\\SafeRouting\.Common\\SafeRouting\.Common\.csproj""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
  var generatorProjectReferenceRegex = new Regex(@$"(?<indent>^\s*)<ProjectReference\s+Include=""\.\.\\\.\.\\{escapedGeneratorProject}\\{escapedGeneratorProject}\.csproj""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
  
  var lines = File.ReadAllLines($"./Test/{IntegrationTestsProject}/{IntegrationTestsProject}.csproj");

  for (var i = 0; i < lines.Length; i++)
  {
    if (commonProjectReferenceRegex.IsMatch(lines[i]))
    {
      lines[i] = "";
      continue;
    }

    var match = generatorProjectReferenceRegex.Match(lines[i]);
    if (!match.Success)
    {
      continue;
    }

    var indent = match.Groups["indent"].Value;
    var replacedLine = $"{indent}<PackageReference Include=\"{NugetPackageName}\" Version=\"{version}\" />" + Environment.NewLine
      + @$"{indent}<Compile Include=""..\{IntegrationTestsProject}\**\*.cs"" Link=""%(Filename)%(Extension)""/>" + Environment.NewLine
      + @$"{indent}<Compile Remove=""..\{IntegrationTestsProject}\bin\**\*""/>" + Environment.NewLine
      + @$"{indent}<Compile Remove=""..\{IntegrationTestsProject}\obj\**\*""/>";

    lines[i] = replacedLine;

    break;
  }

  Directory.CreateDirectory($"./Test/{NugetIntegrationTestsProject}");
  File.WriteAllLines($"./Test/{NugetIntegrationTestsProject}/{NugetIntegrationTestsProject}.csproj", lines);
}

void WriteLine(string? text, bool isError = false, ConsoleColor? color = null)
{
  if (text == null) return;

  lock (consoleLock)
  {
    if (color != null)
    {
      Console.ForegroundColor = color.Value;
    }

    if (isError)
    {
      Console.Error.WriteLine(text);
    }
    else
    {
      Console.WriteLine(text);
    }

    if (color != null)
    {
      Console.ResetColor();
    }
  }
}

ConsoleColor? GetLineColor(string? text)
{
  if (text is null) return null;

  if (text.StartsWith("Passed!"))
    return ConsoleColor.Green;

  if (text.StartsWith("Failed!") || text.Contains(": error "))
    return ConsoleColor.Red;

  if (text.Contains(": warning "))
    return ConsoleColor.Yellow;

  return null;
}

int RunCommand(string command, string arguments)
{
  WriteLine($"> {command} {arguments}{Environment.NewLine}");
  
  var cmd = new Process();
  cmd.StartInfo = new ProcessStartInfo
  {
    FileName = command,
    Arguments = arguments,
    CreateNoWindow = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    WindowStyle = ProcessWindowStyle.Hidden
  };
  cmd.OutputDataReceived += (_, e) => WriteLine(e.Data, isError: false, GetLineColor(e.Data) ?? ConsoleColor.DarkGray);
  cmd.ErrorDataReceived += (_, e) => WriteLine(e.Data, isError: true, ConsoleColor.Red);

  cmd.Start();
  cmd.BeginOutputReadLine();
  cmd.BeginErrorReadLine();
  cmd.WaitForExit();

  return cmd.ExitCode;
}
