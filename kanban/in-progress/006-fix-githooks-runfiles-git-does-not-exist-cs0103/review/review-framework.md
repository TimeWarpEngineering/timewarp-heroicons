# Review framework — task 006

**Date:** 2026-11-13
**Host task:** kanban/in-progress/006-fix-githooks-runfiles-git-does-not-exist-cs0103/
**Diff scope:** branch `task/006-fix-githooks-runfiles-git-does-not-exist-cs0103` vs `master` (`4cafa1e`...`9df6c36`), 18 files
**Plan / brief:** Make `.githooks` runfiles compile (`Git.FindRoot` / CS0103) and stay on the layout `ganda repo audit` expects. Recorded implementer results cover build, test, audit, and hook `dotnet run`.
**Effort:** 2 (by-diff, 584 lines)
**Reviewer roster:** general
**Session IDs:** implementer session not reported; review session not reported

## Round 2

**Date:** 2026-11-13
**Diff scope:** `9df6c36`..`78bf3e6` (since `review/round-1/base.sha`). Product delta is one line in `.github/workflows/workflow.yml` (the M1 `$LASTEXITCODE` guard on Sync heroicons).
**Plan / brief:** Re-verify M1 against that delta. Recorded fix-pass results cover build, test, audit, and a `LASTEXITCODE` grep.
**Effort:** 1 (by-diff, 104 lines)
**Reviewer roster:** general
**Session IDs:** review session not reported

## Budget (by-diff)

- Lines changed: 104
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Build, test, and audit stay on the implementer's recorded results. One targeted command may check one suspected bug.
