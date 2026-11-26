# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-heroicons

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-heroicons.
Filter: (all)
Outdated:
  TimeWarp.Nuru 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the two dev-only Nuru pins in `Directory.Packages.props` from 4.0.0-beta.16 to 4.0.0-beta.17
(`TimeWarp.Nuru`, `TimeWarp.Nuru.DevCli`; both consumed only by `tools/dev-cli`).

Acceptance:
- `Directory.Packages.props` pins both packages at 4.0.0-beta.17; no other file's package pins change.
- `<Version>` in `source/Directory.Build.props` is unchanged (no release).
- The dev CLI compiles and runs against beta.17; `./bin/dev build` and the dev-cli tests pass; `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261126T021837Z-0cb631
- Implementer: bumped TimeWarp.Nuru and TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 in `Directory.Packages.props`; rebuilt `bin/dev` via self-install.
- Boy-scout audit fix: `dev-cli-telemetry` (Error, pre-existing) failed; added `.UseTelemetry(options => options.ServiceName = "timewarp-heroicons-dev")` to the `NuruApp.CreateBuilder()` chain in `tools/dev-cli/dev.cs`.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 61512aa2-26a2-4166-a3c6-bc9069f5bfee, max-turns 500, Done — 2026-11-26T02:42:22Z
- Review oracle: round 1 general by grok (xAI, Grok 4.7), session 01a215bb-ad5f-7053-950e-0b7b09261a05, max-turns 80, disposition clean — 2026-11-26
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a215bb-ad5f-7053-950e-0b7b09261a05, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-26T02:45:28Z

## Results

Changed files:
- `Directory.Packages.props`: TimeWarp.Nuru and TimeWarp.Nuru.DevCli pinned at 4.0.0-beta.17.
- `tools/dev-cli/dev.cs`: telemetry turned on (audit `dev-cli-telemetry` fix).
- `source/Directory.Build.props` `<Version>` unchanged.

Gates:
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` (dev CLI compiles against beta.17): success, run before and after the dev.cs edit.
- `./bin/dev build`: success, 0 errors (9 pre-existing IL trimming warnings in tests/sample-app).
- `dotnet run --file tests/dev-cli/heroicons-version.extract-upstream.cs`: 6/6 passed (no test file added or edited; this is the only test covering dev-cli code).
- `ganda repo audit`: first run exit 1 (`dev-cli-telemetry` Error); after the fix, exit 0. Remaining warnings, not errors: `ci-code-coverage`, `vscode-window-icon` (pre-existing, left for a separate pass).

Review:
- Round 1, effort 1, roster general (grok session 01a215bb-ad5f-7053-950e-0b7b09261a05, 2026-11-26).
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0; nit 0.
- Disposition: clean. No wontfix and no escalation. The implementer restatement matches the task; no misread recorded.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke: `dotnet run --file tools/dev-cli/dev.cs -- self-install && ./bin/dev --help && ./bin/dev build`
Expect: the dev CLI publishes against TimeWarp.Nuru 4.0.0-beta.17, `--help` prints "Development CLI for timewarp-heroicons" with its command list, the build ends with "Build completed successfully!", and `ganda repo audit` exits 0.

## Review

- Round 1: Done
