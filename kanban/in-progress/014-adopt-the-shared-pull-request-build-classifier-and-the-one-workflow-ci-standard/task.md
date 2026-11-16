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

- [x] `ganda repo audit --fix` run and committed
- [x] DevCli pin at 4.0.0-beta.15
- [x] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [x] No `paths` filter on `pull_request`; `push` filter kept with a why comment
- [x] Extra build roots (if any) declared through config
- [x] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Notes

Default branch is `main`. Leave `sync-icons.yml` as it is.
After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Results

- `ganda repo audit --fix` added `.githooks/prepare-commit-msg` (+ `.cs`); committed as 8eb0892.
- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.13 → 4.0.0-beta.15.
  Boy-scout for `package-pin-currency`: `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` 3.0.0-beta.2 → 3.0.0-beta.5.
- `tools/dev-cli/endpoints/workflow-command.cs`: the non-release branch calls
  `PullRequestBuildClassifier.DecideAsync(RepoRoot, ct)`, prints `decision.Message`, and returns (exit 0) when
  `RequiresBuild` is false. Release path unchanged. No local copy of the rule.
- `.github/workflows/workflow.yml`: `pull_request` has no `paths` filter (comment says why); the `push` filter is
  kept with a comment that a kanban-only merge must not rebuild and upload packages. `sync-icons.yml` untouched.
- `.timewarp/dev.jsonc` (new): `pullRequestBuild.buildPaths` = `readme.md`, `assets/logo.png` — both are packed
  into the nupkg by `source/Directory.Build.props`, and the default rule would skip them. No other build roots
  outside `source`, `tests`, `tools`, `msbuild` (root `Directory.*.props` already build under the default rule).
- CI run URL: the pull request is opened by the host `open-pr` node after this oracle; record the `CI` run there.
- Follow-up (not blocking): `ci-code-coverage` audit warning was already present before this task (CI does not
  collect coverage); left as is.

### Gates

| Command | Result |
|---|---|
| `./bin/dev build` | PASS — 0 errors (9 pre-existing warnings) |
| `dotnet run --file tools/dev-cli/dev.cs -- self-install` (dev-cli changed) | PASS |
| `./bin/dev test` | PASS — exit 0, all suites green |
| `ganda repo audit` | PASS — `workflow-file` and `dev-workflow-classifier` pass; 1 advisory warning (`ci-code-coverage`, pre-existing) |

### How to validate

Smoke (skip path, kanban-only diff):

```bash
b=$(git rev-parse 7e2a1d0^); h=$(git rev-parse 7e2a1d0); d=$(mktemp -d)
echo "{\"pull_request\":{\"base\":{\"sha\":\"$b\"},\"head\":{\"sha\":\"$h\"}}}" > $d/e.json
GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=$d/e.json ./bin/dev workflow; echo EXIT=$?
```

Expect: `Pull request changes only declared non-build paths. Skipping build and test. ... (1 changed paths)` and
`EXIT=0`, with no clean/build/test steps. (Observed exactly that on 2026-11-16.)

Also expect: `ganda repo audit` passes `workflow-file` and `dev-workflow-classifier`; this task's pull request
shows a `CI` check even though it is not filtered by paths.

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 4d3701eb-8da6-4253-bf82-a322b9fec426, max-turns 500, Done — 2026-11-16T15:50:12Z
