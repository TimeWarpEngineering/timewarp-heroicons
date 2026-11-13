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
// heroicons release (authenticated with GITHUB_TOKEN / GH_TOKEN when present).
//   - Upstream newer: with --pr, skip if an open sync PR for sync/heroicons-vX.Y.Z
//     already exists; otherwise switch to that branch, clone optimized SVGs,
//     transform to Razor, set Version = upstream, update releases.md, build + test,
//     commit, push the branch, and open a PR against the base branch (optional
//     --auto-merge). Without --pr the regenerated files stay in the working tree.
//   - Upstream equal: check nuget.org. Published → "up to date". Not published →
//     report "release pending" and exit non-zero so the scheduled run is visibly red
//     until someone cuts the GitHub release (the only publish path).
//   - Props ahead of upstream: report it, do not call it "up to date".
// This command never pushes to the base branch and never publishes to NuGet.
// Publishing happens only via a GitHub release → `release` event → `dev workflow`.
#endregion

using System.Net;
using System.Net.Http.Headers;

namespace DevCli.Commands;

[NuruRoute("update-icons", Description = "Sync icon components from upstream heroicons releases")]
internal sealed class UpdateIconsCommand : ICommand<Unit>
{
  [Option("pr", Description = "Commit the update on a sync/heroicons-vX.Y.Z branch, push it, and open a PR (needs gh + GH_TOKEN)")]
  public bool Pr { get; set; }

  [Option("auto-merge", Description = "With --pr: try to enable auto-merge (merge commit) on the opened PR")]
  public bool AutoMerge { get; set; }

  [Option("base", Description = "With --pr: base branch for the sync PR (default: master)")]
  public string? Base { get; set; }

  internal sealed class Handler : ICommandHandler<UpdateIconsCommand, Unit>
  {
    private const string HeroiconsRepo = "https://github.com/tailwindlabs/heroicons.git";
    private const string LatestReleaseUrl = "https://api.github.com/repos/tailwindlabs/heroicons/releases/latest";
    private const string NuGetVersionsUrl = "https://api.nuget.org/v3-flatcontainer/timewarp-heroicons/index.json";
    private const string PackageId = "timewarp-heroicons";
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
      string? latestUpstream = await FetchLatestHeroiconsVersionAsync(ct);
      if (latestUpstream is null) return Value;

      Terminal.WriteLine($"Package version (props):   {currentVersion}");
      Terminal.WriteLine($"Current heroicons version: {currentUpstream}");
      Terminal.WriteLine($"Latest heroicons release:  {latestUpstream}");

      int comparison = CompareVersions(currentUpstream, latestUpstream);
      if (comparison == 0)
      {
        await ReportPublishStateAsync(currentVersion, ct);
        return Value;
      }

      if (comparison > 0)
      {
        Terminal.WriteLine(
          $"Props pin {currentUpstream} is ahead of the latest heroicons release {latestUpstream}. Nothing to sync; check source/Directory.Build.props.".Yellow());
        await ReportPublishStateAsync(currentVersion, ct);
        return Value;
      }

      string newVersion = BumpPackageVersion(currentVersion, latestUpstream);
      string branch = $"sync/heroicons-v{latestUpstream}";
      string baseBranch = string.IsNullOrWhiteSpace(command.Base) ? "master" : command.Base;

      if (command.Pr)
      {
        string? openPr = await FindOpenPullRequestAsync(repoRoot, branch, ct);
        if (openPr is null) return Value; // lookup failed; error already reported
        if (openPr.Length > 0)
        {
          Terminal.WriteLine($"Sync PR for heroicons {latestUpstream} is already open: {openPr}".Yellow());
          Terminal.WriteLine("Merge it, then publish a GitHub release v" + newVersion + " to ship the package.");
          return Value;
        }

        if (!await RunGitAsync(repoRoot, ct, "checkout", "-B", branch)) return Value;
      }

      string cloneDir = Path.Combine(Path.GetTempPath(), $"heroicons-{latestUpstream}-{Guid.NewGuid():N}");

      try
      {
        if (!await CloneHeroiconsAsync(latestUpstream, cloneDir, ct)) return Value;
        if (!await TransformIconsAsync(repoRoot, cloneDir, ct)) return Value;

        UpdateVersion(propsPath, newVersion);
        UpdateReleases(repoRoot, newVersion, latestUpstream);

        if (!await BuildAsync(repoRoot, ct)) return Value;
        if (!await TestAsync(repoRoot, ct)) return Value;

        if (!command.Pr)
        {
          Terminal.WriteLine($"\nRegenerated icons for heroicons {latestUpstream} (package {newVersion}) in the working tree.".Green());
          Terminal.WriteLine("Nothing was committed. Run with --pr to open a sync PR.");
          return Value;
        }

        string? prUrl = await CommitPushAndOpenPrAsync(repoRoot, branch, baseBranch, latestUpstream, newVersion, ct);
        if (prUrl is null) return Value;

        if (command.AutoMerge)
        {
          await TryEnableAutoMergeAsync(repoRoot, branch, ct);
        }

        Terminal.WriteLine($"\nOpened sync PR for heroicons {latestUpstream} (package {newVersion}): {prUrl}".Green());
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

    // NuGet identity is the version without +metadata (that is what nuget.org stores).
    private static string NuGetIdentity(string version) => version.Split('+')[0];

    // Numeric X.Y.Z compare; falls back to ordinal when either side is not numeric.
    private static int CompareVersions(string left, string right)
    {
      if (Version.TryParse(left, out Version? l) && Version.TryParse(right, out Version? r))
      {
        return l.CompareTo(r);
      }

      return string.CompareOrdinal(left, right);
    }

    // On icon sync, NuGet version = heroicons version (no patch++ drift).
    // TW-only revisions are not produced here — hand-edit to e.g. 2.2.0-tw.1.
    private static string BumpPackageVersion(string currentVersion, string upstreamVersion)
    {
      _ = currentVersion;
      return upstreamVersion;
    }

    private static HttpClient CreateHttpClient()
    {
      HttpClient client = new() { Timeout = TimeSpan.FromSeconds(60) };
      client.DefaultRequestHeaders.UserAgent.ParseAdd("timewarp-heroicons-dev-cli");
      return client;
    }

    private static string? GitHubToken()
    {
      string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
      if (string.IsNullOrWhiteSpace(token))
      {
        token = Environment.GetEnvironmentVariable("GH_TOKEN");
      }

      return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    private async Task<string?> FetchLatestHeroiconsVersionAsync(CancellationToken ct)
    {
      string? token = GitHubToken();
      using HttpClient client = CreateHttpClient();
      using HttpRequestMessage request = new(HttpMethod.Get, LatestReleaseUrl);
      request.Headers.Accept.ParseAdd("application/vnd.github+json");
      if (token is not null)
      {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      }

      try
      {
        using HttpResponseMessage response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
          string detail = $"{(int)response.StatusCode} {response.ReasonPhrase}";
          if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests
            && response.Headers.TryGetValues("x-ratelimit-remaining", out IEnumerable<string>? remaining)
            && remaining.FirstOrDefault() == "0")
          {
            detail += " (GitHub API rate limit exceeded)";
          }

          string hint = token is null
            ? " Set GITHUB_TOKEN (or GH_TOKEN) to authenticate."
            : "";
          return FailLookup($"GitHub API returned {detail} for {LatestReleaseUrl}.{hint}");
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        if (!document.RootElement.TryGetProperty("tag_name", out JsonElement tagElement)
          || tagElement.GetString() is not { Length: > 0 } tag)
        {
          return FailLookup("heroicons latest release response has no tag_name.");
        }

        return tag.StartsWith('v') ? tag[1..] : tag;
      }
      catch (HttpRequestException ex)
      {
        return FailLookup($"could not reach the GitHub API: {ex.Message}");
      }
      catch (TaskCanceledException) when (!ct.IsCancellationRequested)
      {
        return FailLookup("timed out calling the GitHub API.");
      }
      catch (JsonException ex)
      {
        return FailLookup($"GitHub API returned invalid JSON: {ex.Message}");
      }
    }

    private string? FailLookup(string message)
    {
      Terminal.WriteErrorLine($"Error: upstream heroicons release lookup failed: {message}".Red());
      Environment.ExitCode = 1;
      return null;
    }

    // Props already match (or lead) upstream. Only "up to date" when nuget.org has the identity.
    private async Task ReportPublishStateAsync(string currentVersion, CancellationToken ct)
    {
      string identity = NuGetIdentity(currentVersion);
      bool? published = await IsPublishedOnNuGetAsync(identity, ct);
      if (published is null) return; // error already reported

      if (published.Value)
      {
        Terminal.WriteLine($"Icons are already up to date ({PackageId} {identity} is on nuget.org).".Green());
        return;
      }

      Terminal.WriteErrorLine(
        $"Release pending: props say {currentVersion} but {PackageId} {identity} is not on nuget.org.".Red());
      Terminal.WriteErrorLine(
        $"Publish it through the normal path: merge any open sync PR, then create a GitHub release with tag v{identity} on the base branch.");
      Environment.ExitCode = 1;
    }

    private async Task<bool?> IsPublishedOnNuGetAsync(string identity, CancellationToken ct)
    {
      using HttpClient client = CreateHttpClient();
      try
      {
        using HttpResponseMessage response = await client.GetAsync(NuGetVersionsUrl, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        if (!response.IsSuccessStatusCode)
        {
          return FailNuGet($"{(int)response.StatusCode} {response.ReasonPhrase}");
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        if (!document.RootElement.TryGetProperty("versions", out JsonElement versions)
          || versions.ValueKind != JsonValueKind.Array)
        {
          return FailNuGet("response has no versions array");
        }

        return versions.EnumerateArray()
          .Select(v => v.GetString())
          .Any(v => string.Equals(v, identity, StringComparison.OrdinalIgnoreCase));
      }
      catch (HttpRequestException ex)
      {
        return FailNuGet(ex.Message);
      }
      catch (TaskCanceledException) when (!ct.IsCancellationRequested)
      {
        return FailNuGet("timed out");
      }
      catch (JsonException ex)
      {
        return FailNuGet($"invalid JSON: {ex.Message}");
      }
    }

    private bool? FailNuGet(string message)
    {
      Terminal.WriteErrorLine($"Error: could not check nuget.org for published versions: {message}".Red());
      Environment.ExitCode = 1;
      return null;
    }

    // Returns the open PR URL, "" when none is open, or null when gh failed.
    private async Task<string?> FindOpenPullRequestAsync(string repoRoot, string branch, CancellationToken ct)
    {
      CommandOutput output = await Shell.Builder("gh")
        .WithArguments("pr", "list", "--head", branch, "--state", "open", "--json", "url", "--jq", ".[0].url // \"\"")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);

      if (!output.Success)
      {
        Terminal.WriteErrorLine($"Error: gh pr list failed (is gh installed and GH_TOKEN set?): {output.Combined.Trim()}".Red());
        Environment.ExitCode = 1;
        return null;
      }

      return output.Stdout.Trim();
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

    private async Task<string?> CommitPushAndOpenPrAsync(
      string repoRoot,
      string branch,
      string baseBranch,
      string upstreamVersion,
      string newVersion,
      CancellationToken ct)
    {
      Terminal.WriteLine($"\nCommitting on {branch} and opening a PR into {baseBranch}...");

      await ConfigureGitBotIdentityAsync(repoRoot, ct);

      if (!await RunGitAsync(repoRoot, ct, "add", "source/", "releases.md")) return null;

      CommandOutput status = await Shell.Builder("git")
        .WithArguments("status", "--porcelain")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (string.IsNullOrWhiteSpace(status.Stdout))
      {
        Terminal.WriteErrorLine("Error: upstream is newer but regeneration produced no changes; not opening a PR.".Red());
        Environment.ExitCode = 1;
        return null;
      }

      string title = $"chore: update heroicons to {upstreamVersion}";
      if (!await RunGitAsync(repoRoot, ct, "commit", "-m", title)) return null;

      // The sync branch is bot-owned; a leftover branch from a closed PR is replaced.
      if (!await RunGitAsync(repoRoot, ct, "push", "--force", "origin", $"HEAD:refs/heads/{branch}")) return null;

      string body =
        $"Automated sync to [heroicons v{upstreamVersion}](https://github.com/tailwindlabs/heroicons/releases/tag/v{upstreamVersion}).\n\n" +
        $"- Regenerated icon components from `optimized/`\n" +
        $"- `source/Directory.Build.props` `<Version>` → `{newVersion}`\n" +
        $"- `releases.md` section added\n\n" +
        "Build and tests ran in the sync job before this PR was opened.\n\n" +
        $"After merge, publish by creating a GitHub release with tag `v{NuGetIdentity(newVersion)}` on `{baseBranch}` " +
        "(the `release` event runs `dev workflow`, which packs and pushes to nuget.org).";

      CommandOutput created = await Shell.Builder("gh")
        .WithArguments("pr", "create", "--base", baseBranch, "--head", branch, "--title", title, "--body", body)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (!created.Success)
      {
        Terminal.WriteErrorLine($"Error: gh pr create failed: {created.Combined.Trim()}".Red());
        Environment.ExitCode = 1;
        return null;
      }

      return created.Stdout.Trim();
    }

    private async Task TryEnableAutoMergeAsync(string repoRoot, string branch, CancellationToken ct)
    {
      CommandOutput merged = await Shell.Builder("gh")
        .WithArguments("pr", "merge", branch, "--auto", "--merge")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);

      if (merged.Success)
      {
        Terminal.WriteLine("Auto-merge enabled on the sync PR.");
        return;
      }

      Terminal.WriteLine($"Warning: could not enable auto-merge (PR stays open for review): {merged.Combined.Trim()}".Yellow());
    }

    private async Task<bool> RunGitAsync(string repoRoot, CancellationToken ct, params string[] arguments)
    {
      int exitCode = await Shell.Builder("git")
        .WithArguments(arguments)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);
      if (exitCode == 0) return true;

      Terminal.WriteErrorLine($"Error: git {string.Join(' ', arguments)} failed (exit {exitCode}).".Red());
      Environment.ExitCode = exitCode;
      return false;
    }

    // Only in GitHub Actions; a local --pr run keeps the developer's own git identity.
    private static async Task ConfigureGitBotIdentityAsync(string repoRoot, CancellationToken ct)
    {
      if (!string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase)) return;

      await Shell.Builder("git").WithArguments("config", "user.name", "github-actions[bot]")
        .WithWorkingDirectory(repoRoot).WithNoValidation().RunAsync(ct);
      await Shell.Builder("git").WithArguments("config", "user.email", "41898282+github-actions[bot]@users.noreply.github.com")
        .WithWorkingDirectory(repoRoot).WithNoValidation().RunAsync(ct);
    }
  }
}
