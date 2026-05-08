# Building SoundSwitch

## Prerequisites

- **.NET SDK 10.0** — The target framework is defined in `global.json`. The SDK version and runtime can be auto-detected (on Windows) or installed via `winget`.
- **Visual Studio 2022** (recommended) or **MSBuild** from the [Build Tools for Visual Studio](https://visualstudio.microsoft.com/visual-cpp-build-tools/).
- **npm** — Required for the semantic-release toolchain that drives versioning and changelog generation.
- **Git** — For source control.
- **PowerShell 7+** (recommended) — Build and installer scripts use PowerShell 7+ features.

## Quick Build (from Visual Studio)

1. Open `SoundSwitch.sln` in Visual Studio.
2. Select the desired configuration (Debug/Release) and platform (AnyCPU).
3. Build the solution using **Build → Build Solution**.

## Command-Line Build

### Build All Projects

```cmd
dotnet build SoundSwitch.sln -c Debug
```

### Publish (Release)

```cmd
dotnet publish SoundSwitch\SoundSwitch.csproj -c Release
dotnet publish SoundSwitch.CLI\SoundSwitch.CLI.csproj -c Release
```

This outputs binaries to the `bin/` directory of each project.

### Build and Installer (Make.bat)

The repository includes a `Make.bat` script that automates the full build pipeline including the installer:

```cmd
Make.bat [Release|Debug]
```

This script:

1. Extracts the target framework from `SoundSwitch.csproj`.
2. Runs `dotnet publish` for the main app and CLI projects.
3. Generates HTML pages from `CHANGELOG.md`, `Terms.md`, and `README.md` (uses the `markdown-html` npm package if available).
4. Copies required assets (images, license, terms).
5. Runs the Inno Setup compiler to produce the installer.

### Build and Installer (PowerShell)

For PowerShell environments, use:

```powershell
.\tools\Build-Installer.ps1 -Configuration Release
```

This script provides a more detailed build pipeline with error checking and is the preferred method for CI environments.

## Running Tests

```cmd
dotnet test SoundSwitch.Tests\SoundSwitch.Tests.csproj
dotnet test SoundSwitch.Audio.Manager.Tests\SoundSwitch.Audio.Manager.Tests.csproj
```

Or test the entire solution:

```cmd
dotnet test SoundSwitch.sln -c Debug
```

## Project-Specific Commands

| Project | Command |
|---------|---------|
| Main application | `dotnet build SoundSwitch\SoundSwitch.csproj` |
| Audio Manager | `dotnet build SoundSwitch.Audio.Manager\SoundSwitch.Audio.Manager.csproj` |
| Shared utilities | `dotnet build SoundSwitch.Common\SoundSwitch.Common.csproj` |
| CLI | `dotnet build SoundSwitch.CLI\SoundSwitch.CLI.csproj` |
| IPC | `dotnet build SoundSwitch.IPC\SoundSwitch.IPC.csproj` |
| UI Menu | `dotnet build SoundSwitch.UI.Menu\SoundSwitch.UI.Menu.csproj` |
| UI User Controls | `dotnet build SoundSwitch.UI.UserControls\SoundSwitch.UI.UserControls.csproj` |

## Build Artifacts

- **Debug**: Binaries land in each project's `bin/Debug/` directory with full debug symbols.
- **Release**: Binaries land in `bin/Release/` with optimizations enabled.
- **Installers**: The `Make.bat` and `Build-Installer.ps1` scripts output an Inno Setup installer (`.exe`) alongside the published binaries.

## CI/CD Pipeline

The repository uses GitHub Actions for continuous integration and deployment:

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `.NET` | Push/PR to `dev`, `master`, `beta` | Builds, runs tests, validates translations, and lints code. |
| `release` | Push to `master`, `beta` | Runs `semantic-release` to determine version, builds, creates installer, and publishes to GitHub Releases. |
| `nightly` | Scheduled (daily) | Builds and publishes pre-release versions to a "nightly" GitHub release. |
| `test-installer-build` | Push/PR | Tests the installer build in isolation. |
| `codeql-analysis` | Scheduled/Push/PR | Runs GitHub's CodeQL static analysis. |
| `winget` | Release | Submits new versions to the Windows Package Manager (winget). |
| `docs` | Push to `master` | Builds and deploys the VuePress documentation site. |

## Debugging

### Visual Studio

- Set breakpoints and use F5 to start debugging.
- The main application entry point is in `SoundSwitch/Program.cs`.
- Audio manager tests can be run directly from the Test Explorer.

### Logging

SoundSwitch uses **Serilog** for structured logging. Logs are written to the application's data directory and can be exported via the **Troubleshooting** tab in the settings UI. To view live logs during development, you can attach a debugger and inspect the `Log` object in the relevant service classes.

## Platform Support

| Platform | Architecture | Status |
|----------|-------------|--------|
| Windows | x64 | ✅ Primary |
| Windows | ARM64 | ✅ Supported (7.0+) |

Cross-platform builds are not supported; SoundSwitch relies on Windows-specific APIs (Core Audio API, WinForms, WinAPI hooks). On Linux CI runners, `EnableWindowsTargeting=true` is set during `dotnet restore` to allow building Windows-targeted projects.
