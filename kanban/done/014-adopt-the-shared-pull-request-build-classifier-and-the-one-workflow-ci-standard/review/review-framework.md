# Review framework — task 014

**Date:** 2026-11-16
**Host task:** kanban/in-progress/014-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/
**Diff scope:** branch `task/014-adopt-the-shared-pull-request-build-classifier-and` vs merge-base `ed7db9b` (`master`). Product commits `8eb0892` (audit `--fix` hook) and `fb9504f` (classifier, pins, workflow trigger, `.timewarp/dev.jsonc`). Product paths: `.githooks/prepare-commit-msg`, `.githooks/prepare-commit-msg.cs`, `.github/workflows/workflow.yml`, `.timewarp/dev.jsonc`, `Directory.Packages.props`, `tools/dev-cli/endpoints/workflow-command.cs`.
**Plan / brief:** Adopt the shared `PullRequestBuildClassifier` and the one-workflow CI standard (no `pull_request` paths filter; skip build on non-build diffs). See `## Goal (implementer restatement)` in `task.md`.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** implementer-claude session 4d3701eb-8da6-4253-bf82-a322b9fec426; reviewer grok session 01a1e50d-5efe-7f40-840c-5d8a2274163f

## Budget (by-diff)

- Lines changed: 294
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
