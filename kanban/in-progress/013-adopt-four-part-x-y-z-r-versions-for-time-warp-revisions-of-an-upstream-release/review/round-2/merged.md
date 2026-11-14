# Round 2 — merged findings
**Date:** 2026-11-14
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: glossary.md:13
- Description: Round 1: `upstream pin` and `TW-only fix` still described the old strip-at-`-` / `-tw.N` rule, so `2.2.0.1-beta.1` would pin `2.2.0.1` and `2.0.12+build-1` would pin `build-1`, while `ExtractUpstream` pins both to `2.2.0` and `2.0.12`. The readme contributing paragraph stated the same old rule. Re-review of `b42cc49..a8fee15`: `glossary.md` now defines the pin as the first three numeric parts, with numeric `+X.Y.Z` metadata still naming the pin and non-numeric metadata ignored; `TW-only fix` is `X.Y.Z.R` / `X.Y.Z.R-beta.N` and names `-tw.N` as the old form; `release pending` lists `2.2.0.1` and `2.2.0.1-beta.1` beside `2.2.0-tw.1`. `readme.md` lines 68–71 point at the four-part scheme. The examples match `heroicons-version.cs` and `tests/dev-cli/heroicons-version.extract-upstream.cs`.
- Suggestion: Applied as specified in round 1.
- Source: general
- Disposition notes: Fixed in `a8fee15`. Verified on the docs delta. No new findings.

## Duplicates / conflicts

- Single reviewer. M1 carried from round 1. No new issues.
