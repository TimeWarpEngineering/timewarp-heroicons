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

## Checklist

- [ ] Authenticated + graceful upstream release lookup
- [ ] Sync opens PR instead of pushing to master; no duplicate PRs
- [ ] Remove NuGet login/publish from sync job
- [ ] Idempotency / not-published detection
- [ ] Release tag vs props version guard
- [ ] Fix `DetermineMode` workflow_dispatch mapping
- [ ] Bump actions versions
- [ ] Remove legacy `scripts/*.ps1`
- [ ] Update readme (sync → PR → release flow)
- [ ] Build passes; local `dev update-icons` (no push) reports up to date

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- Current state: NuGet versions 1.0.0, 2.0.12-update.1, 2.0.12, 2.0.18, 2.0.19, 2.2.0;
  heroicons latest v2.2.0 (2024-11-18). Version policy in update-icons-command.cs Design
  region (NuGet == heroicons on sync; `-tw.N` for TW-only fixes) stays as is.
- Stale origin branches `Cramer/2026-08-12/heroicons-version-match` and
  `Cramer/2026-08-12/sync-heroicons-2.2.0` can be deleted (optional, not code).
