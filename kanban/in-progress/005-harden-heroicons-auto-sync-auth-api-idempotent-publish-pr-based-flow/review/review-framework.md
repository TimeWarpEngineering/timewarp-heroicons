# Review framework — task 005

**Date:** 2026-11-13
**Host task:** kanban/in-progress/005-harden-heroicons-auto-sync-auth-api-idempotent-publish-pr-based-flow/
**Diff scope:** branch `task/005-harden-heroicons-auto-sync-auth-api-idempotent` vs `master` (4 commits, 1731 lines). No open PR was read (context reported offline).
**Plan / brief:** Weekly heroicons sync authenticates the GitHub release lookup, opens a `sync/heroicons-vX.Y.Z` PR instead of pushing to `master` or publishing, fails closed when the props version is not on NuGet, and the release workflow rejects a tag that does not match props `<Version>`.
**Effort:** 3 (general only)
**Reviewer roster:** general
**Session IDs:** review harness did not supply a vendor session id (Grok 4.7, 2026-11-13). Implementer session 26e3005a-bcfc-478a-80a6-40bef2cc8043.

## Budget (by-diff)

- Lines changed: 1731
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (review budget). Harness did not pass `--max-turns`; session bound recorded as uncapped.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/` and the kitchen `task.md` sections this oracle is allowed to touch
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Build, test, and audit stay on the implementer's recorded Results. One targeted command may check one suspected bug.
