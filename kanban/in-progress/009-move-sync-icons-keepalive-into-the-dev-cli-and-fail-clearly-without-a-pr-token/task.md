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

## Checklist

- [ ] Keepalive dev-cli command
- [ ] `sync-icons.yml` keepalive job calls it (no inline logic)
- [ ] Clear PR-permission failure message + missing-secret warning in `update-icons --pr`
- [ ] readme one-time setup for `HEROICONS_SYNC_TOKEN`
- [ ] actionlint / build / test / audit green

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- After merge, a manual `workflow_dispatch` of "Sync icons" verifies the keepalive (upstream is
  still v2.2.0, so the sync step should report up to date).
- Do not create the secret or flip repo/org Actions settings — owner action.
