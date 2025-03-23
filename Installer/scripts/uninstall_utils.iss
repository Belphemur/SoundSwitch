// SoundSwitch uninstallation utility functions
// Copyright © 2010-2025 SoundSwitch

#include "checkMutex.iss"
#include "app_defines.iss"

#ifndef uninstallUtilsIss
#define uninstallUtilsIss

[Code]
// Called before uninstallation starts to check if the application is running
function InitializeUninstall(): Boolean;
begin
  Result := PromptUntilProgramClosedOrInstallationCanceled('{#MyAppSetupName}');

  if not Result then
  begin
    MsgBox('The uninstallation process was canceled.', mbInformation, MB_OK);
  end;  
end;

// Called during uninstallation process
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  mres: integer;
  appDir: string;
begin
  case CurUninstallStep of
    usUninstall:
      begin
        // Save the app directory for later
        appDir := ExpandConstant('{app}');
        
        // Remove from PATH
        RemoveFromPath(appDir);
      end;
    usPostUninstall:
      begin
        // Always delete program files when running as admin
        if IsAdminInstallMode then
        begin
          DelTree(ExpandConstant('{commonpf}\{#MyAppSetupName}'), True, True, True);
          DelTree(ExpandConstant('{userpf}\{#MyAppSetupName}'), True, True, True);
        end;
        
        // Ask about deleting settings
        mres := MsgBox(ExpandConstant('{cm:UninstallQuestion,{#MyAppSetupName}}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2);
        if mres = IDYES then
        begin
          DelTree(ExpandConstant('{userappdata}\{#MyAppSetupName}'), True, True, True);
        end;
      end;  
  end;
end;
#endif