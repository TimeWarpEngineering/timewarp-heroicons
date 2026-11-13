# Multi-target the library for net10.0 and net11.0 (ASP.NET Components 11)

## Description

`ganda nuget outdated` (2026-11-13) reports `Microsoft.AspNetCore.Components.Web`,
`Components.WebAssembly` and `Components.WebAssembly.DevServer` at 10.0.3 → 11.0.0.
`Components.Web` is the only dependency of the published package
(`source/timewarp-heroicons/timewarp-heroicons.csproj`, `net10.0`). The 11.x packages target
net11.0, so bumping in place would drop every .NET 10 consumer — a breaking change to a stable
package. Instead, multi-target so .NET 10 consumers keep working and .NET 11 consumers get the
11.x dependency. The repo already builds on the .NET 11 SDK (task 007).

## Requirements

- Library: `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>`.
- Central package management with per-TFM versions: `Components.Web` 10.0.x for net10.0
  (latest 10.0 patch) and 11.0.0 for net11.0, e.g. conditional `PackageVersion` items in
  `Directory.Packages.props` keyed on `$(TargetFramework)`. Confirm with a pack + inspect
  `.nuspec`: two dependency groups, net10.0 → 10.0.x, net11.0 → 11.0.x.
- `tests/sample-app`: move to net11.0 with WebAssembly / DevServer 11.0.0 (it is not
  shipped), or keep it on net10 if WASM tooling on 11 is not ready — record which and why.
- Check `HeroIconBase.cs` and the generated components still compile with no new
  warnings on both TFMs.
- Versioning: do **not** change `<Version>` or `releases.md` here. Adding a TFM is a
  package-only change with no new heroicons release; per the policy in
  `tools/dev-cli/endpoints/update-icons-command.cs` (Design region) the next ship would be a
  TW-only prerelease such as `2.2.0-tw.1`. Note that in the PR description; cutting a release
  waits for an explicit ask.
- readme: state supported frameworks (net10.0, net11.0).
- `./bin/dev build`, `./bin/dev test`, `./bin/dev verify-samples` (if applicable),
  `ganda repo audit` green; `ganda nuget outdated` no longer lists the three packages (or
  lists only the net10 pin, which is intentional — note it).

## Goal (implementer restatement)

Ship `timewarp-heroicons` for both .NET 10 and .NET 11 without dropping .NET 10 consumers.
The library targets `net10.0;net11.0`; central package management picks
`Microsoft.AspNetCore.Components.Web` 10.0.x (latest patch, 10.0.15) for net10.0 and 11.0.0
for net11.0, so the packed `.nuspec` carries two dependency groups.

Acceptance:

- `timewarp-heroicons.csproj` has `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>`.
- `Directory.Packages.props` keys the `Components.Web` version on `$(TargetFramework)`;
  a `dotnet pack` shows net10.0 → 10.0.15 and net11.0 → 11.0.0 in the nuspec.
- `tests/sample-app` moves to net11.0 with WebAssembly/DevServer 11.0.0 if it builds, else
  stays on net10 with the reason recorded.
- Both TFMs compile with no new warnings.
- readme states supported frameworks (net10.0, net11.0).
- `<Version>` and `releases.md` stay untouched (next ship would be a TW-only prerelease).
- `./bin/dev build`, `./bin/dev test`, `ganda repo audit` green; `ganda nuget outdated`
  result noted.

## Checklist

- [x] Library multi-targets net10.0;net11.0
- [x] Per-TFM Components.Web versions; nuspec shows two dependency groups
- [x] sample-app decision (net11 or stay) recorded
- [x] Both TFMs build clean
- [x] readme supported frameworks
- [x] build / test / audit green

## Depends on

- 010

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 771e57b7-bfcb-4449-9142-e5b1702fe97c, max-turns 500, Done — 2026-11-13T13:59:30Z
- Review oracle: general (cursor, model grok-4.7), session not reported, effort 2, turn cap 120 (cursor uncapped), disposition clean — 2026-11-13
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts general.md, merged.md, disposition.md — 2026-11-13
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts general.md, merged.md, disposition.md — 2026-11-13T14:07:27Z

## Review

- Round 1: Done

## Notes

- Launched with `--no-merge` (Steve, 2026-11-13): stable-package dependency change stops at the
  PR for human review.

## Results

### Changes

- `source/timewarp-heroicons/timewarp-heroicons.csproj`: `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>`.
- `Directory.Packages.props`: `Microsoft.AspNetCore.Components.Web` has two `PackageVersion`
  items keyed on `$(TargetFramework)`: 10.0.15 (latest 10.0 patch on nuget.org) for net10.0,
  11.0.0 otherwise. `Components.WebAssembly` and `.DevServer` move to 11.0.0 (sample-app only).
  Also put the `TimeWarp.SourceGenerators` item on its own line (formatting only).
- `tests/sample-app/sample-app.csproj`: net10.0 → net11.0. **Decision: move to net11.** It is
  not shipped, the .NET 11 SDK (11.0.100) is GA, and the WASM app builds and publishes cleanly on
  11.0.0 with no workload needed. Through the project reference it consumes the library's
  net11.0 asset.
- `readme.md`: dotnet badge now says 10.0 | 11.0 (was 6.0), and a new "Supported frameworks"
  section lists net10.0 and net11.0 with their Components.Web dependency lines.
- `<Version>` (2.2.0) and `releases.md` stay unchanged. Adding a TFM is a package-only change
  with no new heroicons release. Under the Design-region policy in
  `tools/dev-cli/endpoints/update-icons-command.cs`, the next ship would be a TW-only
  prerelease such as `2.2.0-tw.1`. Cutting it waits for an explicit ask. **PR note:** put this
  in the PR description.

### Warnings

Both library TFMs compile with no new warnings. The only library diagnostic is the existing
`TWA0004` on `HeroIconBase.cs`, now reported once per TFM. The sample-app's `IL2110`/`IL2111`
trimmer warnings in `App_razor.g.cs` (`Router.NotFoundPage`, `LayoutView.Layout`) are
pre-existing. A build of the base commit on net10.0 shows the same three.

### Gates

- `./bin/dev build`: passed (0 errors; warnings as above). Pack produced
  `timewarp-heroicons.2.2.0.nupkg` with `lib/net10.0` and `lib/net11.0`.
- `./bin/dev test`: passed ("Tests completed successfully!").
- `./bin/dev verify-samples`: passed ("Samples verified successfully!").
- `ganda repo audit`: passed (exit 0, "Repository passes — 2 advisory (non-blocking) warning(s)").
  Both warnings were already there and are the same ones recorded in tasks 007–010:
  `ci-code-coverage` (the repo has no unit-test suite) and `vscode-window-icon`
  (`.vscode/settings.json` is git-ignored and local only).
- `ganda nuget outdated`: lists only `Microsoft.AspNetCore.Components.Web 10.0.15 -> 11.0.0`,
  which is the intentional net10.0 pin. Its 11.0.0 entry, WebAssembly and DevServer report up
  to date.

### How to validate

Smoke:

```bash
./bin/dev build
unzip -p source/timewarp-heroicons/bin/Release/timewarp-heroicons.2.2.0.nupkg timewarp-heroicons.nuspec \
  | sed -n '/<dependencies>/,/<\/dependencies>/p'
./bin/dev test
./bin/dev verify-samples
```

Expect: the build succeeds and the nupkg has `lib/net10.0` and `lib/net11.0`. The nuspec
shows two groups: `net10.0` → `Microsoft.AspNetCore.Components.Web` 10.0.15 and `net11.0` →
11.0.0. Test and verify-samples pass, and `sample-app` builds to `bin/Release/net11.0`.

### Review

- Round 1, roster general, effort 2. Disposition: **clean**. Final open count: 0.
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0 / 0 / 0; nit 0 / 0 / 0. No wontfix, no escalation.
- The implementer restatement matches the original task. No misread.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
