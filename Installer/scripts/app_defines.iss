// filepath: c:\Users\Antoine\source\repos\SoundSwitch\Installer\scripts\app_defines.iss
// SoundSwitch global application definitions
// Copyright © 2010-2025 SoundSwitch

#ifndef appDefinesIss
#define appDefinesIss 1

#define MyAppSetupName 'SoundSwitch'
#define ExeDir  '..\Final\'
#define WinX64Exe ExeDir + 'win-x64\SoundSwitch.exe'
#define WinArm64Exe ExeDir + 'win-arm64\SoundSwitch.exe'
#if FileExists(WinX64Exe)
  #define MyAppVersion GetVersionNumbersString(WinX64Exe)
#elif FileExists(WinArm64Exe)
  #define MyAppVersion GetVersionNumbersString(WinArm64Exe)
#else
  #error "No published SoundSwitch.exe found in Final\win-x64 or Final\win-arm64."
#endif
#define MyAppDescription 'SoundSwitch is a powerful audio switching application.'

#endif
