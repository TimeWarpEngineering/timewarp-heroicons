[![Dotnet](https://img.shields.io/badge/dotnet-11.0-blue)](https://dotnet.microsoft.com)
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

### Supported frameworks

The package targets `net11.0` only and depends on Microsoft.AspNetCore.Components.Web 11.0.x.

.NET 10 (`net10.0`) projects should stay on version `2.2.0`, the last release that targets .NET 10.

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

**1. Sync opens a PR.** The `sync-icons` job in `.github/workflows/sync-icons.yml` runs
`dev update-icons --pr` on the 1st of each month at 06:00 UTC. When upstream has a newer release it regenerates the icon components on a
`sync/heroicons-vX.Y.Z` branch, sets `<Version>` in `source/Directory.Build.props`, adds a
`releases.md` section, builds and tests, then opens a PR into `master`. It never pushes to `master`
and never publishes. If that sync PR is already open, a later run reports it and opens no duplicate.
You can also run it by hand: **Actions → Sync icons → Run workflow**, or
`gh workflow run sync-icons.yml`.

**The schedule keeps itself alive.** GitHub disables a scheduled workflow after 60 days with no
repository activity, and heroicons releases are often further apart than that. Each run of
`sync-icons.yml` has a `keepalive` job that runs `dev workflow-keepalive --workflow sync-icons.yml`.
It re-enables the workflow through the REST API
(`PUT /repos/{owner}/{repo}/actions/workflows/sync-icons.yml/enable`, `actions: write`), which
resets that timer without committing to `master`. The keepalive is a separate job, so it still runs
when the sync fails. `workflow.yml` (push/PR CI and the release job) has no schedule, so inactivity
never disables it. If `gh workflow list --all` ever shows `Sync icons` as `disabled_inactivity`,
re-enable it once with `gh workflow enable sync-icons.yml`.

A failed sync (lookup error, release pending, failed `gh pr create`) fails the run, so GitHub emails
the failed scheduled run to the repository owner.

**One-time setup: the `HEROICONS_SYNC_TOKEN` secret.** The sync job opens its PR with `gh pr create`.
Give it a token that may do that:

1. Create a fine-grained personal access token (**Settings → Developer settings → Fine-grained
   tokens**) with **Repository access: Only select repositories → timewarp-heroicons** and the
   repository permissions **Contents: Read and write** and **Pull requests: Read and write**.
2. Store it as a repository secret: `gh secret set HEROICONS_SYNC_TOKEN` (or **Settings → Secrets
   and variables → Actions → New repository secret**).

A PR opened with this token also triggers the normal CI run. Without it, the job falls back to
`GITHUB_TOKEN`. That works only when **Settings → Actions → General → Allow GitHub Actions to
create and approve pull requests** is enabled (an organization policy may also have to allow it),
and a PR opened with `GITHUB_TOKEN` triggers no CI. Each `--pr` run under Actions without the
secret logs a warning. When `gh pr create` is refused for permissions, the run fails and the error
names both fixes. The PAT expires, so renew the secret before its expiry date.

The keepalive job needs no secret: `dev workflow-keepalive` uses the job's `GITHUB_TOKEN` with
`actions: write` (plus `contents: read` for the checkout of the dev-cli).

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
not on nuget.org, `dev update-icons` prints `Release pending` and exits non-zero, so the monthly run
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