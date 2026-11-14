# Round 1 — general
**Date:** 2026-11-14
**Scope reviewed:** branch `task/013-adopt-four-part-x-y-z-r-versions-for-time-warp` vs merge-base `35d882ca` (product files plus the task kitchen). Effort 2, roster general.

## Summary

`<Version>` is `2.2.0.1-beta.1`. `HeroiconsVersion.ExtractUpstream` returns the first three numeric parts, so `2.2.0`, `2.2.0.1`, and `2.2.0.1-beta.1` all pin `2.2.0`, and a numeric `+metadata` pin such as `2.0.19+2.0.18` still wins. `update-icons` compares that pin to the latest heroicons tag and, on a newer release, writes the three-part version. The Design region and `releases.md` describe `X.Y.Z.R`. Six Jaribu cases cover the required inputs and are invoked from `dev test`, the `dev workflow` test step (PR CI runs `dev workflow`), and the `update-icons` test step. Recorded gates cover build, test (runfiles 6/6), audit, and `check-version`. `release --dry-run` stops on the task branch at the master guard; a read of Nuru.DevCli 4.0.0-beta.13 `release-guard.cs` and `nuget-version-service.version-comparison.cs` matches the recorded claim (tag `v{Version}`, core compare walks every numeric part, `--prerelease` when the version contains `-`). The implementer restatement matches the original task. No misread.

`glossary.md` and the readme contributing paragraph still define the pin and a TW-only fix with the old `-tw.N` rule.

## Issues

### Issue 1 — Severity: bug
- File: glossary.md:13
- Description: `upstream pin` is defined as "`+metadata` when present, else the version without its prerelease suffix (`2.2.0-tw.1` pins `2.2.0`)". Applied to the version this task ships, that is the old strip-at-`-` rule: `2.2.0.1-beta.1` pins `2.2.0.1`, which `update-icons` treats as ahead of heroicons `2.2.0`. The code pins it to `2.2.0` (`heroicons-version.cs` `ExtractUpstream`, and the Design region at `update-icons-command.cs` lines 15–16). The same glossary sentence treats any `+metadata` as the pin, so `2.0.12+build-1` would pin `build-1`; the code and the new test ignore non-numeric metadata and return `2.0.12`. `TW-only fix` (glossary.md:15) is still "a prerelease on the pinned base (`2.2.0-tw.1`)". `readme.md:69` states that same rule as current policy. `releases.md` and the Design region say a TW-only fix is a fourth part `X.Y.Z.R` with prereleases `X.Y.Z.R-beta.N`, which replaces `-tw.N`. The repo glossary rule says a term used two ways is defined in `glossary.md` in the same PR. `release pending` (glossary.md:12) is still the right condition once the pin definition matches the code; its only example is the old `2.2.0-tw.1` form.
- Suggestion: Define `upstream pin` as the first three numeric parts, with a numeric `+metadata` version still naming the pin (`2.0.19+2.0.18` → `2.0.18`) and non-numeric metadata ignored. Define `TW-only fix` as `X.Y.Z.R` / `X.Y.Z.R-beta.N`, and note that `-tw.N` is the old form. Point the readme contributing sentence at that scheme. Mention a four-part example next to `2.2.0-tw.1` on `release pending`.
- Status: open
