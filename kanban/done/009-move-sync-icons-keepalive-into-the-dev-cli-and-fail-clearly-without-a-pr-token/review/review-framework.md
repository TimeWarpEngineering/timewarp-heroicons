# Review framework — task 009

**Date:** 2026-11-13
**Host task:** kanban/in-progress/009-move-sync-icons-keepalive-into-the-dev-cli-and-fail-clearly-without-a-pr-token/
**Diff scope:** branch `task/009-move-sync-icons-keepalive-into-the-dev-cli-and` vs `master` (merge-base `bcbd091`); product commit `1f30b98`. Files: `.github/workflows/sync-icons.yml`, `readme.md`, `tools/dev-cli/endpoints/update-icons-command.cs`, `tools/dev-cli/endpoints/update-icons-pull-request.cs`, `tools/dev-cli/endpoints/workflow-keepalive-command.cs`.
**Plan / brief:** Move the sync-icons keepalive `gh api` call into `dev workflow-keepalive`, and make `dev update-icons --pr` fail with both owner fixes when PR creation is refused, plus a non-failing warning when Actions has no `HEROICONS_SYNC_TOKEN`. See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 2 (general only; no specialist axes)
**Reviewer roster:** general
**Session IDs:** reviewer grok (grok-4.7), body tw-implementation-review, print mode. Session id not reported. Turn cap 120; cursor session uncapped (no `--max-turns` flag).

## Budget (by-diff)

- Lines changed: 434
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Build, test, and audit stay with the implementer's recorded `## Results` and `### How to validate`. Do not re-run those gates.
