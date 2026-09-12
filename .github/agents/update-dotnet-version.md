# Skill: Update Required .NET Version

This skill guides GitHub Copilot and other agents on how to correctly and consistently update the required .NET version for the SoundSwitch application.

> **Note:** SoundSwitch installers are self-contained — they bundle the .NET
> Desktop Runtime inside the installer payload. The installer no longer checks
> for, downloads, or uninstalls any .NET runtime, and the
> `Installer/scripts/CodeDependencies.iss` /
> `Installer/scripts/uninstall_dotnet.iss` helper scripts were removed. A .NET
> version bump therefore only affects the SDK/TargetFramework of the projects
> and the FAQ documentation below.

## Scope & Target Files

When a request is made to bump the required .NET version (for example, from `10.0.8` to `10.0.9`), the following files must be updated:

1. **`Directory.Build.props` / project files**: Update `TargetFramework` (and any pinned SDK version in `global.json`) to the new .NET version.
2. **`website/src/faq/update-7-0-dotnet-required.md`**: Provides manual installation instructions and download links for the .NET Desktop Runtime for users still running an old (pre-self-contained) installer.

---

## Detailed Update Steps

### 1. Update the Target Framework

- Update `<TargetFramework>` in `SoundSwitch/SoundSwitch.csproj` (and the other projects sharing the framework) to `net<MAJOR>.<MINOR>`.
- If `global.json` pins an SDK version, update `sdk.version` accordingly.

### 2. Update User Documentation (`website/src/faq/update-7-0-dotnet-required.md`)

Open the FAQ markdown file and update the manual download links for .NET Desktop Runtime to match the newly required version:
- Locate the download links for `x64` and `arm64`.
- Update the version number in both the URL and the link text (e.g. from `10.0.8` to `10.0.9`).

---

## Verification & Validation

After making the updates, ensure that:
1. The solution successfully builds with `dotnet build SoundSwitch.sln -c Debug`.
2. All unit tests discover and pass successfully with `dotnet test SoundSwitch.Tests\SoundSwitch.Tests.csproj`.