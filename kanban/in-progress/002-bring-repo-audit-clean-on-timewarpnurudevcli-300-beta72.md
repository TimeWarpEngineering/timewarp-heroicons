# Bring repo audit-clean on TimeWarp.Nuru.DevCli 3.0.0-beta.72

## Description

Org wave (timewarp-nuru 458-010 remediation + DevCli 3.0.0-beta.72 adoption —
they are the same wave: the audit's `nuru` check went red org-wide when
beta.72 shipped, by design). Passing `ganda repo audit` now means adopting the
full release toolkit: `dev release`, promotion gates, attestation verifier,
trusted-publishing probe, derived package sets.

## Checklist

- [x] `ganda repo audit --fix` (bumps TimeWarp.Nuru/DevCli to latest, fixes kebab/structure where fixable)
- [x] Verify Directory.Packages.props pins TimeWarp.Nuru.DevCli (and TimeWarp.Nuru where referenced) at 3.0.0-beta.72
- [x] Build — NURU050 names any missing DI registration (e.g. `IPackableProjectService`); add per the DevCli readme migration notes (CS0101 local-CiMode note also applies)
- [x] `dev self-install` (AOT binary is a snapshot; new commands like `release` are absent until reinstalled)
- [x] `ganda repo audit` → PASSES ALL CHECKS (if a check is structurally unfixable here, record it explicitly with a reason instead of forcing)
- [x] Smoke: `dev --help` shows `release`; `dev check-version` derives the packable set (publishers only)
- [x] Commit everything (audit fixes, props, dev.cs, kanban) — local commits fine; ride the repo's normal merge flow

## Notes

Created 2026-08-08 from the nuru 458 program session. timewarp-nuru is the
reference (audit-clean at beta.72, first release shipped through the full
machinery).

### Implementation notes (2026-08-08)

**Before:** 20 pass / 2 fail / 1 skip — `nuru` beta.71, `kebab-path-names` (7 paths).

**After:** all checks pass.

- Pins: Nuru+DevCli 3.0.0-beta.72, Amuru 1.0.0, Amuru.Tools 1.0.0-beta.2
- Package refs for Amuru/Amuru.Tools on tools/dev-cli; DI + IPackableProjectService
- Kebab: LICENSE/README, Logo.svg→logo.svg, sample-app Client/Pages/Shared → client/pages/shared
- Logo.png collided with existing logo.png (different content) → renamed `brand-logo.png` + readme link update

## Results

Repo is audit-clean on TimeWarp.Nuru / DevCli **3.0.0-beta.72**.

### How to validate

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-heroicons/main
grep -E 'TimeWarp\.(Nuru|Amuru)' Directory.Packages.props
ganda repo audit   # exit 0
./bin/dev --help | grep release
./bin/dev check-version
```

**Depends on / Not in scope:** local commits only; version already published.

## Session

- Implementation: grok (2026-08-08)
