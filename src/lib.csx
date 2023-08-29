#r "nuget: Markdig, 0.31.0"

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Markdig;

#nullable enable

public static class Constants
{
  public const string SolutionName = "SafeRouting";

  public const string NugetPackageName = "SafeRouting";

  public const string CommonProjectName = $"{SolutionName}.Common";
  public const string GeneratorProjectName = $"{SolutionName}.Generator";
  public const string IntegrationTestsProjectName = $"{SolutionName}.Tests.Integration";

  public const string ArtifactsPath = "./artifacts";
  public const string GeneratorProjectPath = $"./{GeneratorProjectName}";
  public const string IntegrationTestsProjectPath = $"./Test/{IntegrationTestsProjectName}";
  public const string NugetConfigFile = "nuget.integration-tests.config";
}

public static class Steps
{
  /// <summary>
  /// Set current directory to ./src so the build scripts can be invoked from any directory
  /// </summary>
  public static void SetCurrentDirectory()
  {
    var sourceDirectory = new FileInfo(FileHelper.GetCallerFilePath()).Directory!;
    Environment.CurrentDirectory = sourceDirectory.FullName;
  }

  public static void UpdateReadme()
  {
    Console.WriteLine();
    Console.WriteLine("Updating README.md...");
    Console.WriteLine();
    MarkdownHelper.GenerateMarkdownFiles("README.source.md", "../README.md", "./Demo/SafeRouting.Demo");
    ConsoleHelper.WriteLine("Done", color: ConsoleColor.DarkGray);
  }

  public static void UpdateCopyright()
  {
    Console.WriteLine();
    Console.WriteLine("Updating copyright...");
    Console.WriteLine();
    ProjectHelper.UpdateCopyright();
    ConsoleHelper.WriteLine("Done", color: ConsoleColor.DarkGray);
  }

  public static void EnforceCleanWorkingDirectory()
  {
    Console.WriteLine();
    Console.WriteLine("Checking working directory...");
    Console.WriteLine();
    if (!GitHelper.IsWorkingDirectoryClean())
    {
      ConsoleHelper.WriteLine($"ERROR: The working directory must be clean first. Commit or stash any changes.", isError: true, ConsoleColor.Red);
      Environment.Exit(1);
    }
    ConsoleHelper.WriteLine("Done", color: ConsoleColor.DarkGray);
  }

  public static void CreateNugetPackage(string version)
  {
    Console.WriteLine();
    Console.WriteLine("Creating NuGet package...");
    Console.WriteLine();
    if (ConsoleHelper.RunCommand("dotnet", $"pack {Constants.GeneratorProjectPath} -c Release -o {Constants.ArtifactsPath} /p:ContinuousIntegrationBuild=true -p:Version={version}") != 0)
    {
      Environment.Exit(1);
    }
  }

  public static void RestoreNugetTestProject(string version)
  {
    Console.WriteLine();
    Console.WriteLine("Restoring packages for NuGet integration tests...");
    Console.WriteLine();
    var result = ConsoleHelper.RunCommand("dotnet", $"""
  restore {Constants.IntegrationTestsProjectPath} --packages ./packages --configfile "{Constants.NugetConfigFile}" -p:SafeRoutingPackageVersion={version}
  """);
    if (result != 0)
    {
      Environment.Exit(1);
    }
  }

  public static void BuildNugetTestProject(string version)
  {
    Console.WriteLine();
    Console.WriteLine("Building NuGet integration tests...");
    Console.WriteLine();
    if (ConsoleHelper.RunCommand("dotnet", $"build {Constants.IntegrationTestsProjectPath} -c Release --packages ./packages --no-restore -p:SafeRoutingPackageVersion={version}") != 0)
    {
      Environment.Exit(1);
    }
  }

  public static void RunNugetTestProject()
  {
    Console.WriteLine();
    Console.WriteLine("Running NuGet integration tests...");
    Console.WriteLine();
    if (ConsoleHelper.RunCommand("dotnet", $"test {Constants.IntegrationTestsProjectPath} -c Release --no-build --no-restore") != 0)
    {
      Environment.Exit(1);
    }
  }

  public static void AddGitTag(string version)
  {
    Console.WriteLine();
    Console.WriteLine("Adding git tag...");
    Console.WriteLine();
    ConsoleHelper.RunCommand("git", $"tag v{version}");
  }
}

public static class ConsoleHelper
{
  public static void WriteLine(string? text, bool isError = false, ConsoleColor? color = null)
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

  public static int RunCommand(string command, string arguments)
  {
    ConsoleHelper.WriteLine($"> {command} {arguments}{Environment.NewLine}");

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
    cmd.OutputDataReceived += (_, e) => ConsoleHelper.WriteLine(e.Data, isError: false, ConsoleHelper.GetColorForLine(e.Data) ?? ConsoleColor.DarkGray);
    cmd.ErrorDataReceived += (_, e) => ConsoleHelper.WriteLine(e.Data, isError: true, ConsoleColor.Red);

    cmd.Start();
    cmd.BeginOutputReadLine();
    cmd.BeginErrorReadLine();
    cmd.WaitForExit();

    return cmd.ExitCode;
  }

  public static ConsoleColor? GetColorForLine(string? text)
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

  private static readonly object consoleLock = new object();
}

public static class MarkdownHelper
{
  public static string AddMarkdownTableOfContents(string sourceFile)
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

    tocBuilder.AppendLine("## Table of Contents").AppendLine();

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

  public static void GenerateMarkdownFiles(string sourceFile, string destinationFile, string regionProjectPath)
  {
    var markdownContent = MarkdownHelper.AddMarkdownTableOfContents(sourceFile);

    var regions = FileHelper.EnumerateFiles(regionProjectPath, ".cs", ".cshtml")
      .SelectMany(x => CSharpHelper.EnumerateRegions(x))
      .Distinct(new RegionEqualityComparer())
      .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCulture);

    markdownContent = MarkdownHelper.ReplaceMarkdownRegions(markdownContent, regions);

    File.WriteAllText(destinationFile, markdownContent);
  }

  public static string ReplaceMarkdownRegions(string content, Dictionary<string, Region> regions)
  {
    var builder = new StringBuilder(content);

    foreach (Match match in markdownRegionRegex.Matches(content).Reverse())
    {
      var regionName = match.Groups["name"].Value;
      if (!regions.TryGetValue(regionName, out var region))
        throw new Exception($"Could not find region with name {regionName}");

      builder.Remove(match.Index, match.Length);
      builder.Insert(match.Index, Environment.NewLine + "```" + region.Language + Environment.NewLine + region.GetContent() + Environment.NewLine + "```" + Environment.NewLine);
    }

    return builder.ToString();
  }

  private static readonly Regex markdownRegionRegex = new Regex(@"^\s*<region:\s*(?<name>.+)\s*>\s*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromSeconds(5));
}

public static class ProjectHelper
{
  public static void UpdateCopyright()
  {
    const string filename = "./Directory.Build.props";

    var currentYear = DateTime.Now.ToString("yyyy");
    var copyrightRegex = new Regex(@$"<Copyright>.* (?<year>[0-9]+)</Copyright>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    var matchEvaluator = new MatchEvaluator(m =>
    {
      var group = m.Groups["year"];
      return m.Value[..(group.Index - group.Length)] + currentYear + m.Value[group.Index..];
    });

    var lines = File.ReadAllLines(filename);

    for (var i = 0; i < lines.Length; i++)
    {
      lines[i] = copyrightRegex.Replace(lines[i], matchEvaluator);
    }

    File.WriteAllLines(filename, lines);
  }
}

public static class GitHelper
{
  public static bool IsWorkingDirectoryClean()
  {
    var hasOutput = false;
    var command = "git";
    var arguments = "status --porcelain";
    ConsoleHelper.WriteLine($"> {command} {arguments}{Environment.NewLine}");

    using var cmd = new Process();
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
        ConsoleHelper.WriteLine(e.Data, isError: false, ConsoleHelper.GetColorForLine(e.Data) ?? ConsoleColor.DarkGray);
      }
    };
    cmd.ErrorDataReceived += (_, e) =>
    {
      if (e.Data != null)
      {
        hasOutput = true;
        ConsoleHelper.WriteLine(e.Data, isError: true, ConsoleColor.Red);
      }
    };

    cmd.Start();
    cmd.BeginOutputReadLine();
    cmd.BeginErrorReadLine();
    cmd.WaitForExit();

    return !hasOutput && cmd.ExitCode == 0;
  }
}

public static class FileHelper
{
  public static IEnumerable<string> EnumerateFiles(string path, params string[] extensions)
  {
    foreach (var directory in Directory.EnumerateDirectories(path))
    {
      var directoryName = new DirectoryInfo(directory).Name;
      if (string.Equals(directoryName, "bin", StringComparison.Ordinal) || string.Equals(directoryName, "obj", StringComparison.Ordinal) || directoryName.StartsWith(".", StringComparison.Ordinal))
        continue;

      foreach (var file in EnumerateFiles(directory, extensions))
        yield return file;
    }

    if (extensions.Length == 0)
    {
      foreach (var file in Directory.EnumerateFiles(path))
        yield return file;
    }
    else
    {
      foreach (var extension in extensions)
        foreach (var file in Directory.EnumerateFiles(path, $"*{extension}"))
          yield return file;
    }
  }

  public static string GetCallerFilePath([CallerFilePath] string path = "") => path;
}

public static class CSharpHelper
{
  public static IEnumerable<Region> EnumerateRegions(string path)
  {
    var regionLines = new List<string>();
    var isInRegion = false;
    var regionName = "";

    var (startRegionRegex, endRegionRegex, language) = new FileInfo(path).Extension switch
    {
      ".cs" => (startRegionCsRegex, endRegionCsRegex, "csharp"),
      ".cshtml" => (startRegionCshtmlRegex, endRegionCshtmlRegex, "cshtml"),
      _ => (null, null, "")
    };

    if (startRegionRegex is null || endRegionRegex is null)
      yield break;

    foreach (var line in File.ReadAllLines(path))
    {
      if (isInRegion)
      {
        if (endRegionRegex.IsMatch(line))
        {
          if (regionLines.Count > 0)
          {
            yield return new Region(regionName, language, regionLines.ToArray());
            regionLines.Clear();
          }

          isInRegion = false;
        }
        else
        {
          regionLines.Add(line);
        }
      }
      else
      {
        if (startRegionRegex.Match(line) is { Success: true } match)
        {
          regionName = match.Groups["name"].Value.Trim();
          isInRegion = true;
        }
      }
    }
  }

  private static readonly Regex startRegionCsRegex = new Regex(@"^\s*#region\s+(?<name>.+)\s*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(5));
  private static readonly Regex endRegionCsRegex = new Regex(@"^\s*#endregion\s*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(5));

  private static readonly Regex startRegionCshtmlRegex = new Regex(@"^\s*@{\s*#region\s+(?<name>.+)\s*}\s*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(5));
  private static readonly Regex endRegionCshtmlRegex = new Regex(@"^\s*@{\s*#endregion\s*}\s*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(5));
}

public sealed class Region
{
  public Region(string name, string language, IReadOnlyCollection<string> lines)
  {
    Name = name;
    Language = language;
    Lines = lines;
  }

  public string Name { get; }
  public string Language { get; }
  public IReadOnlyCollection<string> Lines { get; }

  public string GetContent() =>
    string.Join(Environment.NewLine, GetCleanedLines());
  public IEnumerable<string> GetCleanedLines()
  {
    var lastNonEmptyLineIndex = default(int?);
    var leadingWhitespace = default(int?);
    var result = new List<string>(Lines.Count);

    foreach (var line in Lines)
    {
      if (string.IsNullOrWhiteSpace(line))
      {
        if (lastNonEmptyLineIndex is not null)
        {
          result.Add("");
        }
      }
      else
      {
        result.Add(line.TrimEnd());
        lastNonEmptyLineIndex = result.Count - 1;

        var lineLeadingWhitespace = CountLeadingWhitespaceCharacters(line);

        if (leadingWhitespace is { } w)
          leadingWhitespace = Math.Min(w, lineLeadingWhitespace);
        else
          leadingWhitespace = lineLeadingWhitespace;
      }
    }

    if (lastNonEmptyLineIndex is { } lastNonEmptyLineIndexValue
      && result.Count - (lastNonEmptyLineIndexValue + 1) is var removeCount
      && removeCount > 0)
    {
      result.RemoveRange(lastNonEmptyLineIndexValue + 1, removeCount);
    }

    return TrimLeadingCharacters(result, leadingWhitespace ?? 0);
  }

  private static IEnumerable<string> TrimLeadingCharacters(IEnumerable<string> lines, int count)
  {
    if (count == 0)
      return lines;

    return lines.Select(line =>
    {
      if (line.Length <= count)
        return "";

      return line[count..];
    });
  }
  private static int CountLeadingWhitespaceCharacters(string line)
  {
    var count = 0;

    foreach (var character in line)
    {
      if (!char.IsWhiteSpace(character))
        break;

      count++;
    }

    return count;
  }
}

public sealed class RegionEqualityComparer : IEqualityComparer<Region>
{
  public bool Equals(Region? x, Region? y) => string.Equals(x?.Name, y?.Name, StringComparison.InvariantCulture);
  public int GetHashCode([DisallowNull] Region obj) => HashCode.Combine(obj.Name);
}
