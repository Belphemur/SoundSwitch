// SoundSwitch setup lifecycle utility functions
// Copyright © 2010-2025 SoundSwitch

#ifndef setupUtilsIss
#define setupUtilsIss

[Code]
// Called during setup process to handle PATH modifications
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Add to PATH if the user selected that option
    if IsTaskSelected('addtopath') then
    begin
      // Add to appropriate PATH based on installation privileges
      if IsAdminLoggedOn then
        // System PATH
        ModifyPath(ExpandConstant('{app}'), 1) 
      else
        // User PATH
        ModifyPath(ExpandConstant('{app}'), 0);
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
  FileCopy(logfilepathname, newfilepathname, false);
  
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