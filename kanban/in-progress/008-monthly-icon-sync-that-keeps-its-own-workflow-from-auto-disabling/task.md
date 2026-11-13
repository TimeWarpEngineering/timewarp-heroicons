# Monthly icon sync that keeps its own workflow from auto-disabling

## Description

The `CI/CD` workflow (`.github/workflows/workflow.yml`) carries the scheduled heroicons sync
(`dev update-icons`, which since task 005 opens a sync PR). GitHub disables scheduled workflows
in public repos after 60 days with no repository activity. That happened here:
`gh workflow list --all` showed `CI/CD disabled_inactivity` on 2026-11-13. The weekly sync had
stopped, and PRs #22/#23 got no checks until it was re-enabled by hand.

Upstream releases are rare. tailwindlabs/heroicons: 7 releases in 2023, 5 in 2024, none since
v2.2.0 (2024-11-18). So this repo will normally sit idle for more than 60 days, and the
schedule will be disabled when the next release lands unless the workflow keeps itself alive.

## Requirements

- Change the sync schedule from weekly (`0 6 * * 0`) to **monthly** (e.g. `0 6 1 * *`).
- Keep the workflow from being disabled for inactivity without manual action. Preferred
  approach: the scheduled run re-enables its own workflow through the API
  (`PUT /repos/{owner}/{repo}/actions/workflows/{id}/enable`, `gh workflow enable`) with
  `permissions: actions: write`. A monthly cadence (< 60 days) keeps resetting the timer.
  Do **not** use dummy keepalive commits to master. If the API approach is shown not to reset
  the inactivity timer, record the evidence in Notes and choose an alternative that needs no
  commits on master (for example a separate small keepalive workflow), justified in Notes.
- Consider splitting the scheduled sync into its own workflow file (e.g. `sync-icons.yml`) so the
  keepalive and sync schedule are not tangled with PR/push/release CI. Decide and record why. If
  split, update the trusted-publishing notes only if the release job's workflow file changes
  (it must not: nuget.org trusted publishing is bound to `workflow.yml`).
- Failure visibility: a scheduled sync failure must fail the run (non-zero), so GitHub's failed
  scheduled-run email reaches the repo owner. No silent success on errors.
- Update readme's sync section (cadence + keepalive) and `glossary.md` if a new term appears.

## Checklist

- [ ] Monthly cron for the sync
- [ ] Self re-enable (keepalive) step with least-privilege permissions
- [ ] Decide: split sync into its own workflow file or not (record why)
- [ ] Failure path exits non-zero
- [ ] readme / glossary updated
- [ ] `actionlint` (or equivalent) clean; manual `workflow_dispatch` run of the sync succeeds after merge

## Depends on

- 007

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- 007 (in progress at filing) also edits `workflow.yml` (`setup-dotnet` → 11.0.x); hence the
  dependency. Rebase on master after 007 merges.
- Release history checked 2026-11-13 via `gh api repos/tailwindlabs/heroicons/releases`.
