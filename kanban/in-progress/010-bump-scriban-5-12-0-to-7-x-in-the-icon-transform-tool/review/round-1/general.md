# Round 1 — general
**Date:** 2026-11-13
**Scope reviewed:** branch `task/010-bump-scriban-5-12-0-to-7-x-in-the-icon-transform` vs merge-base `49caf88`. Product diff is `Directory.Packages.props` (Scriban `5.12.0` → `7.6.1`). Call sites in `tools/transform/Program.cs` and `tools/transform/template.scriban` were read; they are unchanged.

## Summary

The change pins Scriban to 7.6.1, the 7.x version named when the task was filed, and leaves the published library, its version, and `releases.md` untouched. Scriban is referenced only by `tools/transform`. `Template.Parse` and `Template.Render(object)` are still the APIs that `Program.cs` calls, and `StandardMemberRenamer.Rename` is the same in 5.12.0 and 7.6.1, so `theNamespace` still imports as `the_namespace` for `{{the_namespace}}` and `{{svg}}`. The implementer restatement matches the original task, including the `update-icons` regeneration steps. Recorded Results cover the bump, a byte-identical regen of 1289 icons, and green build, test, and audit. No issues.

## Issues
