# Releases

## 2.2.0.1-beta.1

* Re-ships the `2.2.0-tw.1` content (`net11.0` only, Microsoft.AspNetCore.Components.Web
  11.0.0, icons from heroicons 2.2.0) under a version that sorts after `2.2.0`.
* Version scheme: TimeWarp-only revisions now use a fourth part (`2.2.0.1`) instead of
  `-tw.N`. NuGet sorts `2.2.0-tw.1` before `2.2.0`, so an update from `2.2.0-tw.1`
  offered the older `net10.0` build.

## 2.2.0-tw.1

* Target .NET 11 (`net11.0`) only; depends on Microsoft.AspNetCore.Components.Web 11.0.0.
* **Breaking:** .NET 10 is no longer supported. Stay on `2.2.0` for `net10.0`.
* Icons unchanged (heroicons 2.2.0).

## 2.2.0

* Update to heroicons version 2.2.0

Versioning: the first three parts of the NuGet version always **match**
[heroicons](https://github.com/tailwindlabs/heroicons) (`2.2.0` = heroicons 2.2.0).
TW-only fixes without a new heroicons release add a fourth part, `X.Y.Z.R`
(`2.2.0.1`, `2.2.0.2`), with prereleases `X.Y.Z.R-beta.N`. NuGet orders
`2.2.0 < 2.2.0.1-beta.1 < 2.2.0.1 < 2.2.0.2 < 2.2.1`, so a revision sorts after the
release it revises. `X.Y.Z.R` replaces the old `-tw.N` suffix: a prerelease label sorts
before the plain version, so `2.2.0-tw.1` was older than `2.2.0`. `+` metadata is not an
option either, because NuGet ignores it. When heroicons ships a new version the package
goes to that three-part version and the revision starts again.
Historical note: `2.0.19` was a TW rework of heroicons **2.0.18**, not an upstream tag.

## 2.0.19+2.0.18

* Update to optimized version 2.0.18
* Icon can now be use with colors

## 2.0.18-update

* Update to version 2.0.18

## 2.0.12-update.1

* Changed from ComponentBase to IComponent ( Thanks @chan18 )
* Updated PowerShell scripts
* Released as update since we already used the version number and Heroicons has not updated

## 2.0.12+build-1

* Original release
