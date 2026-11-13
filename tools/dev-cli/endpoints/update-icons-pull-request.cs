#region Purpose
// update-icons --pr: commit the sync on its own branch, push it, open (and optionally auto-merge) a PR
#endregion
#region Design
// Never pushes to the base branch. The sync branch is bot-owned, so a leftover branch from a
// closed PR is force-replaced. Bot git identity is set only inside GitHub Actions.
// A `gh pr create` permission failure (403 / "not permitted to create or approve pull requests")
// prints both owner fixes: a HEROICONS_SYNC_TOKEN secret or the repo Actions setting. Under
// Actions, sync-icons.yml sets HEROICONS_SYNC_TOKEN_SET ('true'/'false') because the job cannot
// tell which token GH_TOKEN holds; without the PAT a warning (not a failure) says the PR would
// trigger no CI, since events from GITHUB_TOKEN start no workflow runs.
#endregion

namespace DevCli.Commands;

internal sealed partial class UpdateIconsCommand
{
  internal sealed partial class Handler
  {
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
        string output = created.Combined.Trim();
        Terminal.WriteErrorLine($"Error: gh pr create failed: {output}".Red());
        if (IsPullRequestPermissionError(output))
        {
          Terminal.WriteErrorLine(
            $"The token in GH_TOKEN may not open pull requests. The pushed branch {branch} is kept. Fix one of:".Red());
          Terminal.WriteErrorLine(SyncTokenSetupHint);
          Terminal.WriteErrorLine(
            "  2. Settings -> Actions -> General -> Workflow permissions -> enable \"Allow GitHub Actions to create and approve pull requests\" (an org-level policy may also need it).");
          Terminal.WriteErrorLine("Then re-run the Sync icons workflow.");
        }

        Environment.ExitCode = 1;
        return null;
      }

      return created.Stdout.Trim();
    }

    private const string SyncTokenSetupHint =
      "  1. Add a HEROICONS_SYNC_TOKEN repository secret: a fine-grained PAT for this repository only, with " +
      "Contents: read and write and Pull requests: read and write (gh secret set HEROICONS_SYNC_TOKEN). " +
      "PRs it opens also trigger the normal CI run.";

    // gh reports a 403 as "HTTP 403" or GraphQL "not permitted to create or approve pull requests".
    private static bool IsPullRequestPermissionError(string output) =>
      output.Contains("not permitted to create or approve pull requests", StringComparison.OrdinalIgnoreCase)
      || output.Contains("Resource not accessible by integration", StringComparison.OrdinalIgnoreCase)
      || output.Contains("HTTP 403", StringComparison.OrdinalIgnoreCase)
      || output.Contains("403 Forbidden", StringComparison.OrdinalIgnoreCase);

    // Under Actions without the PAT the PR (if allowed at all) is opened by GITHUB_TOKEN and starts no CI.
    private void WarnWhenSyncTokenMissing()
    {
      if (!string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase)) return;
      if (string.Equals(Environment.GetEnvironmentVariable("HEROICONS_SYNC_TOKEN_SET"), "true", StringComparison.OrdinalIgnoreCase)) return;

      // Plain text: the ::warning:: workflow command must start the line, so no color codes.
      Terminal.WriteLine(
        "::warning title=HEROICONS_SYNC_TOKEN not set::No HEROICONS_SYNC_TOKEN secret, so the sync PR uses GITHUB_TOKEN. " +
        "That needs \"Allow GitHub Actions to create and approve pull requests\" enabled, and a PR opened with GITHUB_TOKEN triggers no CI run. " +
        "Add a fine-grained PAT for this repository (Contents + Pull requests: read and write) as the HEROICONS_SYNC_TOKEN secret.");
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
