# Bump version to 2.2.0-tw.1 for the net10.0/net11.0 multi-target release

## Description

Scope changed at filing (Steve, 2026-11-13): **drop net10.0 and target net11.0 only**, then bump
the version for the release. Steve: "Moving it to 11.x would drop .NET 10 consumers. YES that is
what I want." Task 011 (PR #28) made the library multi-target `net10.0;net11.0` with a per-TFM
`Components.Web` pin (10.0.15 for net10.0, 11.0.0 otherwise); undo the net10 half.

The title still says multi-target; the actual release is **net11.0 only**.

## Goal (implementer restatement)

Ship 2.2.0-tw.1 as a **net11.0-only** package (breaking for .NET 10 consumers, who stay on 2.2.0).

Acceptance:
- Library csproj uses a single `<TargetFramework>net11.0</TargetFramework>`.
- `Directory.Packages.props` has one unconditional Components.Web 11.0.0 pin; the net10 pin and comment are gone; `ganda nuget outdated` clean.
- `tools/transform` targets net11.0; no `net10` left in any csproj/props.
- `<Version>2.2.0-tw.1</Version>`; `releases.md` has a `## 2.2.0-tw.1` section with the breaking note.
- readme badge + Supported frameworks say .NET 11 only and point .NET 10 users at 2.2.0.
- Packed nuspec: version 2.2.0-tw.1, single net11.0 group, Components.Web 11.0.0.
- build / test / audit green; no tag or GitHub release.

## Requirements

- `source/timewarp-heroicons/timewarp-heroicons.csproj`: `<TargetFramework>net11.0</TargetFramework>`
  (single TFM).
- `Directory.Packages.props`: one unconditional
  `Microsoft.AspNetCore.Components.Web` `PackageVersion` at 11.0.0 (latest 11.x); remove the
  conditional net10 pin and its comment. `ganda nuget outdated` must report nothing outdated.
- `tools/transform/transform.csproj`: net10.0 → net11.0 too, so nothing in the repo targets
  net10.0 (`grep -rn net10 --include=*.csproj --include=*.props` empty, excluding obj/bin).
- `source/Directory.Build.props`: `<Version>2.2.0-tw.1</Version>`.
- `releases.md`: new top section `## 2.2.0-tw.1` (above `## 2.2.0`), e.g.:
  - Target .NET 11 (net11.0) only; depends on Microsoft.AspNetCore.Components.Web 11.0.0.
  - **Breaking:** .NET 10 is no longer supported; stay on 2.2.0 for net10.0.
  - Icons unchanged (heroicons 2.2.0).
- readme: badge and "Supported frameworks" say .NET 11 only; mention 2.2.0 for .NET 10 users.
- Pack and inspect the `.nuspec`: single `net11.0` dependency group, Components.Web 11.0.0,
  version `2.2.0-tw.1`.
- `./bin/dev build`, `./bin/dev test`, `ganda repo audit` green. `dev check-version` (if it
  runs locally) accepts 2.2.0-tw.1.
- Do not tag or create a GitHub release; `dev release` runs after merge.

## Checklist

- [x] Library net11.0 only
- [x] Single Components.Web 11.0.0 pin; `ganda nuget outdated` clean
- [x] transform tool net11.0; no net10 left
- [x] Version 2.2.0-tw.1 + releases.md section (breaking note)
- [x] readme frameworks
- [x] nuspec checked
- [x] build / test / audit green

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- Breaking change to a stable package → walk launched with `--no-merge` (global policy); merge
  with `ganda pr merge --task-id 012`, then `dev release` from clean synced master.

## Results

- Library `source/timewarp-heroicons/timewarp-heroicons.csproj` → `<TargetFramework>net11.0</TargetFramework>`.
- `Directory.Packages.props`: one unconditional `Microsoft.AspNetCore.Components.Web` 11.0.0; net10 pin and comment removed.
- `tools/transform/transform.csproj` → net11.0. No `net10` left in any csproj/props.
- `source/Directory.Build.props` → `<Version>2.2.0-tw.1</Version>`.
- `releases.md`: new `## 2.2.0-tw.1` section (net11.0 only, breaking note for .NET 10, icons unchanged).
- `readme.md`: badge says `dotnet 11.0`; Supported frameworks says net11.0 only and sends .NET 10 users to 2.2.0.
- Packed `timewarp-heroicons.2.2.0-tw.1.nupkg` nuspec: `<version>2.2.0-tw.1</version>`, single
  `<group targetFramework="net11.0">` with `Microsoft.AspNetCore.Components.Web` 11.0.0.
- No tag or GitHub release created.

### Gates

| Command | Result |
|---------|--------|
| `./bin/dev build` | Pass — 0 errors, 9 warnings (pre-existing TWA0004 / RS0030 / IL211x) |
| `./bin/dev test` | Pass — "Tests completed successfully!" |
| `ganda nuget outdated` | Pass — all 11 packages up to date |
| `./bin/dev check-version` | Pass — 2.2.0-tw.1 is new vs NuGet 2.2.0 |
| `ganda repo audit` | Pass — 42 passed, 2 advisory warnings (pre-existing `ci-code-coverage`, `vscode-window-icon`); `ganda repo audit --fix` changed nothing |

### How to validate

Smoke:

```bash
grep -rn net10 --include=*.csproj --include=*.props . | grep -v '/obj/\|/bin/'
./bin/dev build
unzip -p source/timewarp-heroicons/bin/Release/timewarp-heroicons.2.2.0-tw.1.nupkg timewarp-heroicons.nuspec | grep -n 'version\|group\|dependency'
./bin/dev check-version
```

Expect: the grep prints nothing; build succeeds; the nuspec shows `<version>2.2.0-tw.1</version>`
and exactly one `<group targetFramework="net11.0">` with Components.Web `11.0.0`; check-version
reports 2.2.0-tw.1 as new.

## Follow-ups

- Advisory audit warnings `ci-code-coverage` (repo has no test project, only `tests/sample-app`) and
  `vscode-window-icon` (`.vscode/settings.json` missing) pre-date this task and are not auto-fixable.
- Breaking change to a stable package: merge via `ganda pr merge --task-id 012`, then `dev release`.
