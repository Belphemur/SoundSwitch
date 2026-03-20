# SoundSwitch Repository Agents

## Scope

This file defines repository-wide guidance for the SoundSwitch workspace. More specific `AGENTS.md` files in subdirectories override or extend this guidance for their own areas.

## Technology

- Primary language: C#
- Platform: .NET on Windows
- UI stack: WinForms desktop application
- Main solution entry point: `SoundSwitch.sln`

## Repository Map

- `SoundSwitch/`: main WinForms application, localization, framework, services, UI, and application model
- `SoundSwitch.Audio.Manager/`: Windows audio device management and policy switching
- `SoundSwitch.Common/`: shared code used across projects
- `SoundSwitch.CLI/`: command-line entry points and automation surface
- `SoundSwitch.IPC/`: inter-process communication components
- `SoundSwitch.UI.Menu/` and `SoundSwitch.UI.UserControls/`: UI-focused reusable components
- `SoundSwitch.Tests/` and `SoundSwitch.Audio.Manager.Tests/`: automated regression coverage

## General Rules

- Keep changes consistent with an existing C#/.NET codebase; prefer small, targeted fixes over broad refactors.
- Treat this repository as a Windows desktop app first; avoid introducing cross-platform assumptions into WinForms and Windows integration code.
- Keep user-facing strings in localization resources instead of hardcoded literals.
- Preserve existing public behavior unless the change explicitly intends to alter it.
- Prefer `NameClean` over raw device names when dealing with displayed audio device names.

## Commits

- Use conventional commits for commit messages.
- Follow the repository semantic-release config in `package.json` rather than generic conventional-commit defaults.
- Release-relevant commit types are `feat`, `fix`, `perf`, `lang`, and `boost`.
- `feat` produces a minor release; `fix`, `perf`, `lang`, `boost`, and `revert` produce a patch release; breaking changes produce a major release.
- `tests` is recognized in generated release notes, but it does not trigger a release on its own.
- Scoped or module-style commit messages are encouraged when useful, for example `fix(localization): ...` or `feat(audio-manager): ...`.
- Keep the subject concise and focused on the user-visible or engineering outcome.

## Validation

- Default validation: `dotnet build SoundSwitch.sln -c Debug`
- Run targeted tests for the area you change, and at minimum run the related test project when changing shared logic.
