# Add trusted-publishing probe mode to workflow.yml

## Description

org 458-009 probe (NuGet has no policy-enumeration API; probe = dispatch mode that runs only the nuget/login OIDC exchange and stops — success proves the workflow.yml policy matches; reference timewarp-nuru's workflow.yml).

## Checklist

- [x] probe input added
- [x] login step condition extended
- [x] probe-result step added
- [x] pipeline step skipped in probe mode
- [x] YAML valid

## Results

- Added a `mode` choice input (`merge`/`probe`, default `merge`) alongside the existing `sync_icons` boolean input on `workflow_dispatch`; `sync_icons` left untouched.
- The `sync-icons` job (its trigger condition and its unconditional `nuget/login` step) was left completely untouched, as directed — probe mode does not affect the icon-sync path.
- In the `ci` job, extended the `NuGet login (OIDC Trusted Publishing)` step's `if:` to also run when `inputs.mode == 'probe'`.
- Added a `Trusted publishing probe result` step immediately after login that echoes success when probe mode reaches it.
- Added `if: github.event_name != 'workflow_dispatch' || inputs.mode != 'probe'` to the `Run CI Pipeline` step so the actual build/test/publish pipeline is skipped in probe mode.
- The `ci` job's own gating condition (`github.event_name != 'schedule' && !(github.event_name == 'workflow_dispatch' && inputs.sync_icons)`) already lets probe dispatches through unchanged, since `sync_icons` defaults false.

### How to validate

**Smoke:** `gh workflow run workflow.yml -f mode=probe` after push → expect the "Trusted publishing probe result" step to run and go green.
**Expect:** a failure of the NuGet login step means the trusted-publishing policy is missing or misconfigured on NuGet.org for this repo + workflow.yml — not a bug in this change.
