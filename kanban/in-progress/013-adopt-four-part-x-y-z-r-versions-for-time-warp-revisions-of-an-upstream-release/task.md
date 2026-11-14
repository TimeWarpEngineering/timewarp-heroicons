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

- [ ] `<Version>` is `2.2.0.1-beta.1`
- [ ] `ExtractUpstreamVersion` returns three parts, with tests
- [ ] Policy text updated in the Design region and `releases.md`
- [ ] `dev check-version` and `dev release --dry-run` pass on the new version

## Session

- Created: e92789a1-bc49-442d-98c6-87a856ac7395 (2026-11-14)

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
