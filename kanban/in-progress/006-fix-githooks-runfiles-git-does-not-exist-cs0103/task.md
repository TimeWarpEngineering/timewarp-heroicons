# Fix githooks runfiles: Git does not exist (CS0103)

## Description

Every commit in this repo prints a build failure from the post-commit hook
(`core.hooksPath = .githooks`):

```
.githooks/post-commit(7,16): error CS0103: The name 'Git' does not exist in the current context
The build failed. Fix the build errors and run again.
```

The hooks (`.githooks/post-commit`, `.githooks/post-commit.cs`, `.githooks/post-merge`,
`.githooks/post-merge.cs`) are runfiles with `#:package TimeWarp.Amuru` (no version, so the
central pin in `Directory.Packages.props` applies: `TimeWarp.Amuru` 1.0.0) and
`using TimeWarp.Amuru;`, then call `Git.FindRoot()`. `Git` does not resolve against that
package/namespace. The dev-cli also calls `Git.FindRoot()` and builds, but it references both
`TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` (1.0.0-beta.2) — likely `Git` lives in the
other package or a different namespace in Amuru 1.0.0. Found 2026-11-13 while committing task 005.

The hook failure does not block commits, but it means `ganda memsearch index-repo` never runs
on commit/merge, and it prints noise on every commit.

## Requirements

- All four hook files compile and run (exit 0) as runfiles with the repo's pinned packages.
- Fix the real cause (correct package/namespace/version), using `tw-amuru` / `tw-runfiles`
  guidance. Do not delete the hooks or drop the memsearch call.
- Check whether the hook content is ganda-managed (e.g. emitted by `ganda repo audit --fix` /
  hook install). If it is, fix it where ganda generates it or note the upstream issue, and make
  sure `ganda repo audit` stays clean.
- `post-commit` and `post-commit.cs` (and the `post-merge` pair) are duplicates; keep whatever
  layout ganda expects.

## Checklist

- [ ] Diagnose where `Git` lives for the pinned Amuru version(s)
- [ ] Fix all hook runfiles
- [ ] Verify: `dotnet run .githooks/post-commit` (and post-merge) exit 0 with no build error
- [ ] Verify a commit in the task worktree prints no CS0103
- [ ] `ganda repo audit` clean

## Session

- Created: 5e680869-5aa9-4ad6-b5cb-007489689241 (2026-11-13)

## Notes

- Use `ganda runfile cache --clear` to force a recompile when testing.
