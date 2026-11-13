# Round 1 — general
**Date:** 2026-11-13
**Scope reviewed:** branch `task/008-monthly-icon-sync-that-keeps-its-own-workflow-from` vs `master` (merge-base `919cc2e`), product commit `dff8bf6`

## Summary

The sync moves to `.github/workflows/sync-icons.yml` on `0 6 1 * *` plus `workflow_dispatch`. A `keepalive` job with only `actions: write` calls `PUT /repos/{owner}/{repo}/actions/workflows/sync-icons.yml/enable` and does not `needs` the sync job, so a failing sync still re-enables the workflow. `workflow.yml` loses its schedule, the `sync_icons` input, and the sync job. The `ci` job, `mode: merge|probe`, the `release` trigger, and the nuget.org OIDC login stay in `workflow.yml`. The readme describes the monthly cadence, the dispatch path, the keepalive, and the failure email. `glossary.md` defines **keepalive**.

That matches the task and the restatement. The task asked for a monthly cron, the enable API with `actions: write` and no commits on `master`, a recorded split decision, a non-zero failure, and the readme/glossary updates. The restatement turns the preferred API path and the chosen split into acceptance criteria. Notes record why the schedule left `workflow.yml` (inactivity was disabling PR checks, and trusted publishing stays bound to that file) and why `gh workflow enable` is the wrong call on an already-active workflow. The Notes also say GitHub's docs do not prove the timer reset and keep the no-commit fallback if `Sync icons` later shows `disabled_inactivity`. The task requires that fallback only once the API is shown not to reset the timer. No misread.

The executed keepalive line uses pwsh `$env:GITHUB_REPOSITORY`. Both the keepalive step and the moved sync step end with `exit $LASTEXITCODE`. `dev update-icons` still sets `Environment.ExitCode = 1` for lookup failure, release pending, regenerate failure, and a failed `gh pr create`. The only `schedule:` in the repo is the monthly cron. Removing the `ci` job `if:` does not change push, pull_request, release, or probe: that condition only skipped schedule runs and `sync_icons` dispatches, both gone from this file. Probe still skips the pipeline step.

Recorded `## Results` cover the delta: `actionlint` reported 0 errors in the two workflow files, and `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` are recorded green with the same two pre-existing advisory warnings named in task 007. Those gates do not execute the workflow. The post-merge `workflow_dispatch` check is still open on the checklist, which is the right place for it: a new workflow file is not on the default branch until merge. The smoke helper `grep schedule` also hits the comment in `workflow.yml` that says this file has no schedule. `sync_icons` and `sync-icons:` do not match. The Expect sentence (no schedule trigger and no sync job) holds. Not a finding.

## Issues

None.
