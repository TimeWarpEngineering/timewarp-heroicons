# Review framework — task 005

**Date:** 2026-11-13
**Host task:** kanban/in-progress/005-harden-heroicons-auto-sync-auth-api-idempotent-publish-pr-based-flow/
**Diff scope:** branch `task/005-harden-heroicons-auto-sync-auth-api-idempotent` vs `master` (4 commits, 1731 lines). No open PR was read (context reported offline).
**Plan / brief:** Weekly heroicons sync authenticates the GitHub release lookup, opens a `sync/heroicons-vX.Y.Z` PR instead of pushing to `master` or publishing, fails closed when the props version is not on NuGet, and the release workflow rejects a tag that does not match props `<Version>`.
**Effort:** 3 (general only)
**Reviewer roster:** general
**Session IDs:** review harness did not supply a vendor session id (Grok 4.7, 2026-11-13). Implementer session 26e3005a-bcfc-478a-80a6-40bef2cc8043.

## Budget (by-diff)

- Lines changed: 152
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/` and the kitchen `task.md` sections this oracle is allowed to touch
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Build, test, and audit stay on the implementer's recorded Results. One targeted command may check one suspected bug.

## Round 2

**Date:** 2026-11-13
**Diff scope:** product delta `fde535bbbba19dd611d902eeba6496ea9de4aaac..35ebe3088a537c109e23dd280f2f98df09285665` (commit `35ebe30`). Files: `.github/workflows/workflow.yml`, `glossary.md`, `tools/dev-cli/endpoints/update-icons-command.cs`, `tools/dev-cli/endpoints/update-icons-lookups.cs`. Re-verify M1–M3. The header Diff scope and Effort above are round 1. The Budget block is this round.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review harness did not supply a vendor session id (Grok 4.7, 2026-11-13). Bound turn cap 80; the harness did not pass `--max-turns`, so the session line records the cap as uncapped.
