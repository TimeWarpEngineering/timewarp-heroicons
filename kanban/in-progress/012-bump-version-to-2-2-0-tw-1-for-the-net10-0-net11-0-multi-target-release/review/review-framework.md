# Review framework — task 012

**Date:** 2026-11-13
**Host task:** kanban/in-progress/012-bump-version-to-2-2-0-tw-1-for-the-net10-0-net11-0-multi-target-release/
**Diff scope:** branch `task/012-bump-version-to-2-2-0-tw-1-for-the-net10-0-net11-0` vs merge-base `320b1ec` (`master`). Product paths: `Directory.Packages.props`, `source/timewarp-heroicons/timewarp-heroicons.csproj`, `tools/transform/transform.csproj`, `source/Directory.Build.props`, `readme.md`, `releases.md`.
**Plan / brief:** Drop `net10.0`, ship `2.2.0-tw.1` as a net11.0-only package, and document the break for .NET 10 consumers (stay on 2.2.0). See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review cursor session not reported (print mode, 2026-11-13); implementer session `e4799561-1150-46f2-97a1-358f3c48d2fb`

## Budget (by-diff)

- Lines changed: 183
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
- Build, test, and audit stay on the implementer's recorded Results. This review did not re-run those gates.
