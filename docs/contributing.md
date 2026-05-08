# Contributing to SoundSwitch

Thank you for your interest in contributing to SoundSwitch! This document outlines the standards, conventions, and processes for contributing code, translations, and documentation.

## Code of Conduct

Be respectful, constructive, and inclusive. SoundSwitch is a community tool used by audio enthusiasts, gamers, and professionals. Feedback and contributions are welcome from everyone.

## How to Contribute

### Bug Reports

1. Search the [existing issues](https://github.com/Belphemur/SoundSwitch/issues) to avoid duplicates.
2. If no existing issue matches, create a new one with:
   - A clear, descriptive title.
   - Steps to reproduce.
   - Expected vs. actual behavior.
   - SoundSwitch version (`Help → About` in the app).
   - Windows version and audio device information.
   - Log files (exported from **Settings → Troubleshooting → Export Log Files**).

### Feature Requests

1. Search existing issues and discussions to avoid duplicates.
2. Create a new issue describing:
   - The problem you're trying to solve.
   - How the proposed feature addresses it.
   - Any alternative approaches you've considered.

### Code Contributions

1. Fork the repository.
2. Create a branch from `dev` with a descriptive name (`fix/audio-device-cleanup`, `feat/profile-restore`).
3. Make your changes following the conventions described below.
4. Build the solution (`dotnet build SoundSwitch.sln -c Debug`) to verify compilation.
5. Run the test suite (`dotnet test SoundSwitch.sln`) to ensure everything passes.
6. Submit a pull request targeting the `dev` branch.

## Coding Conventions

### Language and Framework

- **C#** across all projects.
- **.NET 10** (see `global.json` for SDK version pinning).
- **WinForms** for the user interface.
- **Core Audio API** (Windows) for audio device management.

### C# Style

- Follow the existing `.editorconfig` and `.DotSettings` for style rules.
- Use `NameClean` over raw device names when dealing with displayed audio device names.
- Prefer small, targeted fixes over broad refactors.
- Keep user-facing strings in localization resources (`.resx` files), not hardcoded.
- Treat this as a **Windows desktop app first**; avoid cross-platform assumptions in WinForms and Windows integration code.

### Architecture Principles

- Each project has a clear responsibility. See the [Architecture Overview](./architecture.md) for details.
- Keep `SoundSwitch.Common/` as the sole location for shared utilities.
- Audio device management lives in `SoundSwitch.Audio.Manager/`; the UI layer should not directly call Windows audio APIs.
- Inter-process communication is handled by `SoundSwitch.IPC/`; CLI and UI communicate through this layer.

### Commits

SoundSwitch uses **Conventional Commits** with a custom configuration for automatic semantic versioning and changelog generation.

#### Commit Format

```text
type(scope): description

[optional body explaining WHY, not what]

[optional footer with breaking changes or issue refs]
```

#### Release-Relevant Commit Types

| Type | Effect |
|------|--------|
| `feat` | Minor release version bump |
| `fix` | Patch version bump |
| `perf` | Patch version bump |
| `lang` | Patch version bump (translation updates) |
| `boost` | Patch version bump (performance improvements) |
| `revert` | Patch version bump |
| `tests` | No version bump (logged in release notes only) |
| `docs` | No version bump |
| `ci` | No version bump |
| `build` | No version bump |

#### Scopes

Use module-style scopes to indicate which area of the codebase the change affects:

- `fix(localization): ...`
- `feat(audio-manager): ...`
- `perf(ui): ...`
- `lang(es-ES): ...` (for Spanish translation)

#### Breaking Changes

To trigger a major release, append `!` to the type:

```text
feat(api)!: change device switching API signature

BREAKING CHANGE: The `SwitchDevice` method now requires a `DeviceType` enum parameter.
```

### Pull Requests

- Target the `dev` branch for features and fixes.
- Target `master` or `beta` for release-related changes.
- Include a clear description of what changed and why.
- Link to related issues using `#1234` syntax.
- For UI changes, include "before" and "after" screenshots.
- For new features, consider whether tests are needed and update test coverage accordingly.

## Translations

SoundSwitch supports 20+ languages. Translations are community-maintained via [Weblate](https://hosted.weblate.org/projects/soundswitch/#languages).

- Visit Weblate to translate or improve existing languages.
- Translation changes from Weblate are periodically synced back to the repository as `lang(scope)` commits.
- If you want to add a new language, start by adding it on Weblate.

## Documentation

### User Documentation

The live documentation site is built with **VuePress** and lives in `website/`. For contributions, edit the markdown files in `website/src/` and run `npm run docs:dev` locally to preview.

### Developer Documentation

This `docs/` folder contains developer-facing guides. Update or add files here when contributing architectural changes.

## Testing

### Running Tests Locally

First, build the solution to verify compilation:

```cmd
dotnet build SoundSwitch.sln -c Debug
```

Then run the test suite:

```cmd
dotnet test SoundSwitch.sln -c Debug
```

### What to Test

- Any change to `SoundSwitch.Audio.Manager/` should have corresponding test coverage in `SoundSwitch.Audio.Manager.Tests/`.
- Changes to UI logic should be tested manually; automated UI testing is limited.
- Localization changes should be validated by launching the app in the target language.

## Release Process

Releases are fully automated via [semantic-release](https://github.com/semantic-release/semantic-release):

1. Commits to `master` or `beta` trigger the **release** GitHub Action.
2. The commit-analyzer determines the next version.
3. Version numbers in `SoundSwitch/Properties/AssemblyInfo.cs` are updated.
4. The changelog is generated and committed.
5. The solution is built, and installers are created.
6. A GitHub Release draft is created with release notes.

Contributors don't need to worry about versioning — just follow the conventional commit format and the system handles the rest.
