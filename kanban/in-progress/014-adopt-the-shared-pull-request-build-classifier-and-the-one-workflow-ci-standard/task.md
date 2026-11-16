# Adopt the shared pull request build classifier and the one-workflow CI standard

## Description

Bring timewarp-heroicons to the org CI standard (ganda 476 / 479 / 565 / 587, audited by ganda 655): `workflow.yml` has
no `paths` / `paths-ignore` filter on `pull_request`, so every pull request reports `CI`, and `dev workflow`
calls the shared classifier `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15
(timewarp-nuru 595) to skip build and test on a pull request that touches no build path (kanban, docs, ...).

Tracked by timewarp-ganda 656 (architecture reposet rollout).

Survey 2026-11-16 for this repo: has a `pull_request` paths filter; `dev workflow` does not classify. Work: remove the filter, add the classifier call.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes.
2. Bump the `TimeWarp.Nuru.DevCli` pin to `4.0.0-beta.15` (and `TimeWarp.Nuru` to the same version if pinned).
3. In the `workflow` command's pull-request path, call `PullRequestBuildClassifier.DecideAsync` first. On skip,
   print its message and exit 0. Push and release paths are unchanged.
4. Remove `paths` / `paths-ignore` from the `pull_request` trigger of `.github/workflows/workflow.yml`. Keep any
   `push` filter and add a comment saying why (a kanban-only merge must not rebuild and upload packages).
5. If this repo builds from roots outside the classifier defaults (`source`, `tests`, `tools`, `msbuild`), declare
   them through the classifier's config (`PullRequestBuildConfig`), not by copying or editing the rule.
6. No local copy of the classification rule remains in `tools/dev-cli/`.
7. `ganda repo audit` passes, including both CI-standard checks from ganda 655.

## Goal (implementer restatement)

Every pull request in this repo must report a `CI` check, and a pull request that touches only non-build
paths (kanban, documentation, ...) must pass that check quickly without building. To get there:

- `ganda repo audit --fix` runs first and its changes are committed.
- `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are pinned at `4.0.0-beta.15`.
- `dev workflow` calls `PullRequestBuildClassifier.DecideAsync` before any build step on non-release runs
  (the classifier itself builds on every non-`pull_request` event). On skip it prints the classifier's message
  and exits 0. The release path is unchanged.
- `workflow.yml` has no `paths` filter on `pull_request`; the `push` filter stays, with a comment saying a
  kanban-only merge must not rebuild and upload packages.
- Repo-specific build inputs the default rule would skip are declared in `.timewarp/dev.jsonc` under
  `pullRequestBuild.buildPaths`: the packed `readme.md` and `assets/logo.png`. No local copy of the rule.
- Acceptance: `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` (including the two CI-standard
  checks) pass; this task's pull request shows a `CI` run.

## Checklist

- [ ] `ganda repo audit --fix` run and committed
- [ ] DevCli pin at 4.0.0-beta.15
- [ ] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [ ] No `paths` filter on `pull_request`; `push` filter kept with a why comment
- [ ] Extra build roots (if any) declared through config
- [ ] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Notes

Default branch is `main`. Leave `sync-icons.yml` as it is.
After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
