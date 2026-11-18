# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-heroicons

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-heroicons.
Filter: (all)
Outdated:
  TimeWarp.Amuru 3.0.0-beta.5 -> 3.0.0-beta.10 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Amuru.Tools 3.0.0-beta.5 -> 3.0.0-beta.10 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Jaribu 1.0.0-beta.16 -> 1.0.0-beta.18 (dev-only: tests/dev-cli/Directory.Build.props)
  TimeWarp.Nuru 4.0.0-beta.15 -> 4.0.0-beta.16 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.15 -> 4.0.0-beta.16 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.SourceGenerators 1.0.0-beta.16 -> 1.0.0-beta.17 (dev-only: PrivateAssets=all on Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the six dev-only TimeWarp package pins in `Directory.Packages.props` to their latest versions
(Amuru/Amuru.Tools 3.0.0-beta.10, Jaribu 1.0.0-beta.18, Nuru/Nuru.DevCli 4.0.0-beta.16,
SourceGenerators 1.0.0-beta.17) without touching `<Version>` in `source/Directory.Build.props`.

Acceptance criteria:
- Only `Directory.Packages.props` changes for product files; the six pins match the targets above.
- `./bin/dev build` passes and the dev-cli tests that exercise these packages still pass.
- `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261118T170809Z-b6e473
- Implementer: bumped the six dev-only pins in `Directory.Packages.props`. Amuru 3.0.0-beta.10 adds
  analyzer AMURU0001, which failed the dev CLI build on raw `Shell.Builder("gh"/"dotnet")` calls. Moved four
  call sites to the typed Amuru.Tools builders (`Gh.Pr().Create()`, `Gh.Pr().Merge(branch)`, `DotNet.Run()`).
  A throwaway runfile printed `Build().ToCommandString()` for each builder. The command lines are the same as before:
  `dotnet run --project p.csproj -- in out`, `dotnet run --file t.cs`,
  `gh pr create --title … --body … --base … --head …`, `gh pr merge <branch> --merge --auto`.
  `<Version>` in `source/Directory.Build.props` is unchanged.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-18T18:10:43Z
- Reviewer: grok session 01a1efda-bb67-7053-8699-84dae62f18f1 (2026-11-19), effort 1, roster general, turn cap 80
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1efda-bb67-7053-8699-84dae62f18f1, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-18T18:16:49Z

## Results

Changed files:
- `Directory.Packages.props`: Amuru and Amuru.Tools 3.0.0-beta.10, Jaribu 1.0.0-beta.18, Nuru and Nuru.DevCli 4.0.0-beta.16,
  SourceGenerators 1.0.0-beta.17.
- `tools/dev-cli/endpoints/update-icons-pull-request.cs`: `gh pr create` and `gh pr merge` now use `Gh.Pr()` builders (AMURU0001).
- `tools/dev-cli/endpoints/update-icons-regenerate.cs`: the transform `dotnet run --project` now uses `DotNet.Run()` (AMURU0001).
- `tools/dev-cli/services/runfile-tests.cs`: the runfile `dotnet run --file` now uses `DotNet.Run().WithFile()` (AMURU0001).

Gates:
- `./bin/dev build`: passed, 0 errors (9 warnings that were already there: TWA0004, RS0030, IL211x in the sample app).
- `dotnet run --file tools/dev-cli/dev.cs -- self-install`: the first run failed with 4× AMURU0001. After the fix it passed and installed `./bin/dev`.
- `dotnet run --file tests/dev-cli/heroicons-version.extract-upstream.cs`: passed, 6/6 (Jaribu 1.0.0-beta.18).
  No test file covers `RunfileTests` or `UpdateIconsCommand`. CI's full `dev test` runs `RunfileTests`.
- `ganda repo audit`: passed (exit 0), 43 passed, 2 skipped. Two advisory warnings that were already there and do not block:
  `ci-code-coverage` (CI does not collect coverage) and `vscode-window-icon` (no `.vscode/settings.json`). Both are out of scope for a pin refresh.

### How to validate

Smoke:
```bash
./bin/dev build
dotnet run --file tools/dev-cli/dev.cs -- self-install
dotnet run --file tests/dev-cli/heroicons-version.extract-upstream.cs
grep -E 'TimeWarp\.(Amuru|Jaribu|Nuru|SourceGenerators)' Directory.Packages.props
```

Expect: the build succeeds with 0 errors. self-install succeeds with no AMURU0001. The runfile test reports 6/6 passed.
The grep shows Amuru/Amuru.Tools 3.0.0-beta.10, Jaribu 1.0.0-beta.18, Nuru/Nuru.DevCli 4.0.0-beta.16,
and SourceGenerators 1.0.0-beta.17.

### Review

- Rounds: 1. Roster: general. Effort: 1.
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0 / 0 / 0; nit 0 / 0 / 0. Final open count: 0.
- Disposition: clean. No wontfix and no escalation.
- The goal restatement matches the task. No misread.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Review

- Round 1: Done
