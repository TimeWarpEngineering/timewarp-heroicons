# Review framework — task 007

**Date:** 2026-11-13
**Host task:** kanban/in-progress/007-move-dev-tooling-to-the-net-11-sdk-and-lift-held-package-pins/
**Diff scope:** branch `task/007-move-dev-tooling-to-the-net-11-sdk-and-lift-held` vs `master` (merge-base `ad4e648`); product commit `d18a419`. `global.json`, `.github/workflows/workflow.yml`, `Directory.Packages.props`, `.editorconfig`, `tools/dev-cli/**`.
**Plan / brief:** Move dev tooling to the .NET 11 SDK, lift the `package-pin-currency.hold` pins, adapt the dev-cli to Amuru 3 / Nuru 4, and keep `source/timewarp-heroicons` on `net10.0`. See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 2 (general only; no specialist axes)
**Reviewer roster:** general
**Session IDs:** review conversation `15a16174-f22d-4fb7-b948-af005341e897` (cursor, grok-4.7); ganda session `508884`. Implementer session `c85193cb-59d5-43cb-8f71-4af128f3cfb7`.

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
- Build, test, and audit stay with the implementer's recorded `## Results` and `### How to validate`. Do not re-run those gates.
