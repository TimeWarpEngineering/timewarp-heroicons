# Round 2 — merged findings
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
- File: .github/workflows/workflow.yml:78
- Description: Round 1: `defaults.run.shell` is `pwsh`, and Sync heroicons was a single native `dotnet run` with no `$LASTEXITCODE` check, so a failed icon sync left the weekly job green. Re-review of `9df6c36`..`78bf3e6`: the step now ends with `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`, immediately after that `dotnet run`, matching Run CI Pipeline.
- Suggestion: Applied as specified in round 1.
- Source: general
- Disposition notes: Fixed in `78bf3e6`. No new findings on the delta.

## Duplicates / conflicts

- Single reviewer. M1 carried from round 1. No new issues.
