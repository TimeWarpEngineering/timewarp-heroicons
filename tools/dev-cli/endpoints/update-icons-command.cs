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
// Handler is partial: lookups (update-icons-lookups.cs), regeneration
// (update-icons-regenerate.cs), and the PR step (update-icons-pull-request.cs).
#endregion

namespace DevCli.Commands;

[NuruRoute("update-icons", Description = "Sync icon components from upstream heroicons releases")]
internal sealed partial class UpdateIconsCommand : ICommand<Unit>
{
  [Option("pr", Description = "Commit the update on a sync/heroicons-vX.Y.Z branch, push it, and open a PR (needs gh + GH_TOKEN)")]
  public bool Pr { get; set; }

  [Option("auto-merge", Description = "With --pr: try to enable auto-merge (merge commit) on the opened PR")]
  public bool AutoMerge { get; set; }

  [Option("base", Description = "With --pr: base branch for the sync PR (default: master)")]
  public string? Base { get; set; }

  internal sealed partial class Handler : ICommandHandler<UpdateIconsCommand, Unit>
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

    public async Task<Unit> Handle(UpdateIconsCommand command, CancellationToken ct)
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
  }
}
