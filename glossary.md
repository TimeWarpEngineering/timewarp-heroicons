# Glossary

Shared meanings for words this repository uses.

When an agent or reviewer has to ask what a term or acronym means, or sees one used two ways, the definition goes into `glossary.md` in the same PR.

## Terms

| Term | Meaning |
|------|---------|
| sync PR | The pull request `dev update-icons --pr` opens from branch `sync/heroicons-vX.Y.Z` when upstream heroicons has a newer release. It carries the regenerated icons, the new `<Version>`, and a `releases.md` section. |
| release pending | There is nothing newer upstream to sync (the upstream pin equals the latest heroicons release, including a TW-only revision such as `2.2.0.1`, `2.2.0.1-beta.1`, or the old `2.2.0-tw.1` form, or the pin is ahead of it), but the NuGet identity of `<Version>` in `source/Directory.Build.props` is not on nuget.org yet. `dev update-icons` exits non-zero until a GitHub release publishes it. |
| upstream pin | The heroicons version a package version tracks: its first three numeric parts, ignoring any fourth part, `-prerelease` label, and non-numeric `+metadata` (`2.2.0.1-beta.1`, `2.2.0.1`, and `2.2.0-tw.1` all pin `2.2.0`; `2.0.12+build-1` pins `2.0.12`). A numeric `+X.Y.Z` metadata still names the pin (`2.0.19+2.0.18` pins `2.0.18`). |
| keepalive | The `keepalive` job in `.github/workflows/sync-icons.yml`. Each run re-enables that workflow through the REST API so GitHub does not disable the monthly schedule after 60 days of repository inactivity. It makes no commits. |
| TW-only fix | A package change with no new heroicons release, shipped as a fourth version part `X.Y.Z.R` on the pinned base (`2.2.0.1`, `2.2.0.2`), with prereleases `X.Y.Z.R-beta.N` (`2.2.0.1-beta.1`). This replaces the old `-tw.N` form (`2.2.0-tw.1`), which NuGet sorts before `2.2.0`. |

## Acronyms

| Acronym | Expansion |
|---------|-----------|
| OIDC | OpenID Connect; the release job exchanges a GitHub token for a short-lived nuget.org API key (trusted publishing). |
