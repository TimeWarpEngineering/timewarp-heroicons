# Harden heroicons auto-sync: auth API, idempotent publish, PR-based flow

## Description

The weekly `sync-icons` job (`.github/workflows/workflow.yml` →
`dev update-icons --push --publish`) is meant to keep the NuGet package in step with
upstream tailwindlabs/heroicons releases. Today it is in sync (NuGet 2.2.0 == heroicons
v2.2.0), but the automation is fragile and bypasses the normal ganda/PR/release flow, so the
next real upstream release is likely to fail or fail silently.

Problems found in review (2026-11-13):

1. **Unauthenticated GitHub API call.** `FetchLatestHeroiconsVersionAsync`
   (`tools/dev-cli/endpoints/update-icons-command.cs`) calls
   `api.github.com/repos/tailwindlabs/heroicons/releases/latest` with no token. Run
   41309057536 (2026-11-01) failed with `403 (rate limit exceeded)` and an unhandled
   exception (exit 134).
2. **Non-idempotent publish.** Order is regenerate → commit+push to master → pack+push NuGet.
   If publish fails after the git push, props already say the new version, so the next run
   reports "up to date" and the version is never published.
3. **Bypasses the normal flow.** Bot commits directly to `master` (works only because master
   is unprotected), publishes from the sync job with no test gate, and creates no `vX.Y.Z`
   tag / GitHub release — unlike 2.2.0, which went PR → GitHub release → `release` event.
4. NuGet OIDC login runs every week in the sync job even when nothing changed.
5. Smaller: `WorkflowCommand.DetermineMode` maps `workflow_dispatch` → Release; release path
   does not verify the release tag matches props `<Version>` (`--skip-duplicate` masks it);
   `actions/checkout@v4` / `setup-dotnet@v4` Node 20 deprecation warnings; legacy
   `scripts/*.ps1` (update/transform/publish) duplicate the dev-cli.

## Requirements

- Upstream version lookup sends `GITHUB_TOKEN` (env) when present; failures are a clean
  error message + non-zero exit, not an unhandled exception.
- Sync no longer pushes to `master`. When upstream is newer it creates a branch, commits the
  regenerated icons + version + releases.md, and opens a PR (e.g. `gh pr create`, with
  auto-merge enabled if allowed). The sync job no longer needs NuGet login or publish.
- Publishing happens only through the normal release path (tag/GitHub release → `release`
  event → `dev workflow`). Document in readme how a synced PR becomes a release (or, if
  creating the release automatically after merge is chosen, do it via the standard path, not
  from the sync job).
- Idempotency: sync detects "props version already newer than / equal to upstream but not on
  NuGet" or an already-open sync PR, and does not silently drop it (report instead of
  "up to date"). A second run while a sync PR is open must not open a duplicate PR.
- Release pipeline fails if the release tag (minus `v`) != props `<Version>`.
- `workflow_dispatch` no longer auto-maps to Release in `DetermineMode`.
- Bump GitHub actions to Node-24-compatible majors.
- Remove `scripts/*.ps1` if nothing references them.
- Workflow permissions: sync job gets `contents: write` + `pull-requests: write` only.

## Goal (implementer restatement)

Make the weekly heroicons sync safe and boring: it must never push to `master` or publish
to NuGet itself, and it must never silently lose a pending version.

Acceptance criteria:

- `dev update-icons` sends `GITHUB_TOKEN` (or `GH_TOKEN`) as a bearer token to the GitHub
  releases API when set. HTTP/network/JSON failures print one clean error and exit non-zero
  (no unhandled exception / exit 134).
- `dev update-icons --pr` (used by the sync job) regenerates on a `sync/heroicons-vX.Y.Z`
  branch, commits icons + props `<Version>` + `releases.md`, pushes that branch, and opens a
  PR against `master` with `gh pr create`. If an open PR for that branch already exists it
  reports it and does nothing else (no duplicate PR). Auto-merge is opt-in (`--auto-merge`).
- `--push`, `--publish`, and `--api-key` are gone; the sync job has no NuGet login and only
  `contents: write` + `pull-requests: write`.
- When props already match upstream, the command checks nuget.org: published → "up to date";
  not published → a clear "release pending" message with the steps, and a non-zero exit so the
  scheduled run is visibly red. Props ahead of upstream is also reported, not "up to date".
- `dev workflow` release mode fails unless the release tag (minus `v`) equals props
  `<Version>` (or its NuGet identity without `+metadata`).
- `DetermineMode` maps `workflow_dispatch` to Merge, not Release.
- GitHub actions bumped to Node 24 majors (checkout@v7, setup-dotnet@v6, upload-artifact@v7).
- `scripts/*.ps1` removed (only the `.sln` solution-items folder and the readme referenced them).
- Readme documents sync → PR → merge → GitHub release `vX.Y.Z` → `release` event → publish.
- Build passes; local `dev update-icons` (no `--pr`) reports up to date against 2.2.0.

## Checklist

- [x] Authenticated + graceful upstream release lookup
- [x] Sync opens PR instead of pushing to master; no duplicate PRs
- [x] Remove NuGet login/publish from sync job
- [x] Idempotency / not-published detection
- [x] Release tag vs props version guard
- [x] Fix `DetermineMode` workflow_dispatch mapping
- [x] Bump actions versions
- [x] Remove legacy `scripts/*.ps1`
- [x] Update readme (sync → PR → release flow)
- [x] Build passes; local `dev update-icons` (no push) reports up to date

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)
- Implement (ganda task work, Claude Opus 5.5, 2026-11-13): sync → PR flow, lookups, tag guard,
  actions bump, scripts removal, readme. Boy-scout audit fixes listed under Results.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 26e3005a-bcfc-478a-80a6-40bef2cc8043, max-turns 500, Done — 2026-11-13T10:49:58Z

## Notes

- Current state: NuGet versions 1.0.0, 2.0.12-update.1, 2.0.12, 2.0.18, 2.0.19, 2.2.0;
  heroicons latest v2.2.0 (2024-11-18). Version policy in update-icons-command.cs Design
  region (NuGet == heroicons on sync; `-tw.N` for TW-only fixes) stays as is.
- Stale origin branches `Cramer/2026-08-12/heroicons-version-match` and
  `Cramer/2026-08-12/sync-heroicons-2.2.0` can be deleted (optional, not code).

## Results

### What changed

- `tools/dev-cli/endpoints/update-icons-*.cs` (split into command / lookups / regenerate /
  pull-request partials to stay under the 500-line cap):
  - Upstream lookup sends `GITHUB_TOKEN` (else `GH_TOKEN`) as a bearer token. Non-2xx, network,
    timeout, and bad JSON print `Error: upstream heroicons release lookup failed: …` and exit 1
    (rate limit is named when `x-ratelimit-remaining: 0`).
  - `--push`, `--publish`, `--api-key` removed. New `--pr` (plus `--auto-merge`, `--base`):
    checks `gh pr list --head sync/heroicons-vX.Y.Z --state open` first and stops if a PR is
    open; otherwise `git checkout -B` that branch, regenerates, bumps `<Version>`, adds the
    `releases.md` section (now idempotent), builds and tests, commits, force-pushes the
    bot-owned branch, and runs `gh pr create`. Bot git identity only inside GitHub Actions.
  - Props == upstream → checks nuget.org flat-container. Published → "up to date". Missing →
    `Release pending …` with the release steps, exit 1. Props ahead of upstream → reported.
- `workflow-command.cs`: release mode verifies `--release-tag` (else `GITHUB_REF_NAME`) minus
  `v` equals props `<Version>` or its NuGet identity; missing tag also fails. `workflow_dispatch`
  → Merge. Push uses the NuGet identity for the package file name.
- `.github/workflows/workflow.yml`: sync job permissions `contents: write` +
  `pull-requests: write`, no NuGet login, runs `update-icons --pr --base <default branch>`;
  `GH_TOKEN` is `secrets.HEROICONS_SYNC_TOKEN || secrets.GITHUB_TOKEN`. Release passes the tag.
  checkout@v7, setup-dotnet@v6, upload-artifact@v7 (all `node24`). Steps are pwsh (audit).
- `scripts/*.ps1` deleted; their `Scripts` solution folder removed from `timewarp-heroicons.sln`
  (nothing else referenced them). Readme "Syncing icons and releasing" replaces the old
  PowerShell steps.

### Boy-scout audit fixes

`ganda repo audit --fix`: suppression baseline seeded, gitignore entries (bin/dev, .local/,
.timewarp/signals/, .vscode/settings.json untracked), githooks refreshed (old post-commit hook
failed to compile), `.memsearch.toml` removed, `glossary.md` scaffold (filled with real terms),
TimeWarp.SourceGenerators + TW0007 filename, `pull_request.branches` adds `feature/**`, pwsh
`defaults`. By hand: `charset = utf-8` unquoted, workflow bash steps → pwsh, line-cap split.

`package-pin-currency`: `--fix` bumped to net11-only releases (Amuru 3 beta, Nuru 4 beta,
Build.Tasks 2.0.0, SourceGenerators beta.15), which do not build on the .NET 10 SDK this repo
uses. Pinned the newest net10-capable versions instead (Amuru/Amuru.Tools 2.0.0, Nuru/Nuru.DevCli
3.0.0, Build.Tasks 1.0.2, SourceGenerators 1.0.0-beta.14, BannedApiAnalyzers 5.6.0) and recorded
`package-pin-currency.hold` entries with reasons in `.editorconfig` `[ganda.audit]`. Nuru 3.0.0
moved `Unit`/handlers to TimeWarp.Mediator: `global using static TimeWarp.Mediator.Unit` and
`Handle` returns `Task<Unit>`. Nuru.DevCli 3.0.0 also adds `dev release` / `check-version`.

Remaining audit warning: `ci-code-coverage` (repo has no test projects; `dev test` only builds
the sample app).

### Gates

- `./bin/dev build` → Build completed successfully (0 errors; pre-existing warnings: Scriban
  NU1902/NU1903 advisories in tools/transform, IL2110/IL2111 in sample-app, RS0030 Console).
- `./bin/dev test` → Tests completed successfully (exit 0).
- `ganda repo audit` → exit 0 (first run failed on pre-existing checks + the line cap; fixed as
  above and re-run); only warning left is `ci-code-coverage`.

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev update-icons; echo "exit=$?"
GITHUB_TOKEN=bogus ./bin/dev update-icons; echo "exit=$?"
./bin/dev workflow --mode release --release-tag v9.9.9; echo "exit=$?"
env -u GITHUB_REF_NAME ./bin/dev workflow --mode release; echo "exit=$?"
# release pending: temporarily set <Version>2.2.0-tw.99</Version> in source/Directory.Build.props
./bin/dev update-icons; echo "exit=$?"   # then restore the props file
```

Expect:

- First run: props 2.2.0 / upstream 2.2.0 / `Icons are already up to date (timewarp-heroicons 2.2.0 is on nuget.org).`, exit 0.
- Bogus token: `Error: upstream heroicons release lookup failed: GitHub API returned 401 Unauthorized …`, exit 1, no stack trace (proves the token is sent).
- Tag v9.9.9: `Pipeline FAILED - release tag v9.9.9 does not match … <Version> 2.2.0.`, exit 1.
- No tag: `Pipeline FAILED - release mode needs a release tag …`, exit 1.
- 2.2.0-tw.99: `Release pending: props say 2.2.0-tw.99 but timewarp-heroicons 2.2.0-tw.99 is not on nuget.org.`, exit 1.
- In CI: the next weekly `sync-icons` run logs "up to date"; `--pr` is exercised for real on the
  next heroicons release (or by a manual dispatch with `sync_icons`).

### Follow-ups / risks

- Repo setting "Allow GitHub Actions to create and approve pull requests" is currently **off**
  (`can_approve_pull_request_reviews: false`), so `gh pr create` with the workflow token will
  fail loudly until it is enabled or a `HEROICONS_SYNC_TOKEN` secret is added. Not changed here
  (repo admin setting). Repo `allow_auto_merge` is false, so `--auto-merge` is not passed by the
  workflow.
- A PR opened with `GITHUB_TOKEN` does not trigger the `pull_request` CI job; the sync run builds
  and tests before opening it. A PAT secret makes CI run on the PR.
- net11 migration (lift the `package-pin-currency.hold` entries) needs its own task.
- Optional: delete stale origin branches `Cramer/2026-08-12/heroicons-version-match` and
  `Cramer/2026-08-12/sync-heroicons-2.2.0` (not done).
