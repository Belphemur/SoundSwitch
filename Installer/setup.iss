#include "scripts\app_defines.iss"
// ReleaseState is expected to be defined through the command line with /D parameter
// e.g. iscc /DReleaseState=Beta setup.iss


[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright=Copyright © 2010-2025 {#MyAppSetupName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=SoundSwitch                                            
AppPublisher=Antoine Aflalo
AppPublisherURL=https://soundswitch.aaflalo.me
AppSupportURL=https://github.com/Belphemur/SoundSwitch
AppUpdatesURL=https://github.com/Belphemur/SoundSwitch/releases
OutputBaseFilename={#MyAppSetupName}_v{#MyAppVersion}_{#ReleaseState}_Installer
DefaultGroupName={#MyAppSetupName}
DefaultDirName={autopf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\SoundSwitch.exe
OutputDir={#ExeDir}
SourceDir=.
AllowNoIcons=yes
SetupIconFile="..\SoundSwitch\Resources\Switch-SoundWave.ico"
SolidCompression=no
CloseApplications=yes
Compression=lzma2/ultra64
LZMANumBlockThreads=3
ChangesEnvironment=yes
WizardStyle=modern
WizardResizable=yes
WizardSizePercent=120
WindowStartMaximized=yes
WindowVisible=yes
WizardImageFile="logo.bmp"
WizardSmallImageFile="logo.bmp"
WizardImageStretch=no

//SignTool=SoundSwitch
SignTool=Certum

SignedUninstaller=yes
LicenseFile={#ExeDir}LICENSE.txt
InfoBeforeFile={#ExeDir}Terms.txt
SetupLogging=yes
RestartApplications=no
;AppMutex={#MyAppSetupName}

;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
MinVersion=6.1.7601sp1
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

;Downloading and installing dependencies will only work if the memo/ready page is enabled (default behaviour)
DisableReadyPage=no
DisableReadyMemo=no
Uninstallable=yes
CreateUninstallRegKey=yes

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl,Languages\en_US.iss"
Name: "de"; MessagesFile: "compiler:Languages\German.isl,Languages\de_DE.iss"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl,Languages\fr_FR.iss"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl,Languages\es_ES.iss"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl,Languages\it_IT.iss"
Name: "pt_br"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl,Languages\pt_BR.iss"
Name: "ru_ru"; MessagesFile: "compiler:Languages\Russian.isl,Languages\ru_RU.iss"
Name: "pl_pl"; MessagesFile: "compiler:Languages\Polish.isl,Languages\pl_PL.iss"
Name: "nl"; MessagesFile: "compiler:Languages\Dutch.isl,Languages\nl_NL.iss"
Name: "zh"; MessagesFile: "Languages\ChineseSimplified.isl,Languages\zh_CN.iss"
Name: "ko"; MessagesFile: "Languages\Korean.isl,Languages\ko_KR.iss"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Check: not IsVerySilent
Name: deletefiles; Description: "{cm:ExistingSettings}"; Flags: unchecked checkedonce
Name: addtopath; Description: "{cm:AddToPath}"; GroupDescription: "{cm:CLIOptions}"; Flags: checkedonce

[Files] 
Source: "{#ExeDir}SoundSwitch.exe"; DestDir: "{app}";  Flags: signonce ignoreversion
Source: "{#ExeDir}SoundSwitch.CLI.exe"; DestDir: "{app}";  Flags: signonce ignoreversion
Source: "{#ExeDir}*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion;

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run\{#MyAppSetupName}"; Flags: uninsdeletekey

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\SoundSwitch.exe"; AppUserModelID: aaflalo.{#MyAppSetupName}.Application
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{userdesktop}\{#MyAppSetupName}"; Filename: "{app}\SoundSwitch.exe"; Tasks: desktopicon; AppUserModelID: aaflalo.{#MyAppSetupName}.Application

[Run]
Filename: "{app}\SoundSwitch.exe"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: nowait postinstall;
Filename: "{app}\Readme.html"; Description: "{cm:ViewReadmeFile}"; Flags: postinstall shellexec skipifsilent
Filename: "https://soundswitch.aaflalo.me/?utm_source={#MyAppVersion}&utm_campaign=installer#donate"; Description: "{cm:SupportTheProject}"; Flags: postinstall shellexec runasoriginaluser; Check: ShowDonate
Filename: "{app}\Changelog.html"; Description: "{cm:ViewChangelogFile}"; Flags: postinstall shellexec skipifsilent unchecked

[UninstallRun]
Filename: "certutil.exe"; Parameters: "-delstore ""Root"" ""eb db 8a 0a 72 a6 02 91 40 74 9e a2 af 63 d2 fc""" ; Flags: runhidden runascurrentuser; RunOnceId: "DeleteRootCert"
Filename: "certutil.exe"; Parameters: "-delstore ""TrustedPublisher"" ""942A37BCA9A9889442F6710533CB5548""" ; Flags: runhidden runascurrentuser; RunOnceId: "DeletePublisherCert"

[InstallDelete]
Type: filesandordirs; Name: {userappdata}\SoundSwitch; Tasks: deletefiles
Type: files; Name: {app}\Audio.EndPoint.Controller.Wrapper.*
Type: files; Name: {app}\AudioEndPointLibrary.*
Type: files; Name: {app}\TracerX-Logger.*
Type: files; Name: {app}\System.*.dll
Type: files; Name: {app}\Audio.Default.Switcher.Wrapper.*
Type: files; Name: {app}\AudioDefaultSwitcher.*
Type: files; Name: {app}\Microsoft.*.dll
Type: files; Name: {app}\Microsoft.*.pdb
Type: files; Name: {app}\CommonMark.dll
Type: files; Name: {app}\CommonMark.pdb
Type: files; Name: {app}\Serilog*.dll
Type: files; Name: {app}\SoundSwitch.UI.UserControls.*
Type: files; Name: {app}\SoundSwitch.InterProcess.Communication.*
Type: files; Name: {app}\Microsoft.WindowsAPICodePack.*

#include "scripts\path_operations.iss"
#include "scripts\command_line_utils.iss"
#include "scripts\setup_utils.iss"
#include "scripts\uninstall_utils.iss"


