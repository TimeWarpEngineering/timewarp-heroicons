# Round 2 — merged findings
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
- File: .github/workflows/workflow.yml:82
- Description: The sync step runs under pwsh and now ends with `exit $LASTEXITCODE`, so a failing `update-icons --pr` fails the step.
- Suggestion: none remaining.
- Source: general
- Disposition notes: Re-verified on `35ebe30`. Sync step line 82 is `exit $LASTEXITCODE` after the single `dotnet run` invocation. Same pattern as the CI step at line 126.

### M2 — Severity: suggestion — Status: fixed
- File: glossary.md:12
- Description: `release pending` means the props NuGet identity is missing from nuget.org when nothing newer is upstream (pin equal, including a TW-only prerelease, or pin ahead). The command exits non-zero in that case.
- Suggestion: none remaining.
- Source: general
- Disposition notes: Re-verified on `35ebe30`. Glossary line 12 matches `ReportPublishStateAsync` for both unpublished paths.

### M3 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/endpoints/update-icons-lookups.cs:108
- Description: A published pin ahead of upstream is reported as on nuget.org with the pin still leading. It is not called up to date. Pin-equal and published still uses the up-to-date sentence.
- Suggestion: none remaining.
- Source: general
- Disposition notes: Re-verified on `35ebe30`. `aheadOfUpstream` is passed from both call sites. The ahead published message has no "up to date" sentence.

## Duplicates / conflicts

- Single reviewer. Prior IDs M1–M3 carried forward. No new findings.
