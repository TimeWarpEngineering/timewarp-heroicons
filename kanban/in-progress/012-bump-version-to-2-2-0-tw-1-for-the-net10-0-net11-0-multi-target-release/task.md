# Bump version to 2.2.0-tw.1 for the net10.0/net11.0 multi-target release

## Description

Scope changed at filing (Steve, 2026-11-13): **drop net10.0 and target net11.0 only**, then bump
the version for the release. Steve: "Moving it to 11.x would drop .NET 10 consumers. YES that is
what I want." Task 011 (PR #28) made the library multi-target `net10.0;net11.0` with a per-TFM
`Components.Web` pin (10.0.15 for net10.0, 11.0.0 otherwise); undo the net10 half.

The title still says multi-target; the actual release is **net11.0 only**.

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

- [ ] Library net11.0 only
- [ ] Single Components.Web 11.0.0 pin; `ganda nuget outdated` clean
- [ ] transform tool net11.0; no net10 left
- [ ] Version 2.2.0-tw.1 + releases.md section (breaking note)
- [ ] readme frameworks
- [ ] nuspec checked
- [ ] build / test / audit green

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- Breaking change to a stable package → walk launched with `--no-merge` (global policy); merge
  with `ganda pr merge --task-id 012`, then `dev release` from clean synced master.
