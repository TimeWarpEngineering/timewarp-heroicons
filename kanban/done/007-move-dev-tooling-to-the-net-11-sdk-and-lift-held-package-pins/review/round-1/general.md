# Round 1 — general
**Date:** 2026-11-13
**Scope reviewed:** branch `task/007-move-dev-tooling-to-the-net-11-sdk-and-lift-held` vs `master` (merge-base `ad4e648`), product commit `d18a419`

## Summary

The change pins SDK `11.0.100` (`rollForward: latestFeature`), points both CI `setup-dotnet` steps at `11.0.x`, and lifts the six packages named in the task 005 hold: Amuru and Amuru.Tools `3.0.0-beta.2`, Nuru and Nuru.DevCli `4.0.0-beta.12`, Build.Tasks `2.0.0`, SourceGenerators `1.0.0-beta.15`. The `[ganda.audit]` hold is gone. The dev-cli handlers match the Nuru 4 endpoint shape (`IRequestHandler<TCommand, Unit>`, `ValueTask<Unit>`, `Unit.ValueTask` on `verify-samples`). `INuGetPackageService` and `IRepoCheckVersionService` are not in Nuru.DevCli 4.0.0-beta.12; the registrations that remain match that package's constructors (`IRepoCleanService`, `NuGetVersionService`, `IRepoConfigService`, `IPackableProjectService`).

`source/timewarp-heroicons` and `tests/sample-app` stay `net10.0`. That matches the restated decision. Build.Tasks 2.0.0 ships as `build/net11.0` (an SDK task) and SourceGenerators 1.0.0-beta.15 ships as an analyzer, so those pins do not force the library off `net10.0`. The implementer's recorded `## Results` cover self-install, `./bin/dev build`, `./bin/dev test`, `ganda repo audit` (`package-pin-currency` PASS), and the five `.githooks` runfiles compiling and exiting 0. The restatement matches the task once the Notes correction is applied (the hold is from task 005 and includes SourceGenerators). No misread.

Targeted check, because CI still calls `dotnet run tools/dev-cli/dev.cs` without `--file`: `dotnet --version` printed `11.0.100`, and `dotnet run --help` on that SDK still documents passing the file as the first argument when the directory has no project file. That is the form the workflow already used. Not a finding.

## Issues

None.
