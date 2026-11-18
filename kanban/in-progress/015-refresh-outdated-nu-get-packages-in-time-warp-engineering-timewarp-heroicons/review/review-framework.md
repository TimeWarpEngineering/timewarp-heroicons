# Review framework — task 015

**Date:** 2026-11-19
**Host task:** kanban/in-progress/015-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-heroicons/
**Diff scope:** branch `task/015-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (merge-base `ca6b0acf`). Product diff: `Directory.Packages.props`, `tools/dev-cli/endpoints/update-icons-pull-request.cs`, `tools/dev-cli/endpoints/update-icons-regenerate.cs`, `tools/dev-cli/services/runfile-tests.cs`.
**Plan / brief:** Bump the six dev-only TimeWarp pins named in the task (Amuru and Amuru.Tools 3.0.0-beta.10, Jaribu 1.0.0-beta.18, Nuru and Nuru.DevCli 4.0.0-beta.16, SourceGenerators 1.0.0-beta.17). Do not change `<Version>` in `source/Directory.Build.props`. The task says to edit `Directory.Packages.props` only; the implementer also moved four `Shell.Builder("gh"|"dotnet")` call sites onto typed Amuru.Tools builders after AMURU0001.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok `01a1efda-bb67-7053-8699-84dae62f18f1` (2026-11-19)

## Budget (by-diff)

- Lines changed: 102
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
