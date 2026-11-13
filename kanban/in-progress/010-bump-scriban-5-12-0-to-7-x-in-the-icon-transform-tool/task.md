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

## Checklist

- [ ] Bump Scriban
- [ ] Fix transform API / template breaks
- [ ] Regenerate from v2.2.0; `source/` byte-identical
- [ ] build / test / audit green

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- Sibling task 011 (multi-target the library) depends on this one; both edit
  `Directory.Packages.props`.
