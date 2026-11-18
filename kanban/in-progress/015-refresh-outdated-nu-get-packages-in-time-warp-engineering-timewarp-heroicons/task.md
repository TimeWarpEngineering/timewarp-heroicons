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

## Session

- Minted by ganda task wave run 20261118T170809Z-b6e473
