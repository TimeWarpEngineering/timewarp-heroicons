# Round 1 — general
**Date:** 2026-11-13
**Scope reviewed:** branch `task/006-fix-githooks-runfiles-git-does-not-exist-cs0103` vs `master` (`4cafa1e`...`9df6c36`)

## Summary

The hook change fixes CS0103 the way the repo's pins allow: each `.githooks/*.cs` runfile references `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools`, calls `Git.FindRoot()` from `TimeWarp.Amuru`, and the extensionless names are symlinks to those runfiles. Package pins that need the .NET 11 SDK stay on the SDK 10 versions behind `package-pin-currency.hold`, with task 007 filed to lift them. Recorded build, test, and audit results cover that part of the diff.

The workflow change does not. `defaults.run.shell` is now `pwsh`, and the Sync heroicons step still treats a native `dotnet run` as success when that process exits non-zero. The Run CI Pipeline step already checks `$LASTEXITCODE`. The recorded validation does not run the workflow.

The Goal restatement drops the original "do not drop the memsearch call" sentence in favor of ganda's attest templates. That mismatch is a misread under `## Learnings` in `task.md`. It is not a finding: `memsearch-scaffold` rejects a hook that still contains memsearch, and the task also requires a clean audit.

## Issues

### Issue 1 — Severity: bug
- File: .github/workflows/workflow.yml:76
- Description: `defaults.run.shell` is `pwsh` (line 48), and Sync heroicons is a single native command with no `$LASTEXITCODE` check. PowerShell does not fail the script when a native executable returns non-zero, so a failed `dotnet run tools/dev-cli/dev.cs -- update-icons ...` leaves the weekly sync job green. Before this change the job used the runner's bash shell, which fails the step on that exit code. The Run CI Pipeline step (line 120) already has `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`. The probe `echo` step has no native command. Recorded `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` do not execute this workflow. Targeted check, because the sync step looked unguarded: `pwsh -NoProfile -Command 'dotnet --this-flag-does-not-exist 2>$null; Write-Output "after native LASTEXITCODE=$LASTEXITCODE"'` then `echo "pwsh-script-exit=$?"`. Result: `LASTEXITCODE=1` and `pwsh-script-exit=0`.
- Suggestion: After the `dotnet run` in Sync heroicons, exit with the native code the same way the CI step does: `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`.
- Status: open
