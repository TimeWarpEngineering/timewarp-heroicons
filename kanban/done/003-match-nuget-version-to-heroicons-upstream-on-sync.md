# Match NuGet version to heroicons upstream on sync

## Description

Fix `dev update-icons` versioning so package identity tracks tailwindlabs/heroicons
exactly on sync, and document TW-only fix releases (`X.Y.Z-tw.N`).

## Requirements

- On icon sync, set `<Version>` to the upstream heroicons version (e.g. `2.2.0`), not patch++
- Extract upstream pin correctly from clean, `+metadata`, and `-tw.N` / `-update.N` forms
- Do not invent disconnected versions like `2.0.19` while still on heroicons 2.0.18
- Document policy for humans (releases.md + Design region)

## Checklist

- [x] Replace `BumpPackageVersion` patch++ with clean upstream match
- [x] Harden `ExtractUpstreamVersion` for `-tw.N` / `-update.N`
- [x] Pack path uses NuGet identity (strip `+` metadata for `.nupkg` name)
- [x] Document policy in Design region and releases.md
- [ ] Follow-up (separate): sync icons to 2.2.0 and publish as 2.2.0

## Notes

Latest upstream is 2.2.0; we are still content-pinned at 2.0.18 with NuGet 2.0.19
(historical screwup). This task only fixes the bumber/policy so the next sync ships
the correct identity.

## Session

- Implementation: grok (2026-08-12)

## Results

### What changed
- `BumpPackageVersion` now returns the upstream heroicons version unchanged (match).
- `ExtractUpstreamVersion` supports `+pin` and prerelease bases (`2.2.0-tw.1` → `2.2.0`).
- Pack looks for `timewarp-heroicons.{identity}.nupkg` without `+` metadata.
- Policy documented in command Design region and `releases.md`.

### How to validate
- Smoke: `dotnet run --file tools/dev-cli/dev.cs -- update-icons --help` succeeds.
- Code review: `BumpPackageVersion(_, "2.2.0")` must return `"2.2.0"`; extract from
  `2.2.0-tw.1` and `2.0.19+2.0.18` yields `2.2.0` / `2.0.18`.
- Does **not** publish 2.2.0 by itself — that is a separate sync+release step.

