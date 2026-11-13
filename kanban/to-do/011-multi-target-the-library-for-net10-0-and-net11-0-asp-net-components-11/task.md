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

## Checklist

- [ ] Library multi-targets net10.0;net11.0
- [ ] Per-TFM Components.Web versions; nuspec shows two dependency groups
- [ ] sample-app decision (net11 or stay) recorded
- [ ] Both TFMs build clean
- [ ] readme supported frameworks
- [ ] build / test / audit green

## Depends on

- 010

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- Launched with `--no-merge` (Steve, 2026-11-13): stable-package dependency change stops at the
  PR for human review.
