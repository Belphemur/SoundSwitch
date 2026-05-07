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
  if '{#DotNetMajorVersion}' = '10' then
  begin
    UninstallOlderDotNetRuntimes(10, 0, 7);
    Dependency_AddDotNet100Desktop;
  end
  else if '{#DotNetMajorVersion}' = '9' then
  begin
    UninstallOlderDotNetRuntimes(9, 0, 15);
    Dependency_AddDotNet90Desktop;
  end
  else if '{#DotNetMajorVersion}' = '8' then
  begin
    UninstallOlderDotNetRuntimes(8, 0, 26);
    Dependency_AddDotNet80Desktop;
  end
  else
  begin
    MsgBox('Unsupported .NET version: {#DotNetMajorVersion}', mbError, MB_OK);
  end;
end;
#endif