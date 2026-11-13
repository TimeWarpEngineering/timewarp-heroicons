# Round 2 — general
**Date:** 2026-11-13
**Scope reviewed:** product delta `fde535bbbba19dd611d902eeba6496ea9de4aaac..35ebe3088a537c109e23dd280f2f98df09285665` (commit `35ebe30`)

## Summary

The fix commit does the three round-1 items and nothing else in product code. The sync step now ends with `exit $LASTEXITCODE` under the workflow's pwsh default, the glossary definition of release pending matches the unpublished pin-equal and pin-ahead paths, and a pin ahead of upstream that is already on nuget.org is no longer called up to date. The implementer restatement still matches the original task. No new defects on this delta. Recorded gates in `## Results` cover the fix (build, test, audit) and `### How to validate` names the new messages.

## Prior findings

### M1 — Severity: bug — Status: fixed
- File: .github/workflows/workflow.yml:82
- Description: The sync step (`defaults.run.shell: pwsh`, line 50) runs `dotnet run … update-icons --pr` and then `exit $LASTEXITCODE`. A failing native command leaves `$LASTEXITCODE` set, and the step returns that code. The CI step at line 126 uses the same exit. Release pending, lookup failure, and a failed `gh pr create` set `Environment.ExitCode = 1` before the process returns, so `dotnet run` fails and the step goes red.
- Suggestion: none remaining.

### M2 — Severity: suggestion — Status: fixed
- File: glossary.md:12
- Description: `release pending` is the props NuGet identity missing from nuget.org when nothing newer is upstream: pin equal to the latest heroicons release (a TW-only prerelease such as `2.2.0-tw.1` pins `2.2.0` via `ExtractUpstreamVersion`), or the pin ahead of upstream. That matches `ReportPublishStateAsync`, which prints `Release pending` and sets exit code 1 for both the equal and ahead unpublished paths.
- Suggestion: none remaining.

### M3 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/endpoints/update-icons-lookups.cs:108
- Description: `ReportPublishStateAsync` takes `aheadOfUpstream`. The command passes `false` when the pin equals upstream (`update-icons-command.cs:80`) and `true` when it leads (`update-icons-command.cs:88`). The published ahead branch prints `timewarp-heroicons <identity> is on nuget.org; the props pin still leads upstream heroicons.` and does not print `Icons are already up to date`. The equal-and-published branch still prints that sentence. `NuGetIdentity` strips `+metadata`, so `<Version>2.2.0+3.0.0</Version>` reports identity `2.2.0` with pin `3.0.0`, which is what `### How to validate` expects. The published ahead path does not set a non-zero exit code.
- Suggestion: none remaining.

## Issues

No new issues.
