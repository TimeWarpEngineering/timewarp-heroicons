# Move sync-icons keepalive into the dev-cli and fail clearly without a PR token

## Description

Repo convention: GitHub workflow YAML is a **thin trigger**; pipeline logic lives in C# in the
dev-cli (`tools/dev-cli/`, e.g. `dev workflow`, `dev update-icons`). Task 008 split the monthly
sync into `.github/workflows/sync-icons.yml` (keep that split: it keeps the inactivity auto-disable
away from `workflow.yml`, which carries PR CI and the trusted-publishing release job). But its
`keepalive` job puts logic inline in YAML:

```yaml
run: |
  gh api --method PUT "repos/$env:GITHUB_REPOSITORY/actions/workflows/sync-icons.yml/enable"
  exit $LASTEXITCODE
```

Move that into the dev-cli so the YAML only calls `dev …`.

Second gap found 2026-11-13 after 008 merged: the sync step runs `gh pr create` with
`GH_TOKEN: secrets.HEROICONS_SYNC_TOKEN || secrets.GITHUB_TOKEN`. The secret does not exist yet
(`gh secret list` empty), and "Allow GitHub Actions to create and approve pull requests" is off at
repo and org level (`can_approve_pull_request_reviews: false`). So on the next upstream release
the PR step fails. The run going red is correct, but the error must say exactly what to fix.
Steve will add the `HEROICONS_SYNC_TOKEN` secret (a fine-grained PAT) himself; this task does not
create secrets or change repo settings.

## Requirements

- New dev-cli endpoint (e.g. `dev workflow-keepalive` or `dev keepalive --workflow sync-icons.yml`;
  pick a name that fits the existing routes) that calls
  `PUT /repos/{owner}/{repo}/actions/workflows/{file}/enable` (204 for an active workflow too).
  Repo from `GITHUB_REPOSITORY`, token from `GH_TOKEN`/`GITHUB_TOKEN`. Use the existing HTTP
  pattern in `update-icons-lookups.cs` or TimeWarp.Amuru `gh` invocation, whichever the dev-cli
  already uses. Non-2xx response → clear error + non-zero exit.
- `sync-icons.yml` `keepalive` job calls the dev-cli command (setup-dotnet + `dotnet run
  tools/dev-cli/dev.cs -- …`), no inline `gh api`. Keep it a separate job with
  `permissions: actions: write` only, so a failing sync does not skip it.
- `dev update-icons --pr`: when PR creation fails for permissions (403 / "GitHub Actions is not
  permitted to create or approve pull requests"), print a message naming both fixes: add a
  `HEROICONS_SYNC_TOKEN` repo secret (fine-grained PAT, this repo, contents + pull requests
  write), or enable the repo setting. Exit non-zero. Also warn (not fail) when running under
  Actions with no `HEROICONS_SYNC_TOKEN`, since a PR opened with `GITHUB_TOKEN` triggers no CI.
  Detecting which token is in use may need a small env flag set in the YAML; keep it minimal.
- readme "Syncing icons and releasing": one-time setup step for the `HEROICONS_SYNC_TOKEN`
  secret, with the scopes.
- `actionlint` clean; `./bin/dev build`, `./bin/dev test`, `ganda repo audit` green.

## Goal (implementer restatement)

Make `sync-icons.yml` a thin trigger again and make the sync PR step fail with an actionable message.

Acceptance:

1. A dev-cli command (`dev workflow-keepalive --workflow <file>`) PUTs
   `/repos/{GITHUB_REPOSITORY}/actions/workflows/{file}/enable` with `GH_TOKEN`/`GITHUB_TOKEN`.
   It succeeds on 2xx (204), and on a non-2xx response or a missing repo/token it prints a clear
   error and exits non-zero.
2. The `keepalive` job in `sync-icons.yml` stays a separate job with `permissions: actions: write`
   only. It runs checkout + setup-dotnet + `dotnet run tools/dev-cli/dev.cs -- workflow-keepalive …`
   and has no inline `gh api`.
3. `dev update-icons --pr`: when `gh pr create` fails for a permissions reason (403, or "not
   permitted to create or approve pull requests"), the error names both fixes: a
   `HEROICONS_SYNC_TOKEN` secret (fine-grained PAT, this repo, contents + pull requests write) or the
   repo setting. The command exits non-zero. Under Actions without the PAT (YAML sets a
   `HEROICONS_SYNC_TOKEN_SET` flag), it prints a warning that does not fail the run: a PR opened with
   `GITHUB_TOKEN` triggers no CI.
4. The readme section "Syncing icons and releasing" has the one-time `HEROICONS_SYNC_TOKEN` setup
   with its scopes.
5. actionlint is clean, and `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` pass.

## Checklist

- [x] Keepalive dev-cli command
- [x] `sync-icons.yml` keepalive job calls it (no inline logic)
- [x] Clear PR-permission failure message + missing-secret warning in `update-icons --pr`
- [x] readme one-time setup for `HEROICONS_SYNC_TOKEN`
- [x] actionlint / build / test / audit green

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)
- Implemented 2026-11-13 (implement oracle, ganda task work): keepalive command, YAML, PR-permission
  message and warning, readme. Gates are recorded under ## Results.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session b3fa260a-f91d-41fe-9212-f092ddcce44a, max-turns 500, Done — 2026-11-13T12:38:28Z

## Results

- New `dev workflow-keepalive --workflow <file> [--repo owner/name]`
  (`tools/dev-cli/endpoints/workflow-keepalive-command.cs`). It sends `PUT /repos/{repo}/actions/workflows/{file}/enable`
  over `HttpClient`, the same pattern as `update-icons-lookups.cs`. Repo comes from `--repo` or
  `GITHUB_REPOSITORY`. Token comes from `GH_TOKEN` or `GITHUB_TOKEN`. A missing workflow, repo, or token,
  or any non-2xx response, prints a single error line with a hint for 401, 403, or 404 and the response
  body, and exits 1.
- `sync-icons.yml` `keepalive` job: checkout, then setup-dotnet, then
  `dotnet run tools/dev-cli/dev.cs -- workflow-keepalive --workflow sync-icons.yml`. There is no inline
  `gh api`. It is still a separate job. Permissions are `actions: write` plus `contents: read`.
  `contents: read` is the only addition, because the job now checks out the repo to run the dev-cli.
- `dev update-icons --pr`:
  - When `gh pr create` fails and the output contains "not permitted to create or approve pull
    requests", "Resource not accessible by integration", `HTTP 403`, or `403 Forbidden`, the
    error names both fixes:
    1. a `HEROICONS_SYNC_TOKEN` fine-grained PAT, this repo only, Contents and Pull requests read/write
    2. the repo setting "Allow GitHub Actions to create and approve pull requests", which the
       org policy may also need to allow

    The command exits 1.
  - Under Actions (`GITHUB_ACTIONS=true`) with `HEROICONS_SYNC_TOKEN_SET` not `true`, every `--pr`
    run prints a `::warning::` annotation that does not fail the run. The YAML sets that flag with
    `${{ secrets.HEROICONS_SYNC_TOKEN != '' }}`.
- readme "Syncing icons and releasing" has a one-time setup block: create the fine-grained PAT
  (repo-only, Contents and Pull requests read/write), store it with `gh secret set HEROICONS_SYNC_TOKEN`,
  and know the fallback behaviour and the renewal note. The keepalive paragraph now names
  `dev workflow-keepalive`.
- No secrets were created and no repo or org settings were changed.

### Gates

| Gate | Command | Result |
|------|---------|--------|
| build | `./bin/dev build` | pass (0 errors; 37 warnings that were already there) |
| dev-cli compile | `dotnet run --file tools/dev-cli/dev.cs -- self-install` (once, after the `tools/dev-cli/` change) | pass |
| actionlint | `docker run --rm -v "$PWD:/repo" -w /repo rhysd/actionlint:latest -no-color` (v1.7.12) | pass (no findings) |
| test | `./bin/dev test` | pass |
| audit | `ganda repo audit` | pass (exit 0). 2 advisory warnings were already there: `ci-code-coverage` (this repo has no unit-test suite to cover) and `vscode-window-icon` (`.vscode/settings.json` is gitignored and local-only). Neither was touched. |

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install   # or use ./bin/dev from a fresh install
./bin/dev workflow-keepalive --help
env -u GITHUB_REPOSITORY ./bin/dev workflow-keepalive --workflow sync-icons.yml; echo $?
GH_TOKEN=bogus ./bin/dev workflow-keepalive --workflow sync-icons.yml --repo TimeWarpEngineering/timewarp-heroicons; echo $?
GITHUB_REPOSITORY=TimeWarpEngineering/timewarp-heroicons GH_TOKEN=$(gh auth token) ./bin/dev workflow-keepalive --workflow sync-icons.yml; echo $?
GITHUB_TOKEN=$(gh auth token) GITHUB_ACTIONS=true HEROICONS_SYNC_TOKEN_SET=false ./bin/dev update-icons --pr; echo $?
GITHUB_TOKEN=$(gh auth token) GITHUB_ACTIONS=true HEROICONS_SYNC_TOKEN_SET=true ./bin/dev update-icons --pr; echo $?
docker run --rm -v "$PWD:/repo" -w /repo rhysd/actionlint:latest -no-color
```

Expect:

- With no repo set: `Error: workflow keepalive failed: repository not set: ...`, exit 1.
- With a bogus token: `... returned 401 Unauthorized. The token is invalid or expired. Response: {"message": "Bad credentials" ...}`, exit 1.
- With a real token: `Workflow sync-icons.yml in TimeWarpEngineering/timewarp-heroicons is enabled; its inactivity timer is reset (204).`, exit 0.
  `gh workflow list --all` still shows `Sync icons active`. All three were verified 2026-11-13.
- `update-icons --pr` with `HEROICONS_SYNC_TOKEN_SET=false` prints the `::warning title=HEROICONS_SYNC_TOKEN not set::...`
  line first, then `Icons are already up to date (timewarp-heroicons 2.2.0 is on nuget.org).`, exit 0.
  With `=true` there is no warning line.
- actionlint prints nothing, exit 0.
- After merge: run Actions → Sync icons → Run workflow. `keepalive` is green with the 204 line. `sync-icons`
  is green, reports up to date, and shows the missing-secret warning annotation until the owner adds
  `HEROICONS_SYNC_TOKEN`.
- Not exercised live: the 403 `gh pr create` message. It needs a newer upstream release, and no
  real PR was opened.

## Notes

- After merge, a manual `workflow_dispatch` of "Sync icons" verifies the keepalive (upstream is
  still v2.2.0, so the sync step should report up to date).
- Do not create the secret or flip repo/org Actions settings — owner action.
