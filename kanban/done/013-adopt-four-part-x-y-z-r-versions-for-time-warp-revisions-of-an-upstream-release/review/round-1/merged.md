# Round 1 — merged findings
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
- Description: `upstream pin` is defined as "`+metadata` when present, else the version without its prerelease suffix (`2.2.0-tw.1` pins `2.2.0`)". Applied to the version this task ships, that is the old strip-at-`-` rule: `2.2.0.1-beta.1` pins `2.2.0.1`, which `update-icons` treats as ahead of heroicons `2.2.0`. The code pins it to `2.2.0` (`heroicons-version.cs` `ExtractUpstream`, and the Design region at `update-icons-command.cs` lines 15–16). The same glossary sentence treats any `+metadata` as the pin, so `2.0.12+build-1` would pin `build-1`; the code and the new test ignore non-numeric metadata and return `2.0.12`. `TW-only fix` (glossary.md:15) is still "a prerelease on the pinned base (`2.2.0-tw.1`)". `readme.md:69` states that same rule as current policy. `releases.md` and the Design region say a TW-only fix is a fourth part `X.Y.Z.R` with prereleases `X.Y.Z.R-beta.N`, which replaces `-tw.N`. The repo glossary rule says a term used two ways is defined in `glossary.md` in the same PR. `release pending` (glossary.md:12) is still the right condition once the pin definition matches the code; its only example is the old `2.2.0-tw.1` form.
- Suggestion: Define `upstream pin` as the first three numeric parts, with a numeric `+metadata` version still naming the pin (`2.0.19+2.0.18` → `2.0.18`) and non-numeric metadata ignored. Define `TW-only fix` as `X.Y.Z.R` / `X.Y.Z.R-beta.N`, and note that `-tw.N` is the old form. Point the readme contributing sentence at that scheme. Mention a four-part example next to `2.2.0-tw.1` on `release pending`.
- Source: general
- Disposition notes: fixed in the round-2 fix pass (2026-11-14). `glossary.md` `upstream pin` is now the first three numeric parts, with numeric `+X.Y.Z` metadata still naming the pin and non-numeric metadata ignored; `TW-only fix` is `X.Y.Z.R` / `X.Y.Z.R-beta.N` and names `-tw.N` as the old form; `release pending` lists `2.2.0.1` and `2.2.0.1-beta.1` beside `2.2.0-tw.1`. `readme.md` contributing paragraph points at the four-part scheme.

## Duplicates / conflicts

- Single reviewer. No overlaps.
