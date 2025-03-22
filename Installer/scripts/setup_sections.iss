// SoundSwitch installer setup sections
// Copyright © 2010-2025 SoundSwitch

#ifndef setupSectionIss
#define setupSectionIss

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; OnlyBelowVersion: 0,6.1

[Files]
Source: "..\Final\SoundSwitch.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Final\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\README.md"; DestDir: "{app}"; DestName: "README.txt"; Flags: ignoreversion
Source: "..\README.de.md"; DestDir: "{app}"; DestName: "README.de.txt"; Flags: ignoreversion
Source: "..\CHANGELOG.md"; DestDir: "{app}"; DestName: "CHANGELOG.txt"; Flags: ignoreversion
Source: "..\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppSetupName}"; Filename: "{app}\SoundSwitch.exe"
Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\SoundSwitch.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppSetupName}"; Filename: "{app}\SoundSwitch.exe"; Tasks: quicklaunchicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "SoundSwitch"; ValueData: """{app}\SoundSwitch.exe"" /startup"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\SoundSwitch"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\SoundSwitch"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".sws"; ValueType: string; ValueName: ""; ValueData: "SoundSwitch.Profile"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "SoundSwitch.Profile"; ValueType: string; ValueName: ""; ValueData: "SoundSwitch Profile"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SoundSwitch.Profile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\SoundSwitch.exe,0"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SoundSwitch.Profile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\SoundSwitch.exe"" ""%1"""; Flags: uninsdeletekey

[Run]
Filename: "{app}\SoundSwitch.exe"; Description: "{cm:LaunchProgram,{#StringChange(MyAppSetupName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\README.txt"; Description: "{cm:ViewReadmeFile}"; Flags: postinstall shellexec skipifsilent
Filename: "{app}\CHANGELOG.txt"; Description: "{cm:ViewChangelogFile}"; Flags: postinstall shellexec skipifsilent

#endif