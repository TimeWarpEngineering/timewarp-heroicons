# Move dev tooling to the .NET 11 SDK and lift held package pins

## Description

`ganda repo audit` (`package-pin-currency`) wants these pins bumped, but every newer
release needs the .NET 11 SDK and this repo's `global.json` pins SDK 10.0.301
(`rollForward: latestFeature`):

- `TimeWarp.Amuru` 1.0.0 → 3.0.0-beta.2 (net11.0 only, NU1202 on net10.0)
- `TimeWarp.Amuru.Tools` 1.0.0-beta.2 → 3.0.0-beta.2 (net11.0 only)
- `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` 3.0.0-beta.74 → 4.0.0-beta.12 (net11 companions)
- `TimeWarp.Build.Tasks` 1.0.0 → 2.0.0 (TWBT0001: 2.x requires the .NET 11 SDK)

Task 006 held these pins with `package-pin-currency.hold` under `[ganda.audit]` in
`.editorconfig`, citing this task. This task moves the dev tooling (dev-cli runfile,
`.githooks` runfiles, CI `setup-dotnet`) to the .NET 11 SDK, bumps the pins, adapts the
dev-cli to the Amuru 3 / Nuru 4 APIs, and deletes the holds. Decide whether the
published library `source/timewarp-heroicons` stays on `net10.0` (SDK 11 can still
target it) or multi-targets.

## Checklist

- [ ] `global.json` → .NET 11 SDK; CI `setup-dotnet` → `11.0.x`
- [ ] Bump the five held pins; fix dev-cli build breaks (Amuru 3, Nuru 4)
- [ ] `.githooks/*.cs` runfiles compile and exit 0 on the new pins
- [ ] Delete the `package-pin-currency.hold` entry from `.editorconfig`
- [ ] `./bin/dev build`, `./bin/dev test`, `ganda repo audit` clean

## Session

- Created: 85400 (2026-11-13), filed from task 006 (audit boy-scout pass)

## Notes

- Library consumers on net10.0 must keep working; check before retargeting the package.
