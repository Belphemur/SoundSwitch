# Building SoundSwitch

## Prerequisites

- **Windows 10/11** (x64 or ARM64) — SoundSwitch is a Windows-only application.
- **.NET SDK 10.0** — The target framework is defined in `global.json`. Run `tools\Install-BuildTools.ps1` to install and auto-detect the correct version.
- **PowerShell 7+** — All build, signing, and release scripts require PowerShell 7 or later.
- **Git** — For source control.
- **GitHub CLI (`gh`)** — Required for release publishing workflows. Installed by `Install-BuildTools.ps1`.

## One-Time Setup

Run the setup script once to install all build prerequisites:

```powershell
.\tools\Install-BuildTools.ps1
```

This installs: .NET SDK, GitHub CLI, Inno Setup 6, Certum SimplySign Desktop (code signing), Python 3, and `signtool.exe`.

## Quick Build (from Visual Studio)

1. Open `SoundSwitch.sln` in Visual Studio 2022.
2. Select the desired configuration (Debug/Release) and platform (AnyCPU).
3. Build the solution using **Build → Build Solution**.

## Command-Line Build

### Build All Projects

```cmd
dotnet build SoundSwitch.sln -c Debug
```

### Publish (Release)

The recommended way to build a full release is through the PowerShell toolchain. For quick local builds, you can publish directly:

```powershell
# Populate the Final\ directory with published binaries
dotnet publish SoundSwitch.CLI\SoundSwitch.CLI.csproj -c Release -o Final
dotnet publish SoundSwitch\SoundSwitch.csproj -c Release -o Final
```

## Building an Installer

The build/release pipeline is orchestrated by PowerShell scripts in `tools/`. The legacy `Make.bat` script is replaced by this toolchain.

### Recommended: Full Release Workflow

`Publish-Release.ps1` orchestrates the complete release process:

```powershell
# Build from source and generate the installer (no publishing to GitHub)
.\tools\Publish-Release.ps1 -BuildFromSource -Configuration Release
```

This script:
1. Cleans and publishes all projects into `Final\`.
2. Converts Markdown documentation (`CHANGELOG.md`, `README.md`, `Terms.md`) to HTML via `tools\markdown_to_html.py`.
3. Bundles assets (images, CLI README, license, terms) into `Final\`.
4. Delegates to `Build-Installer.ps1` to compile and sign the Inno Setup installer.
5. When run without `-BuildFromSource`, also downloads from a draft GitHub release, uploads the signed installer, and publishes the release.

### Installer-Only Build

If you already have a populated `Final\` directory, run:

```powershell
.\tools\Build-Installer.ps1 -FinalDir .\Final -SkipSigning
```

For a signed build:

```powershell
.\tools\Build-Installer.ps1 -FinalDir .\Final
```

`Build-Installer.ps1` handles:
1. Signature validation of binaries in `Final\`.
2. Signing application binaries via `tools\Sign-Binary.ps1`.
3. Invoking Inno Setup (`ISCC.exe`) to compile the installer.
4. Signing the resulting installer.

### Publishing to GitHub Releases

The full release workflow with GitHub integration:

```powershell
# For stable releases
.\tools\Publish-Release.ps1 -Channel release

# For beta releases
.\tools\Publish-Release.ps1 -Channel beta
```

This downloads the draft release created by semantic-release, builds and signs the installer, uploads it, sets the release body from `CHANGELOG.md`, and publishes after confirmation.

## Running Tests

```cmd
dotnet test SoundSwitch.Tests\SoundSwitch.Tests.csproj
dotnet test SoundSwitch.Audio.Manager.Tests\SoundSwitch.Audio.Manager.Tests.csproj
```

Or test the entire solution:

```cmd
dotnet test SoundSwitch.sln -c Debug
```

## Project-Specific Build Commands

| Project | Command |
|---------|---------|
| Main application | `dotnet build SoundSwitch\SoundSwitch.csproj` |
| Audio Manager | `dotnet build SoundSwitch.Audio.Manager\SoundSwitch.Audio.Manager.csproj` |
| Shared utilities | `dotnet build SoundSwitch.Common\SoundSwitch.Common.csproj` |
| CLI | `dotnet build SoundSwitch.CLI\SoundSwitch.CLI.csproj` |
| IPC | `dotnet build SoundSwitch.IPC\SoundSwitch.IPC.csproj` |
| UI Menu | `dotnet build SoundSwitch.UI.Menu\SoundSwitch.UI.Menu.csproj` |
| UI User Controls | `dotnet build SoundSwitch.UI.UserControls\SoundSwitch.UI.UserControls.csproj` |

## Tools Reference

| Script | Purpose |
|--------|---------|
| `Install-BuildTools.ps1` | One-time setup: installs .NET SDK, GitHub CLI, Inno Setup, signtool, Python |
| `Publish-Release.ps1` | Full release orchestration: build, sign, package, upload, publish |
| `Build-Installer.ps1` | Compiles and signs the Inno Setup installer from a populated `Final\` directory |
| `Sign-Binary.ps1` | SHA-256 code signing with RFC 3161 timestamping |
| `markdown_to_html.py` | Converts Markdown files to standalone HTML documents |

## Build Artifacts

- **Debug**: Binaries land in each project's `bin/Debug/` directory with full debug symbols.
- **Release**: The `Final\` directory at the repository root contains published binaries, HTML documentation, and assets ready for packaging.
- **Installer**: The Inno Setup compiler outputs signed installers to `Final\Installer\`.

## CI/CD Pipeline

The repository uses GitHub Actions for continuous integration and deployment:

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `.NET` | Push/PR to `dev`, `master`, `beta` | Builds, runs tests, validates translations, and lints code. |
| `release` | Push to `master`, `beta` | Runs `semantic-release` to determine version, builds, creates installer, and publishes to GitHub Releases. |
| `nightly` | Scheduled (daily) | Builds and publishes pre-release nightly builds. |
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

SoundSwitch uses **Serilog** for structured logging. Logs are written to the application's data directory and can be exported via the **Troubleshooting** tab in the settings UI.

## Platform Support

| Platform | Architecture | Status |
|----------|-------------|--------|
| Windows | x64 | Primary |
| Windows | ARM64 | Supported (7.0+) |

Cross-platform builds are not supported; SoundSwitch relies on Windows-specific APIs (Core Audio API, WinForms, WinAPI hooks).
