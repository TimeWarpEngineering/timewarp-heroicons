# Round 1 — merged findings
**Date:** 2026-11-13
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: .github/workflows/workflow.yml:81
- Description: The sync step runs under pwsh (`defaults.run.shell`, line 50) and does not `exit $LASTEXITCODE`. A failing `dev update-icons` still leaves the step green, so the weekly run does not go red for release pending, lookup failure, or a failed `gh pr create`. The CI step at line 125 already propagates `$LASTEXITCODE`. Targeted check: `pwsh -NoProfile -Command 'bash -c "exit 3"; Write-Output "LAST=$LASTEXITCODE"'` printed `LAST=3` and pwsh exited 0.
- Suggestion: End the sync step with `exit $LASTEXITCODE`.
- Source: general
- Disposition notes: Fixed in implement-fix-2: sync step ends with `exit $LASTEXITCODE` (.github/workflows/workflow.yml).

### M2 — Severity: suggestion — Status: fixed
- File: glossary.md:12
- Description: `release pending` is defined as `<Version>` matching upstream and missing from NuGet. The command and How to validate also use it when the identity is a TW-only prerelease (`2.2.0-tw.99`) or the pin is ahead of upstream and the identity is not on nuget.org.
- Suggestion: Define release pending as the props NuGet identity missing from nuget.org when there is nothing newer to sync (pin equal, including a TW-only prerelease, or pin ahead).
- Source: general
- Disposition notes: Fixed in implement-fix-2: glossary defines release pending as the props NuGet identity missing from nuget.org when nothing newer upstream (pin equal, incl. TW-only prerelease, or ahead).

### M3 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/endpoints/update-icons-command.cs:88
- Description: A pin ahead of upstream that is already on nuget.org is reported and then called `Icons are already up to date` (`update-icons-lookups.cs:107`). The restated acceptance criterion says the ahead case is not "up to date".
- Suggestion: On the ahead path, report the lead and the NuGet presence without the up-to-date sentence. Keep that sentence when the pin equals upstream and the package is published.
- Source: general
- Disposition notes: Fixed in implement-fix-2: `ReportPublishStateAsync` takes `aheadOfUpstream`; the ahead + published case prints `<id> <ver> is on nuget.org; the props pin still leads upstream heroicons.` (yellow), no up-to-date sentence. Smoke with `<Version>2.2.0+3.0.0</Version>` confirmed.

## Duplicates / conflicts

- Single reviewer. No overlaps.
