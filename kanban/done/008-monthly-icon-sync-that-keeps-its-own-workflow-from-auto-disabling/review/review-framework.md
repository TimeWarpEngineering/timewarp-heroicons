# Review framework — task 008

**Date:** 2026-11-13
**Host task:** kanban/in-progress/008-monthly-icon-sync-that-keeps-its-own-workflow-from-auto-disabling/
**Diff scope:** branch `task/008-monthly-icon-sync-that-keeps-its-own-workflow-from` vs `master` (merge-base `919cc2e`); product commit `dff8bf6`. Files: `.github/workflows/sync-icons.yml`, `.github/workflows/workflow.yml`, `readme.md`, `glossary.md`.
**Plan / brief:** Move the heroicons sync to a monthly schedule in its own workflow and re-enable that workflow through the REST API so GitHub inactivity cannot disable it, without commits on `master`. See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 2 (general only; no specialist axes)
**Reviewer roster:** general
**Session IDs:** review conversation `d9e423d0-ddff-4630-a684-08cfa8effc99` (cursor, grok-4.7); ganda session `533312`. Implementer session `6ff77496-da4d-4a2c-81ea-91b8bb740b0a`.

## Budget (by-diff)

- Lines changed: 236
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
