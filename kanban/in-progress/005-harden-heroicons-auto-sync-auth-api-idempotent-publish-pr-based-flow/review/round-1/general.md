# Round 1 — general
**Date:** 2026-11-13
**Scope reviewed:** branch `task/005-harden-heroicons-auto-sync-auth-api-idempotent` vs `master`

## Summary

The sync command matches the task: authenticated lookup with a clean failure, `--pr` on `sync/heroicons-vX.Y.Z` with no duplicate when `gh pr list` finds an open PR, no NuGet publish from the sync job, release-tag check, `workflow_dispatch` → Merge, Node 24 action majors, and the legacy PowerShell scripts are gone. The restatement matches the original task, including auto-merge only when allowed and the tag check accepting the NuGet identity, which is the task's own version policy. The weekly job still cannot go red. `defaults.run.shell` is pwsh, and the sync step never returns the native exit code, so release-pending, lookup failures, and `gh pr create` failures stay green.

## Issues

### Issue 1 — Severity: bug
- File: .github/workflows/workflow.yml:81
- Description: The sync step runs `dotnet run tools/dev-cli/dev.cs -- update-icons --pr --base $env:BASE_BRANCH` under `defaults.run.shell: pwsh` (line 50) and does not `exit $LASTEXITCODE`. PowerShell leaves the process exit code at 0 when a native command fails, so GitHub Actions marks the step successful. The CI step in this same file (line 125) already exits with `$LASTEXITCODE` after the bash-to-pwsh conversion. The readme (line 92) and the task both require a non-zero `dev update-icons` to make the weekly run red (release pending, rate-limit or other lookup failure, failed `gh pr create`). Targeted check, because the step looked like it would swallow that status: `pwsh -NoProfile -Command 'bash -c "exit 3"; Write-Output "LAST=$LASTEXITCODE"'` printed `LAST=3` and the pwsh process exited 0.
- Suggestion: End the sync step with `exit $LASTEXITCODE`, the same way the CI step does.
- Status: open

### Issue 2 — Severity: suggestion
- File: glossary.md:12
- Description: `release pending` is defined as "`<Version>` already matches upstream" and the package is missing from NuGet. The command uses that same message whenever it is not syncing forward and the NuGet identity is missing, including a prerelease such as `2.2.0-tw.99` (upstream pin matches, `<Version>` does not) and a pin that is ahead of upstream. How to validate calls the prerelease case "release pending". The term is narrower in the glossary than in the command and the validation record.
- Suggestion: Define release pending as the props NuGet identity not being on nuget.org when there is nothing newer to sync (pin equal to upstream, including a TW-only prerelease, or pin ahead of upstream).
- Status: open

### Issue 3 — Severity: suggestion
- File: tools/dev-cli/endpoints/update-icons-command.cs:88
- Description: When the upstream pin is ahead of the latest heroicons release and that package identity is already on nuget.org, `ReportPublishStateAsync` still prints `Icons are already up to date`. The ahead line is printed first (line 87), then the up-to-date line (`update-icons-lookups.cs:107`). The restated acceptance criterion says a pin ahead of upstream is reported and is not called up to date. The equal-and-published path should keep the up-to-date line.
- Suggestion: On the ahead path, report that the pin leads upstream and whether the identity is on nuget.org, without the up-to-date sentence. Keep that sentence for pin-equal and published.
- Status: open
