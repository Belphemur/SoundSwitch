// SoundSwitch uninstallation utility functions
// Copyright © 2010-2025 SoundSwitch

#include "checkMutex.iss"

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
        if IsAdminLoggedOn then
          RemoveFromPath(appDir, 1)
        else
          RemoveFromPath(appDir, 0);
      end;
    usPostUninstall:
      begin
        // Always delete program files when running as admin
        if IsAdminLoggedOn then
        begin
          DelTree(ExpandConstant('{commonpf}\{#MyAppSetupName}'), True, True, True);
          DelTree(ExpandConstant('{userpf}\{#MyAppSetupName}'), True, True, True);
        end;
        
        // Ask about deleting settings
        mres := MsgBox(ExpandConstant('{cm:UninstallQuestion}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2);
        if mres = IDYES then
        begin
          DelTree(ExpandConstant('{userappdata}\SoundSwitch'), True, True, True);
        end;
      end;  
  end;
end;
#endif