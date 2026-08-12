#region Purpose
// Syncs Blazor icon components from upstream tailwindlabs/heroicons releases
#endregion
#region Design
// Version policy (this package is NOT a normal independent TimeWarp line):
//   - When we sync to heroicons X.Y.Z, NuGet identity MUST be X.Y.Z (match source).
//   - TW-only fix without a new heroicons release: SemVer prerelease on that base,
//     e.g. 2.2.0-tw.1 (historical: 2.0.12-update.1). Never invent a disconnected
//     patch like 2.0.19 while still on heroicons 2.0.18.
//   - Optional +metadata may record the pin (2.2.0-tw.1+2.2.0) but + alone cannot
//     distinguish NuGet packages — only the identity before + ships to nuget.org.
//
// Flow: read props Version → extract upstream pin → compare to latest GitHub
// heroicons release → if stale, clone optimized SVGs, transform to Razor, set
// Version = upstream (clean match), update releases.md, build, optional push/publish.
#endregion

namespace DevCli.Commands;

[NuruRoute("update-icons", Description = "Sync icon components from upstream heroicons releases")]
internal sealed class UpdateIconsCommand : ICommand<Unit>
{
  [Option("push", Description = "Commit and push when icons were updated")]
  public bool Push { get; set; }

  [Option("publish", Description = "Pack and push to NuGet after updating (requires --api-key)")]
  public bool Publish { get; set; }

  [Option("api-key", Description = "NuGet API key from OIDC Trusted Publishing")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : ICommandHandler<UpdateIconsCommand, Unit>
  {
    private const string HeroiconsRepo = "https://github.com/tailwindlabs/heroicons.git";
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(UpdateIconsCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Value;
      }

      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      string currentVersion = ReadVersion(propsPath);
      string currentUpstream = ExtractUpstreamVersion(currentVersion);
      string latestUpstream = await FetchLatestHeroiconsVersionAsync(ct);

      Terminal.WriteLine($"Current heroicons version: {currentUpstream}");
      Terminal.WriteLine($"Latest heroicons release:  {latestUpstream}");

      if (string.Equals(currentUpstream, latestUpstream, StringComparison.Ordinal))
      {
        Terminal.WriteLine("Icons are already up to date.".Green());
        return Value;
      }

      string newVersion = BumpPackageVersion(currentVersion, latestUpstream);
      string cloneDir = Path.Combine(Path.GetTempPath(), $"heroicons-{latestUpstream}-{Guid.NewGuid():N}");

      try
      {
        if (!await CloneHeroiconsAsync(latestUpstream, cloneDir, ct)) return Value;
        if (!await TransformIconsAsync(repoRoot, cloneDir, ct)) return Value;

        UpdateVersion(propsPath, newVersion);
        UpdateReleases(repoRoot, newVersion, latestUpstream);

        if (!await BuildAsync(repoRoot, ct)) return Value;

        if (command.Push)
        {
          if (!await CommitAndPushAsync(repoRoot, latestUpstream, ct)) return Value;
        }

        if (command.Publish)
        {
          if (!await PackAndPushAsync(repoRoot, newVersion, command.ApiKey, ct)) return Value;
        }

        Terminal.WriteLine($"\nUpdated icons to heroicons {latestUpstream} (package {newVersion}).".Green());
      }
      finally
      {
        if (Directory.Exists(cloneDir))
        {
          Directory.Delete(cloneDir, recursive: true);
        }
      }

      return Value;
    }

    private static string ReadVersion(string propsPath)
    {
      XDocument doc = XDocument.Load(propsPath);
      string? version = doc.Descendants("Version").FirstOrDefault()?.Value;
      if (string.IsNullOrWhiteSpace(version))
      {
        throw new InvalidOperationException($"Could not read <Version> from {propsPath}");
      }

      return version;
    }

    // Upstream pin from Version: prefer +metadata; else strip prerelease (2.2.0-tw.1 → 2.2.0).
    private static string ExtractUpstreamVersion(string version)
    {
      int plus = version.IndexOf('+');
      if (plus >= 0)
      {
        return version[(plus + 1)..];
      }

      // TW-only re-ship: 2.2.0-tw.1 / 2.0.12-update.1 still track base X.Y.Z
      int dash = version.IndexOf('-');
      if (dash >= 0)
      {
        return version[..dash];
      }

      return version;
    }

    // On icon sync, NuGet version = heroicons version (no patch++ drift).
    // TW-only revisions are not produced here — hand-edit to e.g. 2.2.0-tw.1.
    private static string BumpPackageVersion(string currentVersion, string upstreamVersion)
    {
      _ = currentVersion;
      return upstreamVersion;
    }

    private static async Task<string> FetchLatestHeroiconsVersionAsync(CancellationToken ct)
    {
      using HttpClient client = new();
      client.DefaultRequestHeaders.UserAgent.ParseAdd("timewarp-heroicons-dev-cli");
      using HttpResponseMessage response = await client.GetAsync(
        "https://api.github.com/repos/tailwindlabs/heroicons/releases/latest",
        ct);
      response.EnsureSuccessStatusCode();

      await using Stream stream = await response.Content.ReadAsStreamAsync(ct);
      using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
      string tag = document.RootElement.GetProperty("tag_name").GetString()
        ?? throw new InvalidOperationException("heroicons latest release is missing tag_name");

      return tag.StartsWith('v') ? tag[1..] : tag;
    }

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

    private async Task<bool> CommitAndPushAsync(string repoRoot, string upstreamVersion, CancellationToken ct)
    {
      Terminal.WriteLine("\nCommitting and pushing changes...");

      await ConfigureGitBotIdentityAsync(repoRoot, ct);

      int exitCode = await Shell.Builder("git")
        .WithArguments("add", "source/", "releases.md", "source/Directory.Build.props")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);
      if (exitCode != 0) return Fail(exitCode);

      CommandOutput status = await Shell.Builder("git")
        .WithArguments("status", "--porcelain")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (string.IsNullOrWhiteSpace(status.Stdout))
      {
        Terminal.WriteLine("No changes to commit.");
        return true;
      }

      exitCode = await Shell.Builder("git")
        .WithArguments("commit", "-m", $"chore: update heroicons to {upstreamVersion}")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);
      if (exitCode != 0) return Fail(exitCode);

      exitCode = await Shell.Builder("git")
        .WithArguments("push", "origin", "HEAD")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);
      if (exitCode != 0) return Fail(exitCode);

      return true;
    }

    private static async Task ConfigureGitBotIdentityAsync(string repoRoot, CancellationToken ct)
    {
      await Shell.Builder("git").WithArguments("config", "user.name", "github-actions[bot]")
        .WithWorkingDirectory(repoRoot).WithNoValidation().RunAsync(ct);
      await Shell.Builder("git").WithArguments("config", "user.email", "41898282+github-actions[bot]@users.noreply.github.com")
        .WithWorkingDirectory(repoRoot).WithNoValidation().RunAsync(ct);
    }

    private async Task<bool> PackAndPushAsync(string repoRoot, string version, string? apiKey, CancellationToken ct)
    {
      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      Directory.CreateDirectory(artifactsDir);
      string projectPath = Path.Combine(repoRoot, "source", "timewarp-heroicons", "timewarp-heroicons.csproj");

      Terminal.WriteLine("\nPacking NuGet package...");
      int exitCode = await DotNet.Pack(projectPath)
        .WithConfiguration("Release")
        .WithOutput(artifactsDir)
        .WithWorkingDirectory(repoRoot)
        .RunAsync(ct);
      if (exitCode != 0) return Fail(exitCode);

      // Pack uses NuGet identity (metadata after + is not in the file name).
      string nugetIdentity = version.Split('+')[0];
      string packagePath = Path.Combine(artifactsDir, $"timewarp-heroicons.{nugetIdentity}.nupkg");
      if (!File.Exists(packagePath))
      {
        Terminal.WriteErrorLine($"Package not found: {packagePath}".Red());
        Environment.ExitCode = 1;
        return false;
      }

      if (string.IsNullOrEmpty(apiKey))
      {
        Terminal.WriteLine("Skipping NuGet push (no API key provided).");
        return true;
      }

      Terminal.WriteLine("Pushing package to NuGet.org...");
      exitCode = await DotNet.NuGet()
        .Push(packagePath)
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithApiKey(apiKey)
        .WithSkipDuplicate()
        .RunAsync(ct);
      if (exitCode != 0) return Fail(exitCode);

      Terminal.WriteLine("Package pushed to NuGet.org.".Green());
      return true;
    }

    private bool Fail(int exitCode)
    {
      Environment.ExitCode = exitCode;
      return false;
    }
  }
}