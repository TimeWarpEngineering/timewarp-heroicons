# Round 1 — merged findings
**Date:** 2026-11-13
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: .github/workflows/workflow.yml:76
- Description: `defaults.run.shell` is `pwsh` (line 48), and Sync heroicons is a single native command with no `$LASTEXITCODE` check. PowerShell does not fail the script when a native executable returns non-zero, so a failed `dotnet run tools/dev-cli/dev.cs -- update-icons ...` leaves the weekly sync job green. Before this change the job used the runner's bash shell, which fails the step on that exit code. The Run CI Pipeline step (line 120) already has `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`. The probe `echo` step has no native command. Recorded `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` do not execute this workflow. Targeted check, because the sync step looked unguarded: `pwsh -NoProfile -Command 'dotnet --this-flag-does-not-exist 2>$null; Write-Output "after native LASTEXITCODE=$LASTEXITCODE"'` then `echo "pwsh-script-exit=$?"`. Result: `LASTEXITCODE=1` and `pwsh-script-exit=0`.
- Suggestion: After the `dotnet run` in Sync heroicons, exit with the native code the same way the CI step does: `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`.
- Source: general
- Disposition notes: Fixed in the implement-fix-2 pass. Sync heroicons now ends with `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`, matching the Run CI Pipeline step.

## Duplicates / conflicts

- Single reviewer. No overlap.
