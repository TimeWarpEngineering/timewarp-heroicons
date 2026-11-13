# Bump Scriban 5.12.0 to 7.x in the icon transform tool

## Description

`ganda nuget outdated` (2026-11-13) reports `Scriban 5.12.0 -> 7.6.1`. Scriban is used only by
`tools/transform` (SVG → Razor generator: `tools/transform/Program.cs`,
`tools/transform/template.scriban`), which `dev update-icons` runs during an icon sync. It is not a
dependency of the published `timewarp-heroicons` package, so consumers are unaffected. Two major
versions may change the template/parser API.

## Requirements

- `Directory.Packages.props`: Scriban → latest stable 7.x (7.6.1 at filing).
- Fix any API breaks in `tools/transform/Program.cs` and any template syntax changes in
  `template.scriban`.
- **Output must not change:** regenerate all icons from heroicons v2.2.0 `optimized/`
  (clone tag `v2.2.0` into a temp dir and run the transform into `source/timewarp-heroicons`,
  the same way `update-icons` does) and confirm `git status --porcelain source/` is empty.
  If output differs, explain why in Notes and fix the template so it matches; do not commit
  regenerated icon changes.
- Do not change the published library, its version, or `releases.md`.
- `./bin/dev build`, `./bin/dev test`, `ganda repo audit` green.

## Goal (implementer restatement)

Move the icon generator (`tools/transform`) from Scriban 5.12.0 to the latest stable 7.x so the
dev toolchain is current, without any effect on the shipped library.

Acceptance:
- `Directory.Packages.props` pins Scriban to the latest stable 7.x.
- `tools/transform` compiles and runs against it (API/template fixes only if needed).
- Regenerating from heroicons `v2.2.0` `optimized/` into `source/timewarp-heroicons` (same steps as
  `update-icons`: drop 16/20/24 trees, `dotnet run --project tools/transform/transform.csproj`)
  leaves `git status --porcelain source/` empty.
- No change to the library, its version, or `releases.md`; no regenerated icons committed.
- `./bin/dev build`, `./bin/dev test`, `ganda repo audit` pass.

## Checklist

- [x] Bump Scriban
- [x] Fix transform API / template breaks
- [x] Regenerate from v2.2.0; `source/` byte-identical
- [x] build / test / audit green

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)
- Implemented 2026-11-13 (implement oracle, ganda task work): Scriban 5.12.0 → 7.6.1; no code or template changes needed.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session c371ce8b-a7f3-4a18-aa2c-1782605f2b2f, max-turns 500, Done — 2026-11-13T13:46:47Z

## Notes

- Sibling task 011 (multi-target the library) depends on this one; both edit
  `Directory.Packages.props`.

## Results

- `Directory.Packages.props`: Scriban `5.12.0` → `7.6.1` (latest stable on nuget.org, checked
  with `dotnet package search Scriban --exact-match`).
- No API breaks: `Template.Parse` / `Template.Render(object)` and the default snake_case member
  renaming (`theNamespace` → `the_namespace`) behave the same in 7.6.1. `Program.cs` and
  `template.scriban` are unchanged.
- `tools/transform/obj/project.assets.json` resolves `Scriban/7.6.1`.
- Regeneration: cloned heroicons tag `v2.2.0` to a temp dir, removed
  `source/timewarp-heroicons/{16,20,24}`, ran the transform into `source/timewarp-heroicons`
  (same as `update-icons`). 1289 `.razor` files written; `git status --porcelain source/` was
  empty (byte-identical). Nothing regenerated was committed.
- Library, version, and `releases.md` untouched.

### Gates

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | pass (0 errors; 9 pre-existing IL2111 trim warnings in tests/sample-app) |
| Test | `./bin/dev test` | pass |
| Audit | `ganda repo audit` | pass, exit 0 (42 passed, 2 skipped; 2 pre-existing advisory warnings: `ci-code-coverage`, `vscode-window-icon`, non-blocking and unrelated to this change) |

### How to validate

Smoke:

```bash
T=$(mktemp -d)
git clone -q --depth 1 --branch v2.2.0 https://github.com/tailwindlabs/heroicons.git "$T/heroicons"
rm -rf source/timewarp-heroicons/16 source/timewarp-heroicons/20 source/timewarp-heroicons/24
dotnet run --project tools/transform/transform.csproj -- "$T/heroicons/optimized" "$PWD/source/timewarp-heroicons"
git status --porcelain source/
grep -o '"Scriban/[^"]*"' tools/transform/obj/project.assets.json | sort -u
```

Expect: the transform prints its `input:… output:…` line and exits 0; `git status --porcelain
source/` prints nothing; the assets grep prints `"Scriban/7.6.1"`.

