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

## Depends on

- 005

## Goal (implementer restatement)

Move the repo's build tooling to the .NET 11 SDK so the held TimeWarp package pins can move to
their newest releases, then remove the `package-pin-currency.hold` entry that task 005 left in
`.editorconfig`.

Acceptance criteria:

- `global.json` pins the .NET 11 SDK (11.0.100, `rollForward: latestFeature`); both CI
  `setup-dotnet` steps install `11.0.x`.
- `Directory.Packages.props` pins Amuru 3.0.0-beta.2, Amuru.Tools 3.0.0-beta.2,
  Nuru 4.0.0-beta.12, Nuru.DevCli 4.0.0-beta.12, Build.Tasks 2.0.0, SourceGenerators 1.0.0-beta.15.
- The dev-cli (`tools/dev-cli/dev.cs`) compiles and runs on Amuru 3 / Nuru 4 (`./bin/dev` rebuilt
  via self-install).
- Every `.githooks/*.cs` runfile compiles and exits 0 on the new pins.
- The published library `source/timewarp-heroicons` stays on `net10.0` (SDK 11 still targets it),
  so net10 consumers keep working; the sample app stays on net10.0 too.
- The `package-pin-currency.hold` entry is gone; `./bin/dev build`, `./bin/dev test`, and
  `ganda repo audit` pass.

## Checklist

- [x] `global.json` → .NET 11 SDK; CI `setup-dotnet` → `11.0.x`
- [x] Bump the five held pins; fix dev-cli build breaks (Amuru 3, Nuru 4)
- [x] `.githooks/*.cs` runfiles compile and exit 0 on the new pins
- [x] Delete the `package-pin-currency.hold` entry from `.editorconfig`
- [x] `./bin/dev build`, `./bin/dev test`, `ganda repo audit` clean

## Session

- Created: 85400 (2026-11-13), filed from task 006 (audit boy-scout pass)
- 2026-11-13 implement oracle (claude): SDK 11.0.100, pins bumped, dev-cli ported to the
  Nuru 4 / Mediator handler API, `package-pin-currency.hold` removed. Gates green.

## Notes

- Library consumers on net10.0 must keep working; check before retargeting the package.
- Groom 2026-11-13 (updated after merges): 006 was closed as superseded by 005 (PR #22 closed;
  the hook redeploy shipped in #23), so it is not a predecessor. 005 is merged and set the current
  baseline on master: Amuru 2.0.0, Amuru.Tools 2.0.0, Nuru/DevCli 3.0.0, Build.Tasks 1.0.2,
  SourceGenerators 1.0.0-beta.14, with a `package-pin-currency.hold` entry in `.editorconfig`
  that cites task 005 (not 006). The version list and the "Task 006 held these pins" sentence
  above are stale: re-read `Directory.Packages.props` and `.editorconfig` on master first.

## Results

Changes:

- `global.json`: SDK `10.0.301` → `11.0.100` (`rollForward: latestFeature` kept).
- `.github/workflows/workflow.yml`: both `setup-dotnet` steps `10.0.x` → `11.0.x`.
- `Directory.Packages.props`: Amuru 2.0.0 → 3.0.0-beta.2, Amuru.Tools 2.0.0 → 3.0.0-beta.2,
  Nuru 3.0.0 → 4.0.0-beta.12, Nuru.DevCli 3.0.0 → 4.0.0-beta.12, Build.Tasks 1.0.2 → 2.0.0,
  SourceGenerators 1.0.0-beta.14 → 1.0.0-beta.15.
- dev-cli: handlers now implement `IRequestHandler<TCommand, Unit>` and return `ValueTask<Unit>`
  (Nuru 4 / TimeWarp.Mediator API; `ICommandHandler` is gone); `verify-samples` returns
  `Unit.ValueTask`. Dropped the `INuGetPackageService` / `IRepoCheckVersionService`
  registrations from `dev.cs`: those services are gone from Nuru.DevCli 4 (NURU051 on
  `NuGetPackageService`), and the sibling multiavatar dev-cli on the same pins drops them too.
- `.editorconfig`: removed the `[ganda.audit]` section holding `package-pin-currency.hold`
  (its only entry). `package-pin-currency` now passes with no hold.
- Library decision: `source/timewarp-heroicons` stays `net10.0` (and `tests/sample-app` too).
  SDK 11 builds net10.0 fine, so net10 consumers are unaffected; only dev tooling moved to net11.
- `.githooks/*.cs`: no source change needed. All five (post-checkout, post-commit, post-merge,
  pre-commit, pre-push) compiled on the new pins via `dotnet run --file` and exited 0.

### Gates

- `dotnet run --file tools/dev-cli/dev.cs -- self-install` (rebuild `./bin/dev` after the
  dev-cli change): success.
- `./bin/dev build`: exit 0, "Build succeeded". Only warnings are the pre-existing NU1902–NU1904
  advisories on `Scriban` 5.12.0 in `tools/transform` (pin unchanged by this task) and TWA0038
  from Nuru.DevCli package content files.
- `./bin/dev test`: exit 0, "Tests completed successfully!".
- `ganda repo audit`: exit 0, "Repository passes — 2 advisory (non-blocking) warning(s)":
  `ci-code-coverage` and `vscode-window-icon` (missing ignored `.vscode/settings.json`), both
  pre-existing and unrelated to this change. `package-pin-currency` PASS.

### How to validate

Smoke:

```bash
dotnet --version                       # resolves 11.0.x via global.json
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev build && ./bin/dev test
GANDA_ATTEST_HOOK=0 dotnet run --file .githooks/pre-commit.cs; echo $?
ganda repo audit
grep -n 'package-pin-currency.hold' .editorconfig
```

Expect: SDK 11.0.x; self-install, build, and test exit 0; the hook prints nothing and exits 0
on a task branch; audit passes with `package-pin-currency` PASS; the grep finds nothing.

### Follow-ups

- `Scriban` 5.12.0 (tools/transform) has known NU1903/NU1904 vulnerabilities; worth a bump task.
