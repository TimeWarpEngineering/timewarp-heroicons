# Review framework — task 016

**Date:** 2026-11-26
**Host task:** kanban/in-progress/016-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-heroicons/
**Diff scope:** branch `task/016-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (merge-base `dc69c956f63ca9a919dfc29f694e95fd16ce3f86`). Product delta: `Directory.Packages.props` (TimeWarp.Nuru and TimeWarp.Nuru.DevCli 4.0.0-beta.16 to 4.0.0-beta.17) and `tools/dev-cli/dev.cs` (`.UseTelemetry`). Kitchen `task.md` is the implementer record.
**Plan / brief:** Refresh the two outdated dev-only Nuru pins. Do not change `<Version>` in `source/Directory.Build.props`. Edit `Directory.Packages.props` in this repo only.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer-claude 61512aa2-26a2-4166-a3c6-bc9069f5bfee; reviewer grok 01a215bb-ad5f-7053-950e-0b7b09261a05

## Budget (by-diff)

- Lines changed: 51
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
