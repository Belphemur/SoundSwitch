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
    
    // Ensure .NET {#DotNetMajorVersion} Desktop Runtime is installed
    if not IsDotNetDesktopRuntime10Installed() then
    begin
      Log('Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.x not found. Installing via winget...');
      if not InstallDotNetDesktopRuntime10() then
      begin
        Log('Failed to install .NET {#DotNetMajorVersion} Desktop Runtime.');
      end;
    end
    else
    begin
      Log('Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.x is already installed.');
    end;

    // Always try to upgrade to latest runtime version (non-fatal)
    if IsDotNetDesktopRuntime10Installed() then
    begin
      UpgradeDotNetDesktopRuntime10();
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
#endif