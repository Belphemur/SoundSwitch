// SoundSwitch setup lifecycle utility functions
// Copyright © 2010-2025 SoundSwitch

#ifndef setupUtilsIss
#define setupUtilsIss

[Code]
// Called during setup process to handle PATH modifications and runtime installation
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Add to PATH if the user selected that option
    if WizardIsTaskSelected('addtopath') then
    begin
      // Add to PATH (function now handles admin/user mode internally)
      ModifyPath(ExpandConstant('{app}'));
    end;
  end;
end;

// Handle cleanup when setup is finished
procedure DeinitializeSetup();
var
  logfilepathname, logfilename, newfilepathname: string;
  setupExe: string;
  tempPath: string;
  isTempPath: Boolean;
begin
  // Handle log file copying
  logfilepathname := ExpandConstant('{log}');
  logfilename := ExtractFileName(logfilepathname);
  newfilepathname := ExpandConstant('{tmp}\..') + logfilename;
  CopyFile(logfilepathname, newfilepathname, false);
  
  // Check if installer is running from temp folder and delete if true
  setupExe := ExpandConstant('{srcexe}');
  tempPath := GetTempDir;
  isTempPath := Pos(LowerCase(tempPath), LowerCase(ExtractFilePath(setupExe))) = 1;
  
  if isTempPath then
  begin
    // Delete installer executable after 5 seconds
    Sleep(5000);
    DeleteFile(setupExe);
  end;
end;

<event('InitializeWizard')>
procedure ConfigureDotNetDependency;
begin
  Log('Configuring .NET Desktop Runtime dependency for DotNetMajorVersion={#DotNetMajorVersion}.');
#if DotNetMajorVersion == "10"
  Log('Registering .NET 10 Desktop Runtime dependency package.');
  Dependency_AddDotNet100Desktop;
#elif DotNetMajorVersion == "9"
  Log('Registering .NET 9 Desktop Runtime dependency package.');
  Dependency_AddDotNet90Desktop;
#elif DotNetMajorVersion == "8"
  Log('Registering .NET 8 Desktop Runtime dependency package.');
  Dependency_AddDotNet80Desktop;
#else
  #error "Unsupported .NET version: " + DotNetMajorVersion
#endif
  Log('.NET Desktop Runtime dependency configuration complete.');
end;

<event('PrepareToInstall')>
function MyPrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Log('Starting installed .NET Desktop Runtime cleanup for DotNetMajorVersion={#DotNetMajorVersion}.');
#if DotNetMajorVersion == "10"
  Log('Removing .NET 10 Desktop Runtime versions older than 10.0.7 for architecture "' + Dependency_ArchTitle + '".');
  UninstallOlderDotNetRuntimes(10, 0, 7, Dependency_ArchTitle);
#elif DotNetMajorVersion == "9"
  Log('Removing .NET 9 Desktop Runtime versions older than 9.0.15 for architecture "' + Dependency_ArchTitle + '".');
  UninstallOlderDotNetRuntimes(9, 0, 15, Dependency_ArchTitle);
#elif DotNetMajorVersion == "8"
  Log('Removing .NET 8 Desktop Runtime versions older than 8.0.26 for architecture "' + Dependency_ArchTitle + '".');
  UninstallOlderDotNetRuntimes(8, 0, 26, Dependency_ArchTitle);
#else
  #error "Unsupported .NET version: " + DotNetMajorVersion
#endif
  Log('Completed installed .NET Desktop Runtime cleanup.');
  Result := '';
end;
#endif
