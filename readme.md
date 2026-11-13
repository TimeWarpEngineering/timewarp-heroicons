[![Dotnet](https://img.shields.io/badge/dotnet-6.0-blue)](https://dotnet.microsoft.com)
[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://github.com/TimeWarpEngineering/timewarp-heroicons)
[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![nuget](https://img.shields.io/nuget/v/timewarp-heroicons?logo=nuget)](https://www.nuget.org/packages/timewarp-heroicons/)
[![nuget](https://img.shields.io/nuget/dt/timewarp-heroicons?logo=nuget)](https://www.nuget.org/packages/timewarp-heroicons/)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://github.com/TimeWarpEngineering/timewarp-heroicons/issues)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/timewarp-heroicons)](https://github.com/TimeWarpEngineering/timewarp-heroicons)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://unlicense.org)
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Ftimewarp-heroicons)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-heroicons)

[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)


# timewarp-heroicons
All the HeroIcons wrapped as Blazor components.

![TimeWarp Logo](assets/brand-logo.png)

All [heroicons](https://github.com/tailwindlabs/heroicons) wrapped as Blazor components.
See and search all at https://heroicons.com/

## Give a Star! :star:

If you like or are using this project please give it a star. Thank you!

## Usage

```razor
<TimeWarp.HeroIcons.Mini.Solid.AcademicCapIcon class="w-10 h-10" />
<TimeWarp.HeroIcons.Solid.AcademicCapIcon class="w-20 h-20" />
<TimeWarp.HeroIcons.Outline.AcademicCapIcon class="w-20 h-20" />
```

Outputs

![](assets/sample-output.png)  

## Installation

You can see the latest NuGet packages from the official [TimeWarp NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [timewarp-heroicons](https://www.nuget.org/packages/timewarp-heroicons/) [![nuget](https://img.shields.io/nuget/v/timewarp-heroicons?logo=nuget)](https://www.nuget.org/packages/timewarp-heroicons/)

```console
dotnet add package timewarp-heroicons
```

## License

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://unlicense.org)

## Contributing

Time is of the essence.  Before developing a Pull Request I recommend opening a [discussion](https://github.com/TimeWarpEngineering/timewarp-heroicons/discussions).

Please feel free to make suggestions and help out with the [documentation](https://timewarpengineering.github.io/timewarp-heroicons/).
Please refer to [Markdown](http://daringfireball.net/projects/markdown/) for how to write markdown files.

### Syncing icons and releasing

Package versions match [heroicons](https://github.com/tailwindlabs/heroicons) releases
(`2.2.0` = heroicons 2.2.0). TW-only fixes use a prerelease on that base (`2.2.0-tw.1`).
See `releases.md`.

**1. Sync opens a PR.** The weekly `sync-icons` job in `.github/workflows/workflow.yml` runs
`dev update-icons --pr`. When upstream has a newer release it regenerates the icon components on a
`sync/heroicons-vX.Y.Z` branch, sets `<Version>` in `source/Directory.Build.props`, adds a
`releases.md` section, builds and tests, then opens a PR into `master`. It never pushes to `master`
and never publishes. If that sync PR is already open, a later run reports it and opens no duplicate.
You can also run the job by hand: **Actions → CI/CD → Run workflow** with `sync_icons` checked.

One-time setup: `gh pr create` in the job needs either
**Settings → Actions → General → Allow GitHub Actions to create and approve pull requests**, or a
`HEROICONS_SYNC_TOKEN` repository secret (fine-grained PAT with contents + pull requests write).
With the PAT the sync PR also triggers the normal CI run.

**2. Review and merge the PR** like any other change.

**3. Publish with a GitHub release.** Create a release on `master` whose tag is `v` + the props
`<Version>` (for example `v2.2.0`). From a clean, up-to-date `master` checkout, `dev release` does
this from `source/Directory.Build.props` (`dev release --dry-run` shows what it would create). By hand:

```console
gh release create v2.2.0 --target master --title v2.2.0 --notes "Update to heroicons version 2.2.0"
```

The `release` event runs `dev workflow` in release mode. It fails if the tag (minus `v`) does not
match `<Version>`, then builds, packs, and pushes to nuget.org with OIDC trusted publishing.

**Pending releases are not dropped.** When `<Version>` already matches upstream but that version is
not on nuget.org, `dev update-icons` prints `Release pending` and exits non-zero, so the weekly run
stays red until step 3 is done.

Locally, `dev update-icons` (no `--pr`) checks upstream and nuget.org. When upstream is newer it
regenerates into the working tree without committing. The lookup sends `GITHUB_TOKEN` or `GH_TOKEN`
when set, which avoids the anonymous GitHub API rate limit.

## Contact

Sometimes the github notifications get lost in the shuffle.  If you file an [issue](https://github.com/TimeWarpEngineering/timewarp-heroicons/issues) and don't get a response in a timely manner feel free to ping on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)

## References

https://github.com/heroicons/heroicons

### Commands used

```PowerShell
dotnet new sln
dotnet new razorclasslib -n timewarp-heroicons
dotnet sln add .\Source\timewarp-heroicons\timewarp-heroicons.csproj
dotnet new tool-manifest
dotnet tool install dotnet-cleanup
dotnet cleanup -y
```