#region Purpose
// update-icons regeneration: clone upstream, transform SVGs to Razor, bump version, build + test
#endregion
#region Design
// Regenerates in the current working tree. Version and releases.md edits are idempotent so a
// re-run after a failed PR push does not stack duplicate release sections.
#endregion

namespace DevCli.Commands;

internal sealed partial class UpdateIconsCommand
{
  internal sealed partial class Handler
  {
    private async Task<bool> CloneHeroiconsAsync(string version, string cloneDir, CancellationToken ct)
    {
      Terminal.WriteLine($"\nCloning heroicons v{version}...");
      int exitCode = await Shell.Builder("git")
        .WithArguments("clone", "--depth", "1", "--branch", $"v{version}", HeroiconsRepo, cloneDir)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Failed to clone heroicons repository.".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> TransformIconsAsync(string repoRoot, string cloneDir, CancellationToken ct)
    {
      string inputPath = Path.Combine(cloneDir, "optimized");
      string outputPath = Path.Combine(repoRoot, "source", "timewarp-heroicons");
      string transformProject = Path.Combine(repoRoot, "tools", "transform", "transform.csproj");

      if (!Directory.Exists(inputPath))
      {
        Terminal.WriteErrorLine($"heroicons optimized directory not found: {inputPath}".Red());
        Environment.ExitCode = 1;
        return false;
      }

      // Drop prior generated size trees so removed upstream icons disappear and
      // namespace/layout changes (e.g. new 16px Micro) do not leave stale duplicates.
      foreach (string sizeDir in new[] { "16", "20", "24" })
      {
        string path = Path.Combine(outputPath, sizeDir);
        if (Directory.Exists(path))
        {
          Directory.Delete(path, recursive: true);
        }
      }

      Terminal.WriteLine("\nRegenerating Blazor icon components...");
      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("run", "--project", transformProject, "--", inputPath, outputPath)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Icon transform failed.".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private static void UpdateVersion(string propsPath, string newVersion)
    {
      XDocument doc = XDocument.Load(propsPath);
      XElement? versionElement = doc.Descendants("Version").FirstOrDefault()
        ?? throw new InvalidOperationException($"Could not find <Version> in {propsPath}");
      versionElement.Value = newVersion;
      doc.Save(propsPath);
    }

    private static void UpdateReleases(string repoRoot, string newVersion, string upstreamVersion)
    {
      string releasesPath = Path.Combine(repoRoot, "releases.md");
      string newSection = $"## {newVersion}\n\n* Update to heroicons version {upstreamVersion}\n\n";

      if (!File.Exists(releasesPath))
      {
        File.WriteAllText(releasesPath, $"# Releases\n\n{newSection}");
        return;
      }

      string existing = File.ReadAllText(releasesPath).Replace("\r\n", "\n", StringComparison.Ordinal);

      // Re-running a sync (e.g. after a failed PR push) must not stack duplicate sections.
      if (existing.Contains($"\n## {newVersion}\n", StringComparison.Ordinal)) return;

      const string heading = "# Releases\n";
      if (existing.StartsWith(heading, StringComparison.Ordinal))
      {
        string rest = existing[heading.Length..].TrimStart('\n');
        File.WriteAllText(releasesPath, $"{heading}\n{newSection}{rest}");
        return;
      }

      File.WriteAllText(releasesPath, $"# Releases\n\n{newSection}{existing}");
    }

    private async Task<bool> BuildAsync(string repoRoot, CancellationToken ct)
    {
      Terminal.WriteLine("\nVerifying build after icon update...");
      int exitCode = await DotNet.Build(Path.Combine(repoRoot, "timewarp-heroicons.slnx"))
        .WithConfiguration("Release")
        .WithWorkingDirectory(repoRoot)
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Build failed after icon update.".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    // PRs opened with the workflow GITHUB_TOKEN do not trigger other workflows, so the
    // sync run itself is the test gate for the PR it opens.
    private async Task<bool> TestAsync(string repoRoot, CancellationToken ct)
    {
      Terminal.WriteLine("\nRunning tests after icon update...");
      int exitCode = await DotNet.Test()
        .WithProject(Path.Combine(repoRoot, "timewarp-heroicons.slnx"))
        .WithConfiguration("Release")
        .WithWorkingDirectory(repoRoot)
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Tests failed after icon update.".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }
  }
}
