// filepath: c:\Users\Antoine\source\repos\SoundSwitch\Installer\scripts\app_defines.iss
// SoundSwitch global application definitions
// Copyright © 2010-2025 SoundSwitch

// TargetArch (/DTargetArch=x64 or /DTargetArch=arm64) must be defined by the
// caller BEFORE this file is included; setup.iss validates it first.

#ifndef appDefinesIss
#define appDefinesIss 1

#define MyAppSetupName 'SoundSwitch'
#define ExeDir  '..\Final\'

#if TargetArch == "x64"
  // x64 keeps the legacy unsuffixed installer name; arm64 gains an _arm64
  // suffix. OutputBaseFilename appends {#InstallerArchSuffix}.
  #define InstallerArchSuffix ''
  #define ArchPayloadDir ExeDir + 'win-x64\'
  #define ArchPayloadExe ArchPayloadDir + 'SoundSwitch.exe'
#elif TargetArch == "arm64"
  #define InstallerArchSuffix '_arm64'
  #define ArchPayloadDir ExeDir + 'win-arm64\'
  #define ArchPayloadExe ArchPayloadDir + 'SoundSwitch.exe'
#else
  #error "TargetArch must be defined as 'x64' or 'arm64' (pass /DTargetArch=x64 or /DTargetArch=arm64 to ISCC)."
#endif

// The application version is read from the SELECTED architecture's exe only;
// each installer is compiled against exactly one arch payload.
#if FileExists(ArchPayloadExe)
  #define MyAppVersion GetVersionNumbersString(ArchPayloadExe)
#else
  #error "No published SoundSwitch.exe found in the selected arch payload directory (Final\win-x64 or Final\win-arm64)."
#endif
#define MyAppDescription 'SoundSwitch is a powerful audio switching application.'

#endif