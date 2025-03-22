// SoundSwitch installer events
// Copyright © 2010-2025 SoundSwitch

#include "utility_functions.iss"
#include "path_operations.iss"

[Code]
var
  AddToPathCheckBox: TNewCheckBox;
  CleanSettings: Boolean;
  CleanCheckBox: TNewCheckBox;

// Called during installer startup
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

// Called during uninstall initialization
function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
  MsgBoxResult: Integer;
begin
  // Ask if the user wants to delete the settings
  if not IsVerySilent() then
  begin
    MsgBoxResult := MsgBox(ExpandConstant('{cm:UninstallQuestion}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2);
    if MsgBoxResult = IDYES then
    begin
      // Delete registry entries
      RegDeleteKeyIncludingSubkeys(HKCU, 'Software\{#MyAppSetupName}');
    end;
  end;
  
  Result := True;
end;

// Called right after installation
procedure CurStepChanged(CurStep: TSetupStep);
var
  AppPath: string;
begin
  if CurStep = ssPostInstall then
  begin
    // Handle clean settings if checkbox was checked
    if CleanSettings then
    begin
      RegDeleteKeyIncludingSubkeys(HKCU, 'Software\{#MyAppSetupName}');
    end;

    // Add to PATH if option was selected
    if AddToPathCheckBox <> nil then
    begin
      AppPath := ExpandConstant('{app}');
      if AddToPathCheckBox.Checked then
      begin
        ModifyPath(AppPath, 0); // 0 for User PATH
      end;
    end;
  end;
end;

// Called during uninstallation
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppPath: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Remove from PATH during uninstallation
    AppPath := ExpandConstant('{app}');
    RemoveFromPath(AppPath, 0); // 0 for User PATH
  end;
end;