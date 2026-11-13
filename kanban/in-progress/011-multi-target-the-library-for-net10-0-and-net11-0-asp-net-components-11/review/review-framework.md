# Review framework — task 011

**Date:** 2026-11-13
**Host task:** kanban/in-progress/011-multi-target-the-library-for-net10-0-and-net11-0-asp-net-components-11/
**Diff scope:** branch `task/011-multi-target-the-library-for-net10-0-and-net11-0` vs merge-base `e334fda` (`land kanban 010`). Product diff is `Directory.Packages.props`, `source/timewarp-heroicons/timewarp-heroicons.csproj`, `tests/sample-app/sample-app.csproj`, and `readme.md`.
**Plan / brief:** Multi-target `timewarp-heroicons` for `net10.0` and `net11.0`, pin `Microsoft.AspNetCore.Components.Web` per TFM, move `tests/sample-app` to net11.0, and leave `<Version>` and `releases.md` unchanged. See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** implementer session `771e57b7-bfcb-4449-9142-e5b1702fe97c`; review cursor session not reported (2026-11-13)

## Budget (by-diff)

- Lines changed: 214
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
- Build, test, and audit stay on the implementer's recorded Results. This review did not re-run those gates.
