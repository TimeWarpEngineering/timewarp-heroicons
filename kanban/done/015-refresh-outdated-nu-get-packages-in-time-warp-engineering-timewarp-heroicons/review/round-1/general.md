# Round 1 — general
**Date:** 2026-11-19
**Scope reviewed:** branch `task/015-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (merge-base `ca6b0acf`): `Directory.Packages.props` and the three dev-cli call sites, checked against the task and `## Goal (implementer restatement)`.

## Summary

The six central pins match the task targets, and `source/Directory.Build.props` `<Version>` is unchanged. Package references for these packages have no per-project version, so `Directory.Packages.props` is the only pin. Amuru 3.0.0-beta.10 makes AMURU0001 an error for `dotnet` verbs and `gh` commands that have typed builders. The four migrated sites are the ones that diagnostic covers. The remaining `Shell.Builder("gh")` in `update-icons-lookups.cs` is `gh pr list`, and `Gh.Pr()` has no `List()`, so the analyzer does not report it. That matches the recorded 4× AMURU0001 on the first self-install.

The restatement matches the task, including the "edit `Directory.Packages.props` only" limit. The extra `.cs` edits are a necessary deviation recorded in `## Session` and `## Results`, not a misread. `DotNet.Run().WithProject(...).WithArguments(...)` still emits `dotnet run --project <path> -- <args>`, and `WithFile` emits `dotnet run --file <path>`. `Gh.Pr().Create()` emits `gh pr create` with `--title`, `--body`, `--base`, and `--head`. `Gh.Pr().Merge(branch).WithAuto().WithMergeCommit()` emits `gh pr merge <branch> --merge --auto`. Flag order differs from the old `WithArguments` lists; `gh` treats those flags as independent. `WithNoValidation` still means "do not throw on a non-zero exit" and is the default. `WithWorkingDirectory`, `CaptureAsync`, and `RunAsync` are unchanged in role.

Recorded gates cover the pin bump: `./bin/dev build` (0 errors), self-install after the AMURU0001 fix, the Jaribu runfile `tests/dev-cli/heroicons-version.extract-upstream.cs` (6/6), and `ganda repo audit` (exit 0). No test runs `UpdateIconsCommand` or `RunfileTests`. The command lines above were checked against TimeWarp.Amuru.Tools 3.0.0-beta.10 instead. Suspected bug: `WithArguments` might drop the `--` separator the old transform command relied on. Targeted command: `ilspycmd -t TimeWarp.Amuru.DotNetRunBuilder` on `~/.nuget/packages/timewarp.amuru.tools/3.0.0-beta.10/lib/net11.0/timewarp-amuru-tools.dll`. The builder adds `--` before program arguments. The same package's `GhPrCreateBuilder` and `GhPrMergeBuilder` were decompiled for the `gh` flag sets. No product gate was re-run.

## Issues

No issues.
