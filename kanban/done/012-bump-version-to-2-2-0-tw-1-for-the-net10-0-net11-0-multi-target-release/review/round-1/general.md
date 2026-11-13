# Round 1 — general
**Date:** 2026-11-13
**Scope reviewed:** branch `task/012-bump-version-to-2-2-0-tw-1-for-the-net10-0-net11-0` vs merge-base `320b1ec`. Product diff is `Directory.Packages.props`, `source/timewarp-heroicons/timewarp-heroicons.csproj`, `tools/transform/transform.csproj`, `source/Directory.Build.props`, `readme.md`, and `releases.md`. Surrounding projects (`tests/sample-app`, workflows) were read; they were not changed.

## Summary

The library is a single `net11.0` target, `tools/transform` is `net11.0`, and `Microsoft.AspNetCore.Components.Web` is one unconditional `11.0.0` pin. `<Version>` is `2.2.0-tw.1`. `releases.md` adds that section above `## 2.2.0` with the .NET 10 break and the unchanged heroicons 2.2.0 note. The readme badge is `dotnet 11.0`, and Supported frameworks names `net11.0` only and points .NET 10 users at `2.2.0`. No `net10` remains in product csproj or props. `tests/sample-app` was already `net11.0`, and both workflows already install the `11.0.x` SDK. The pack already on disk at `source/timewarp-heroicons/bin/Release/timewarp-heroicons.2.2.0-tw.1.nupkg` has `<version>2.2.0-tw.1</version>`, one `<group targetFramework="net11.0">` depending on Components.Web `11.0.0`, and `lib/net11.0` only (`unzip -p` / `unzip -l` of that existing nupkg; not a rebuild). HEAD has no tag. The implementer restatement matches the original task, including the filing note that the title still says multi-target while the release is net11.0 only. Recorded Results cover build, test, `ganda nuget outdated`, `./bin/dev check-version`, and `ganda repo audit`. No issues.

## Issues
