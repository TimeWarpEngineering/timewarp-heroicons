#region Purpose
// update-icons --pr: commit the sync on its own branch, push it, open (and optionally auto-merge) a PR
#endregion
#region Design
// Never pushes to the base branch. The sync branch is bot-owned, so a leftover branch from a
// closed PR is force-replaced. Bot git identity is set only inside GitHub Actions.
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
