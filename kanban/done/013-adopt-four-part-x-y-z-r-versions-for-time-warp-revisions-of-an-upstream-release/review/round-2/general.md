# Round 2 — general
**Date:** 2026-11-14
**Scope reviewed:** fix delta `b42cc49..a8fee15` (commit `a8fee15`). Product files: `glossary.md`, `readme.md`. Effort 1, roster general. Prior round frozen. Gates were not re-run; the fix-pass `## Results` record covers this docs delta.

## Summary

M1 is fixed. `glossary.md` defines `upstream pin` as the first three numeric parts, ignores a fourth part, a `-prerelease` label, and non-numeric `+metadata`, and still lets numeric `+X.Y.Z` metadata name the pin. Those sentences match `HeroiconsVersion.ExtractUpstream` and the six Jaribu cases: `2.2.0.1-beta.1`, `2.2.0.1`, and `2.2.0-tw.1` pin `2.2.0`; `2.0.12+build-1` pins `2.0.12`; `2.0.19+2.0.18` pins `2.0.18`. `TW-only fix` is `X.Y.Z.R` / `X.Y.Z.R-beta.N`, and `-tw.N` is the old form. `release pending` lists `2.2.0.1` and `2.2.0.1-beta.1` beside `2.2.0-tw.1`. The readme "Syncing icons and releasing" paragraph states the same four-part rule. The Design region and `releases.md` were already aligned and are outside this delta. The implementer restatement matches the original task. No misread. No new issues.

## Issues

None.
