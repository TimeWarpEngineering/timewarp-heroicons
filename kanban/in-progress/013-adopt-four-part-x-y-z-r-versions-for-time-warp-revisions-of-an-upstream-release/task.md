# Adopt four-part X.Y.Z.R versions for TimeWarp revisions of an upstream release

## Description

The package version tracks the upstream heroicons release. A TimeWarp-only change
(packaging, dependency pins) needs its own release that keeps the upstream number
and sorts after the release already on the feed.

The current scheme, `2.2.0-tw.1`, does not do that. A prerelease label sorts
before the plain version, so `2.2.0-tw.1` is older than the published `2.2.0`.
`ganda task wave` also parks on it: "Version plan: <Version> '2.2.0-tw.1' is a
prerelease that is not -beta.N; the wave has no rule for it." Build metadata
(`2.2.0+update.1`) is not an option either: NuGet ignores it, so nuget.org treats
it as the published `2.2.0`.

Use a fourth version part for TimeWarp revisions. NuGet orders:
`2.2.0 < 2.2.0.1-beta.1 < 2.2.0.1 < 2.2.0.2 < 2.2.1`. The first three parts
always equal the upstream heroicons version. When upstream ships a new version,
the package goes to that three-part version and the revision starts again.

## Requirements

- Set `<Version>` in `source/Directory.Build.props` from `2.2.0-tw.1` to
  `2.2.0.1-beta.1`.
- `tools/dev-cli/endpoints/update-icons-command.cs` `ExtractUpstreamVersion`
  returns the first three numeric parts. Today it strips at the first `-` and
  yields `2.2.0.1`, which compares as ahead of upstream `2.2.0` and prints a
  misleading "Props pin 2.2.0.1 is ahead of the latest heroicons release".
  A four-part version on the same upstream release is "equal to upstream".
- When `update-icons` moves to a new upstream release, it writes the three-part
  upstream version (no fourth part).
- Update the policy text in the `update-icons-command.cs` Design region and
  `releases.md`: `X.Y.Z.R` revisions replace `-tw.N`.
- Tests for `ExtractUpstreamVersion` with three-part, four-part, and four-part
  prerelease versions.
- Confirm `./bin/dev check-version` and a `dev release --dry-run` accept
  `2.2.0.1-beta.1`. A code read on 2026-11-14 says they do (string tag compare,
  no SemVer regex), but it was not run.

## Goal (implementer restatement)

The package's first three version parts must always equal the upstream heroicons
release. TimeWarp-only re-ships add a fourth part (`X.Y.Z.R`), which NuGet sorts after
`X.Y.Z` and before `X.Y.(Z+1)`; prereleases of a revision are `X.Y.Z.R-beta.N`.

Acceptance:

- `source/Directory.Build.props` `<Version>` is `2.2.0.1-beta.1`.
- `update-icons` reads the upstream pin as the first three numeric parts, so
  `2.2.0`, `2.2.0.1`, and `2.2.0.1-beta.1` all mean heroicons `2.2.0` (equal to
  upstream, not "ahead").
- On a new upstream release, `update-icons` writes the plain three-part version.
- The Design region of `update-icons-command.cs` and `releases.md` describe `X.Y.Z.R`
  revisions instead of `-tw.N`.
- Tests cover three-part, four-part, and four-part prerelease inputs, and they run
  under `./bin/dev test` (and CI's `dev workflow` test step).
- `./bin/dev check-version` and `./bin/dev release --dry-run` accept `2.2.0.1-beta.1`.

## Checklist

- [x] `<Version>` is `2.2.0.1-beta.1`
- [x] `ExtractUpstreamVersion` returns three parts, with tests
- [x] Policy text updated in the Design region and `releases.md`
- [x] `dev check-version` and `dev release --dry-run` pass on the new version
  (`check-version` run green; `release --dry-run` run up to its master-branch guard,
  version guards confirmed by code read — see Results)

## Session

- Created: e92789a1-bc49-442d-98c6-87a856ac7395 (2026-11-14)

- 2026-11-14 implement oracle (Claude Opus 5.5): implemented, gates green, pushed.
  Boy-scout: the post-commit attest failed `package-pin-currency` (4 dev-only pins
  behind nuget.org); `ganda repo audit --fix` bumped TimeWarp.Build.Tasks 2.0.1-beta.1,
  TimeWarp.Nuru / Nuru.DevCli 4.0.0-beta.13, TimeWarp.SourceGenerators 1.0.0-beta.16.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 134f3dc8-8da8-4400-9d28-e8984634031a, max-turns 500, Done — 2026-11-14T09:23:07Z
- Review (2026-11-14): general, effort 2, round 1, M1 open. Restatement matches the task; no misread. Bound turn cap 120; this session recorded the cap as uncapped (no --max-turns flag).
- Review oracle: reviewed by reviewer-grok (grok, vendor xai, model grok-4.7), body tw-implementation-review, session 01a1d95e-3bd5-79a3-a24e-4a43849c3058, max-turns uncapped (review budget turn cap 120), effort 2, roster general, round 1, open finding M1 — 2026-11-14
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1d95e-3bd5-79a3-a24e-4a43849c3058, max-turns 120, artifacts general.md, merged.md — 2026-11-14T09:31:46Z
- 2026-11-14 implement-fix-2 oracle (Claude Opus 5.5): fixed M1 (docs only). `glossary.md`
  `upstream pin`, `TW-only fix`, and `release pending` now match `HeroiconsVersion.ExtractUpstream`
  and the `X.Y.Z.R` scheme; `readme.md` contributing paragraph updated. Gates green.

## Results

### Changes

- `source/Directory.Build.props`: `<Version>` `2.2.0-tw.1` → `2.2.0.1-beta.1`.
- `tools/dev-cli/services/heroicons-version.cs` (new): `HeroiconsVersion.ExtractUpstream`
  returns the first three numeric parts (drops `-prerelease`, ignores non-numeric `+metadata`,
  still honours a numeric `+X.Y.Z` pin such as historical `2.0.19+2.0.18`).
- `update-icons-command.cs`: `ExtractUpstreamVersion` delegates to it, so `2.2.0.1-beta.1`
  reads as "equal to upstream 2.2.0" (no more "Props pin 2.2.0.1 is ahead").
  `BumpPackageVersion` writes the three-part upstream version. Design region rewritten for
  `X.Y.Z.R`.
- `releases.md`: new `2.2.0.1-beta.1` entry; versioning paragraph describes `X.Y.Z.R`.
  `2.2.0-tw.1` stays as a section because it **is** published on nuget.org.
- Tests: `tests/dev-cli/heroicons-version.extract-upstream.cs` (Jaribu runfile, 6 tests:
  three-part, four-part, four-part prerelease, three-part prerelease, numeric and non-numeric
  metadata). `tests/dev-cli/Directory.Build.props` compiles the helper straight from the dev CLI.
  Jaribu 1.0.0-beta.16 and Shouldly 4.3.0 added to `Directory.Packages.props`.
- `tools/dev-cli/services/runfile-tests.cs` (new): runs every `tests/dev-cli/*.cs` runfile.
  Called by `dev test`, the `dev workflow` Test step (CI), and the `update-icons` test step,
  since the solution has no test project that `dotnet test` would reach.

### Gates

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | pass, 0 errors (pre-existing IL2111 warnings in sample-app); rerun after pin bump: pass |
| Test | `./bin/dev test` | pass: solution + runfile tests 6/6; rerun after pin bump: pass |
| Audit | `ganda repo audit` | first run passed with 2 advisories; commit hook then failed `package-pin-currency`; after `ganda repo audit --fix` the rerun passes with 1 advisory (`ci-code-coverage`, pre-existing, non-blocking) |
| Version | `./bin/dev check-version` | pass: "Version in source: 2.2.0.1-beta.1 … Latest NuGet version: 2.2.0 … ✓ new — safe to release" |
| Release | `./bin/dev release --dry-run` | stops at "release must be cut from master" on the task branch (gh auth and clean-tree guards pass). Code read of Nuru.DevCli 4.0.0-beta.13 `release-guard.cs` / `nuget-version-service.version-comparison.cs`: tag is `v{Version}` string compare, core compare walks every numeric part (4 parts OK), `--prerelease` set from the `-` suffix. Run the dry run from master after merge. |

### Round-1 fix pass (M1)

- `glossary.md`: `upstream pin` = first three numeric parts (fourth part, `-prerelease`, and
  non-numeric `+metadata` ignored; numeric `+X.Y.Z` still names the pin, `2.0.19+2.0.18` →
  `2.0.18`). `TW-only fix` = `X.Y.Z.R` / `X.Y.Z.R-beta.N`, `-tw.N` named as the old form.
  `release pending` example lists `2.2.0.1` and `2.2.0.1-beta.1`.
- `readme.md`: "Syncing icons and releasing" describes the four-part scheme instead of `-tw.N`.

| Gate (fix pass) | Command | Result |
|-----------------|---------|--------|
| Build | `./bin/dev build` | pass, 0 errors |
| Test | `./bin/dev test` | pass, runfile tests 6/6, "Tests completed successfully!" |
| Audit | `ganda repo audit` | exit 0: 43 passed, 2 skipped, 1 non-blocking warning (`ci-code-coverage`, pre-existing advisory). The audit ran twice with no edit in between, only to capture the exit code. |

### How to validate

Smoke:

```bash
./bin/dev test                      # ends "Tests completed successfully!", runfile tests Passed: 6
dotnet run --file tests/dev-cli/heroicons-version.extract-upstream.cs
./bin/dev check-version
grep '<Version>' source/Directory.Build.props
```

Docs (M1): `grep -n 'upstream pin\|TW-only fix' glossary.md` and `sed -n 66,71p readme.md`.

Expect: glossary and readme describe `X.Y.Z.R` / `X.Y.Z.R-beta.N` and a three-part pin
matching the test cases below. `ExtractUpstream("2.2.0.1-beta.1") == "2.2.0"` and the other 5 cases pass;
`check-version` prints `Version in source: 2.2.0.1-beta.1` and "safe to release";
props shows `<Version>2.2.0.1-beta.1</Version>`. `./bin/dev update-icons` (needs network)
prints `Current heroicons version: 2.2.0` and takes the "equal to upstream" path, not
"Props pin … is ahead".

### Follow-ups

- Do not wave this repo until a ganda release contains timewarp-ganda task 641 (see Notes).
- After merge, `./bin/dev release --dry-run` from master is the remaining live check.

### Review

- Round 1. Roster: general. Effort: 2. Turn cap 120; this session recorded the cap as uncapped (no --max-turns flag).
- Counts: bug 1 open / 0 fixed / 0 wontfix; suggestion 0; nit 0. Fix pass: M1 fixed (bug 0 open / 1 fixed).
- Disposition: not written. M1 is open (`glossary.md` / `readme.md` still describe the `-tw.N` pin). Disposition waits until that finding is fixed or marked wontfix.
- The implementer restatement matches the original task. No misread under Learnings.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Review

- Round 1: Blocked — open M1

## Notes

- Needs a ganda release that contains `timewarp-ganda` task 641 before
  `ganda task wave`, `ganda repo tag`, or `ganda reposet status` handle a
  four-part version. Until then the wave plans `2.2.0-beta.2` from
  `2.2.0.1-beta.1`, which is wrong. Do not walk this repo in a wave before that.
- This repo's home branch is `main`.
- There is no `package.json` in this repo, so npm's three-part rule does not
  apply.
- `timewarp-architecture` consumes `timewarp-heroicons` and is blocked in wave run
  `20261114T054552Z-9a0f39` until this repo releases.
- Requested by Steve 2026-11-14: "We want the version to match heroicons version
  number", and on the four-part form, "that's the correct answer".
