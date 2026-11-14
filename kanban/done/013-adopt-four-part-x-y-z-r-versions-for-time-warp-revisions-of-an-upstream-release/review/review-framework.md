# Review framework — task 013

**Date:** 2026-11-14
**Host task:** kanban/in-progress/013-adopt-four-part-x-y-z-r-versions-for-time-warp-revisions-of-an-upstream-release/
**Diff scope:** branch `task/013-adopt-four-part-x-y-z-r-versions-for-time-warp` vs merge-base `35d882ca` (5 commits, 14 files, 471 lines). `master` is 1 commit ahead (`615a414` land kanban 013), outside this diff. No open PR was read (context reported offline).
**Plan / brief:** Ship TimeWarp-only revisions as `X.Y.Z.R` (`2.2.0.1-beta.1`), and make `update-icons` treat the first three numeric parts as the heroicons pin. See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** review session 01a1d95e-3bd5-79a3-a24e-4a43849c3058 (Grok 4.7, 2026-11-14). Implementer session 134f3dc8-8da8-4400-9d28-e8984634031a. Round 2 review session 01a1d967-63a2-7493-9390-ecfaef66ecff (Grok 4.7, 2026-11-14). Round 2 re-verify session 01a1d9ba-b316-7653-a89a-ca88822f31f1 (Grok 4.7, 2026-11-14).

## Round 2 scope

Re-review of the fix delta `b42cc49..a8fee15` (commit `a8fee15`, docs only: `glossary.md` and `readme.md`, plus kitchen). Round 1 files stay frozen. Effort 1, roster general. Bound turn cap 80; this session recorded the cap as uncapped (no `--max-turns` flag).

## Budget (by-diff)

- Lines changed: 115
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (bound review budget; this session has no `--max-turns` flag, so the session line records the cap as uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/` and the kitchen `task.md` sections this oracle is allowed to touch
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
