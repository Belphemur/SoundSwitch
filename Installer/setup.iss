;contribute on github.com/stfx/innodependencyinstaller or codeproject.com/Articles/20868/NET-Framework-1-1-2-0-3-5-Installer-for-InnoSetup

;comment out product defines to disable installing them
;#define use_iis
#define use_kb835732
#define use_msi45
#define use_msiproduct

;#define use_dotnet_core '5.0.4'

;#define use_vc2017

#define MyAppSetupName 'SoundSwitch'
#define ExeDir  '..\Final\'
#define MyAppVersion GetFileVersion('..\Final\SoundSwitch.exe')

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
SolidCompression=yes
CloseApplications=yes
Compression=lzma2/ultra64
LZMANumBlockThreads=2

//SignTool=SoundSwitch
SignTool=Certum

SignedUninstaller=yes
LicenseFile={#ExeDir}LICENSE.txt
InfoBeforeFile={#ExeDir}Terms.txt
SetupLogging=yes
RestartApplications=no
;AppMutex={#MyAppSetupName}

;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
MinVersion=6.1.7601
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline dialog
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

;Downloading and installing dependencies will only work if the memo/ready page is enabled (default behaviour)
DisableReadyPage=no
DisableReadyMemo=no
Uninstallable=yes
CreateUninstallRegKey=yes

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "pt_br"; MessagesFile: "compiler:Languages\brazilianPortuguese.isl"
Name: "ru_ru"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "pl_pl"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "nl"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "zh"; MessagesFile: "Languages\ChineseSimplified.isl"
Name: "ko"; MessagesFile: "Languages\Korean.isl"


[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: deletefiles; Description: "{cm:ExistingSettings}"; Flags: unchecked checkedonce

[Files] 
Source: "{#ExeDir}SoundSwitch.exe"; DestDir: "{app}";  Flags: signonce ignoreversion
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

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2

en.ExistingSettings=Remove any existing settings
fr.ExistingSettings=Supprimer les paramètres existants
de.ExistingSettings=Alle vorhandenen Einstellungen löschen
es.ExistingSettings=Elimiar cualquier configuración existente
it.ExistingSettings=Rimuovi impostazioni esistenti
pt_br.ExistingSettings=Remover configurações já existentes

en.UninstallQuestion=Do you want to remove {#MyAppSetupName}'s settings?
fr.UninstallQuestion=Voulez-vous aussi supprimer les paramètres de {#MyAppSetupName} ?
de.UninstallQuestion=Sollen deine {#MyAppSetupName} Einstellungen gelöscht werden?
es.UninstallQuestion=¿Quieres eliminar la configuración de {#MyAppSetupName}?
it.UninstallQuestion=Vuoi rimuovere le impostazioni di {#MyAppSetupName}?
pt_br.UninstallQuestion=Deseja remover as configurações do {#MyAppSetupName}?

en.CertificatesGroup=Certificates:
fr.CertificatesGroup=Certificats:
de.CertificatesGroup=Zertifikate:
es.CertificatesGroup=Certificados:
it.CertificatesGroup=Certificati:
pt_br.CertificatesGroup=Certificados:

en.ViewReadmeFile=View the README file
it.ViewReadmeFile=Visualizza file README
pt_br.ViewReadmeFile=Visualizar o arquivo README

en.SupportTheProject=Support the project
it.SupportTheProject=Supporta il progetto
pt_br.SupportTheProject=Apoiar o projeto

en.ViewChangelogFile=View the CHANGELOG file
it.ViewChangelogFile=Visualizza file CHANGELOG
pt_br.ViewChangelogFile=Visualizar o CHANGELOG

[UninstallRun]
Filename: "certutil.exe"; Parameters: "-delstore ""Root"" ""eb db 8a 0a 72 a6 02 91 40 74 9e a2 af 63 d2 fc""" ; Flags: runhidden runascurrentuser
Filename: "certutil.exe"; Parameters: "-delstore ""TrustedPublisher"" ""942A37BCA9A9889442F6710533CB5548""" ; Flags: runhidden runascurrentuser


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
[Code]
#include "scripts\checkMutex.iss"

function CmdLineParamExists(const Value: string): Boolean;
var
  I: Integer;  
begin
  Result := False;
  for I := 1 to ParamCount do
    if CompareText(ParamStr(I), Value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

function ShowDonate: Boolean;
begin
  Result := not CmdLineParamExists('/NODONATE');
end;

function InitializeUninstall(): Boolean;
begin
  Result := PromptUntilProgramClosedOrInstallationCanceled('{#MyAppSetupName}');

  if not Result then
  begin
    MsgBox( 'The uninstallation process was canceled.', mbInformation, MB_OK );
  end;  
end;


procedure CurUninstallStepChanged (CurUninstallStep: TUninstallStep);
var
  mres : integer;
begin
  case CurUninstallStep of
    usPostUninstall:
      begin
        mres := MsgBox(ExpandConstant('{cm:UninstallQuestion}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2)
        if mres = IDYES then
          DelTree(ExpandConstant('{userappdata}\SoundSwitch'), True, True, True);
      end;  
  end;
end;

procedure DeinitializeSetup();
var
  logfilepathname, logfilename, newfilepathname: string;
begin
  logfilepathname := ExpandConstant('{log}');
  logfilename := ExtractFileName(logfilepathname);
  // Set the new target path as the directory where the installer is being run from
  newfilepathname := ExpandConstant('{tmp}\..') + logfilename;

  FileCopy(logfilepathname, newfilepathname, false);
end; 
// shared code for installing the products
#include "scripts\products.iss"
// helper functions
#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"
#include "scripts\products\vcredistversion.iss"

// actual products
#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif

#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#ifdef use_dotnetfx11lp
#include "scripts\products\dotnetfx11lp.iss"
#endif
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
//#include "scripts\products\dotnetfx35.iss"
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
//#include "scripts\products\dotnetfx35lp.iss"
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif

#ifdef use_dotnetfx45
#include "scripts\products\dotnetfx45.iss"
#endif

#ifdef use_dotnetfx46
#include "scripts\products\dotnetfx46.iss"
#endif

#ifdef use_dotnetfx47
#include "scripts\products\dotnetfx47.iss"
#endif

#ifdef use_dotnet_core
#include "scripts\products\dotnet-core.iss"
#endif

#ifdef use_wic
#include "scripts\products\wic.iss"
#endif

#ifdef use_msiproduct
#include "scripts\products\msiproduct.iss"
#endif
#ifdef use_vc2005
#include "scripts\products\vcredist2005.iss"
#endif
#ifdef use_vc2008
#include "scripts\products\vcredist2008.iss"
#endif
#ifdef use_vc2010
#include "scripts\products\vcredist2010.iss"
#endif
#ifdef use_vc2012
#include "scripts\products\vcredist2012.iss"
#endif
#ifdef use_vc2013
#include "scripts\products\vcredist2013.iss"
#endif
#ifdef use_vc2015
#include "scripts\products\vcredist2015.iss"
#endif
#ifdef use_vc2017
#include "scripts\products\vcredist2017.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif

#ifdef use_sqlcompact35sp2
#include "scripts\products\sqlcompact35sp2.iss"
#endif

#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif


function InitializeSetup(): boolean;
begin
	// initialize windows version
	initwinversion();

#ifdef use_iis
	if (not iis()) then exit;
#endif

#ifdef use_msi20
	msi20('2.0'); // min allowed version is 2.0
#endif
#ifdef use_msi31
	msi31('3.1'); // min allowed version is 3.1
#endif
#ifdef use_msi45
	msi45('4.5'); // min allowed version is 4.5
#endif
#ifdef use_ie6
	ie6('5.0.2919'); // min allowed version is 5.0.2919
#endif

#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif

	// install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	// check if .netfx 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['2000', '3'])]), mberror, mb_ok);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['XP', '2'])]), mberror, mb_ok);
		exit;
	end;

	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

#ifdef use_dotnetfx35
	//dotnetfx35();
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	//dotnetfx35lp();
	dotnetfx35sp1lp();
#endif
#endif

#ifdef use_wic
	wic();
#endif

	// if no .netfx 4.0 is found, install the client (smallest)
#ifdef use_dotnetfx40
	if (not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then
		dotnetfx40client();
#endif

#ifdef use_dotnetfx45
    //dotnetfx45(2); // min allowed version is 4.5.2
    dotnetfx45(0); // min allowed version is 4.5.0
#endif

#ifdef use_dotnetfx46
    dotnetfx46(2); // min allowed version is 4.6.2
#endif


#ifdef use_dotnetfx47
    dotnetfx47(2); // min allowed version is 4.7.2
#endif

#ifdef use_dotnet_core
    dotnetCore('{#use_dotnet_core}');
#endif

#ifdef use_vc2005
	vcredist2005();
#endif
#ifdef use_vc2008
	vcredist2008();
#endif
#ifdef use_vc2010
	vcredist2010();
#endif
#ifdef use_vc2012
	vcredist2012();
#endif
#ifdef use_vc2013
	vcredist2013();
#endif
#ifdef use_vc2015
	vcredist2015();
#endif
#ifdef use_vc2017
	vcredist2017();
#endif

#ifdef use_mdac28
	mdac28('2.7'); // min allowed version is 2.7
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015'); // min allowed version is 4.0.8015
#endif

#ifdef use_sqlcompact35sp2
	sqlcompact35sp2();
#endif

#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif
	Result := true;
end;

procedure InitializeWizard();
begin
  idpDownloadAfter(wpReady); 
end;
