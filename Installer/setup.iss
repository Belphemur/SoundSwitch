; SoundSwitch Installer
; Copyright © 2010-2025 SoundSwitch
; Main installer script that includes all component scripts

#define MyAppName "SoundSwitch"
#define MyAppSetupName "SoundSwitch"
#define MyAppPublisher "Antoine Aflalo"
#define MyAppCopyright "Antoine Aflalo"
#define MyAppURL "https://soundswitch.aaflalo.me/"
#define MyAppExeName "SoundSwitch.exe"

; Get version from file
#define FileHandle
#define FileLine
#define MyAppVersion "1.0.0.0"
#define public Dependency_NoExternalPath
#define AppId "{{2DEF0022-7605-408D-B7FF-118DD098A7BD}"

;#include "scripts\products.iss"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; Every update should have same AppId but a new version.
AppId={#AppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppCopyright={#MyAppCopyright}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppSetupName}
AllowNoIcons=yes
LicenseFile=..\LICENSE.txt
OutputDir=.
OutputBaseFilename={#MyAppSetupName}_{#MyAppVersion}_Installer
Compression=lzma2/ultra64
SolidCompression=yes
SetupIconFile=..\SoundSwitch\Resources\arrow_switch.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppSetupName}
DisableProgramGroupPage=no
DisableDirPage=no
DisableReadyPage=no
DisableFinishedPage=no
DisableWelcomePage=no
MinVersion=6.1
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x86 x64
ChangesEnvironment=yes

; Include theme configuration
#include "scripts\theme.iss"

; Include all the installer sections
#include "scripts\languages.iss"
#include "scripts\setup_sections.iss"
;#include "scripts\checkMutex.iss"

; Code sections
#include "scripts\utility_functions.iss"
#include "scripts\path_operations.iss"
#include "scripts\installer_events.iss"
#include "scripts\installer_ui.iss"
