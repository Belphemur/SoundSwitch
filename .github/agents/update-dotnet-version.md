# Skill: Update Required .NET Version

This skill guides GitHub Copilot and other agents on how to correctly and consistently update the required .NET version for the SoundSwitch application, both in the installer dependencies and the documentation.

## Scope & Target Files

When a request is made to bump the required .NET version (for example, from `10.0.8` to `10.0.9`), the following files must be updated:

1. **`Installer/scripts/CodeDependencies.iss`**: Configures the prerequisites that the installer will check for and download if missing.
2. **`Installer/scripts/setup_utils.iss`**: Defines the cleanup routines that uninstall older or incompatible .NET versions before installing the new one.
3. **`website/src/faq/update-7-0-dotnet-required.md`**: Provides manual installation instructions and download links for the .NET Desktop Runtime to users in the FAQ.

---

## Detailed Update Steps

### 1. Update Installer Dependencies (`Installer/scripts/CodeDependencies.iss`)
Locate the procedures responsible for registering .NET 10.0 dependencies:
- `Dependency_AddDotNet100` (Core Runtime)
- `Dependency_AddDotNet100Asp` (ASP.NET Core)
- `Dependency_AddDotNet100Desktop` (Desktop Runtime)

For each procedure, perform the following updates:
- Update the check in `Dependency_IsNetCoreInstalled` to check for the new version:
  ```pascal
  // e.g. change (..., 10, 0, 8) to (..., 10, 0, 9)
  if not Dependency_IsNetCoreInstalled('...', 10, 0, <PATCH_VERSION>) then
  ```
- Update the descriptive labels:
  ```pascal
  // e.g. change '.NET Runtime 10.0.8' to '.NET Runtime 10.0.9'
  ```
- Update the x86, x64, and arm64 download URLs to point to the new version's executables under `https://builds.dotnet.microsoft.com/`.

### 2. Update Cleanup Routines (`Installer/scripts/setup_utils.iss`)
Locate the cleanup blocks checking for older runtimes under the `#if DotNetMajorVersion == "10"` directive.
- Update the `Log` and the `UninstallOlderDotNetRuntimes` parameters to match the new version limit:
  ```pascal
  #if DotNetMajorVersion == "10"
    Log('Removing .NET 10 Desktop Runtime versions older than 10.0.<PATCH_VERSION> for architecture "' + Dependency_ArchTitle + '".');
    UninstallOlderDotNetRuntimes(10, 0, <PATCH_VERSION>, Dependency_ArchTitle);
  #endif
  ```

### 3. Update User Documentation (`website/src/faq/update-7-0-dotnet-required.md`)
Open the FAQ markdown file and update the manual download links for .NET Desktop Runtime to match the newly required version:
- Locate the download links for `x64` and `arm64`.
- Update the version number in both the URL and the link text (e.g. from `10.0.8` to `10.0.9`).

---

## Verification & Validation

After making the updates, ensure that:
1. The solution successfully builds with `dotnet build SoundSwitch.sln -c Debug`.
2. All unit tests discover and pass successfully with `dotnet test SoundSwitch.Tests\SoundSwitch.Tests.csproj`.
