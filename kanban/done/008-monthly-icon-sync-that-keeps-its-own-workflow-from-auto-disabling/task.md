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

## Goal (implementer restatement)

Stop the heroicons sync from silently dying when GitHub auto-disables a scheduled workflow after
60 days without repository activity, without adding commits to `master`.

Acceptance:

- The sync runs monthly (`0 6 1 * *`), not weekly.
- Each scheduled (and manual) run re-enables its own workflow through the REST API
  (`PUT /repos/{owner}/{repo}/actions/workflows/{id}/enable`) with only `actions: write`. The
  keepalive does not depend on the sync step succeeding.
- The sync lives in its own `sync-icons.yml`; `workflow.yml` keeps push/PR/release/probe CI with
  no schedule (so trusted publishing, bound to `workflow.yml`, is untouched). Reason recorded.
- A failed sync makes the run fail (non-zero), so the owner gets GitHub's failed-run email.
- readme sync section describes the monthly cadence, the keepalive, and the new dispatch path;
  `glossary.md` gains any new term.
- Workflow YAML passes `actionlint`; build, test, and audit gates are green.

## Checklist

- [x] Monthly cron for the sync
- [x] Self re-enable (keepalive) step with least-privilege permissions
- [x] Decide: split sync into its own workflow file or not (record why)
- [x] Failure path exits non-zero
- [x] readme / glossary updated
- [x] `actionlint` clean
- [ ] Manual `workflow_dispatch` run of the sync succeeds after merge (post-merge; see How to validate)

## Depends on

- 007

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)
- Implement (2026-11-13): merged `origin/master` (kanban land commit), split the sync into
  `.github/workflows/sync-icons.yml` with a `keepalive` job, removed schedule/sync job/`sync_icons`
  input from `workflow.yml`, updated readme and glossary.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 6ff77496-da4d-4a2c-81ea-91b8bb740b0a, max-turns 500, Done — 2026-11-13T12:09:08Z
- Review (2026-11-13): general, effort 2, round 1, disposition clean. Restatement matches the task; no misread. Turn cap 120; this cursor session was uncapped.
- Review oracle: reviewed by reviewer-cursor (cursor, model grok-4.7), session d9e423d0-ddff-4630-a684-08cfa8effc99, ganda session 533312, max-turns uncapped (review budget turn cap 120), effort 2, roster general, round 1, disposition clean — 2026-11-13
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts general.md, merged.md, disposition.md — 2026-11-13T12:13:55Z
- Installed ganda changed during this walk: was 53755096 bytes 8cadae8362001777109f71430d59f6c372c26e23134092fc635b921b629f17bf; now 53775992 bytes 7eaa25de594befcc8c4a0c33da7386f44ba62c4be844156d6bcaeaa3210e499b

## Review

- Round 1: Done

## Notes

- 007 (in progress at filing) also edits `workflow.yml` (`setup-dotnet` → 11.0.x); hence the
  dependency. Rebase on master after 007 merges.
- Release history checked 2026-11-13 via `gh api repos/tailwindlabs/heroicons/releases`.
- Split decision (2026-11-13): the sync moved to its own `.github/workflows/sync-icons.yml`.
  GitHub only auto-disables workflows that have a `schedule` trigger. With the schedule out of
  `workflow.yml`, the push/PR CI and the release job (the file nuget.org trusted publishing is
  bound to) can never be disabled for inactivity again. That is what blocked checks on PRs #22/#23.
  It also keeps `actions: write` and the schedule off the CI/release workflow, and the keepalive
  only has to re-enable one small file. `workflow.yml` is still the release workflow file, so the
  trusted-publishing notes are unchanged.
- Keepalive approach (2026-11-13): a `keepalive` job calls
  `gh api --method PUT repos/$GITHUB_REPOSITORY/actions/workflows/sync-icons.yml/enable` with
  job permissions `actions: write` only (workflow-level `permissions: {}`). It is a separate job
  with no `needs`, so it runs even when the sync job fails (for example "release pending", which
  stays red every month until a release ships). Evidence it works without commits: the
  liskin/gh-workflow-keepalive action uses this endpoint ("uses GitHub API to preemptively re-enable
  the workflow, thus preventing it from being automatically disabled", "will not create any dummy
  commits") with `actions: write`. GitHub's own docs do not state the timer reset, and it cannot be
  shown here without waiting 60+ days, so it is not yet proven in this repo. If `Sync icons` ever
  shows `disabled_inactivity`, the readme says how to re-enable it, and the fallback is a separate
  keepalive workflow (still no commits on master).
- `gh workflow enable` is not used: cli/cli `pkg/cmd/workflow/enable/enable.go` resolves only
  workflows in `DisabledManually`/`DisabledInactivity` states, so on an active workflow it fails
  with "there are no disabled workflows to enable". The raw endpoint returns 204 for an active one.
- Monthly cron `0 6 1 * *`: the longest gap is 31 days, under the 60-day limit.

## Results

- `.github/workflows/sync-icons.yml` (new): `schedule: '0 6 1 * *'` + `workflow_dispatch`;
  `keepalive` job (`actions: write`, REST `PUT .../enable`, `exit $LASTEXITCODE`); `sync-icons`
  job (moved unchanged from `workflow.yml`: `contents: write`, `pull-requests: write`,
  `update-icons --pr`, `exit $LASTEXITCODE`).
- `.github/workflows/workflow.yml`: removed `schedule`, the `sync_icons` dispatch input, the
  `sync-icons` job, and the `ci` job's `if:` (it only existed to skip schedule/sync runs). The
  `ci` job, `mode: merge|probe`, and the release/OIDC path are unchanged.
- Failure path: `dev update-icons` already sets `Environment.ExitCode = 1` on lookup, release
  pending, regenerate, and `gh pr create` failures (task 005); both steps end in
  `exit $LASTEXITCODE`, so pwsh does not swallow the native exit code.
- `readme.md`: sync section now says monthly, names `sync-icons.yml`, the new dispatch path, the
  keepalive, and failure email. `glossary.md`: added **keepalive**.

### Gates

- `docker run --rm -v "$PWD:/repo" --workdir /repo rhysd/actionlint:latest -no-color -verbose` →
  `Found 0 errors in 2 files` (exit 0).
- `./bin/dev build` → `Build completed successfully!`, 0 errors (exit 0).
- `./bin/dev test` → `Tests completed successfully!` (exit 0).
- `ganda repo audit` → exit 0, Passed 42 / Failed 2 / Skipped 2. Both failures are
  Warning severity (advisory, non-blocking) and pre-existing, as recorded in task 007:
  `ci-code-coverage` and `vscode-window-icon` (`.vscode/settings.json` is git-ignored, local only).
  Neither is touched by this change.

### How to validate

**Smoke:**

```bash
grep -n "cron\|actions: write\|enable\|LASTEXITCODE" .github/workflows/sync-icons.yml
grep -n "schedule\|sync_icons\|sync-icons:" .github/workflows/workflow.yml   # expect: no matches
# after merge:
gh workflow run sync-icons.yml && gh run watch "$(gh run list --workflow sync-icons.yml -L1 --json databaseId -q '.[0].databaseId')"
gh workflow list --all
```

**Expect:** `sync-icons.yml` shows `'0 6 1 * *'`, `actions: write`, the `PUT .../enable` call, and
two `exit $LASTEXITCODE` lines; `workflow.yml` has no schedule or sync job. After merge the manual
run shows `keepalive` green and `sync-icons` either green ("up to date"/sync PR opened) or red
only for a real sync failure such as "Release pending". `gh workflow list --all` lists
`Sync icons` and `CI/CD` as `active`.

### Review

- Round 1. Roster: general. Effort: 2. Turn cap 120; this cursor session was uncapped.
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0 open / 0 fixed / 0 wontfix; nit 0 open / 0 fixed / 0 wontfix.
- Disposition: clean. No wontfix and no escalation. The implementer restatement matches the original task; no misread.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
