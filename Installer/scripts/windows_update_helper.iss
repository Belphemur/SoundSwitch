#ifndef WINDOWS_UPDATE_HELPER_ISS
#define WINDOWS_UPDATE_HELPER_ISS

[Code]
const
  KBID_TO_CHECK = 'KB5053606';
  LTSC_PRODUCT_NAME_1 = 'Windows 10 Enterprise LTSC';
  LTSC_PRODUCT_NAME_2 = 'Windows 10 Enterprise LTSB'; // Older naming

var
  PowerShellExePath_WU: String; // Added _WU suffix to avoid potential global name clashes

procedure LogMsg_WU(Msg: String); // Added _WU suffix
begin
  Log(GetDateTimeString('yyyy-mm-dd hh:nn:ss', '-', ':') + ' [WUHelper]: ' + Msg);
end;

function IsWin10LTSC_WU(): Boolean; // Added _WU suffix
var
  ProductName: String;
begin
  Result := False; // Default to False
  if RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\Windows NT\CurrentVersion', 'ProductName', ProductName) then
  begin
    LogMsg_WU('Detected OS ProductName: ' + ProductName);
    if (Pos(LTSC_PRODUCT_NAME_1, ProductName) > 0) or (Pos(LTSC_PRODUCT_NAME_2, ProductName) > 0) then
    begin
      LogMsg_WU('System is Windows 10 LTSC.');
      Result := True;
    end
    else
    begin
      LogMsg_WU('System is not Windows 10 LTSC.');
    end;
  end
  else
  begin
    LogMsg_WU('Error: Could not read ProductName from registry.');
  end;
end;

function FindPowerShell_WU(): String; // Added _WU suffix
var
  SystemDir: String;
  TestPath: String;
begin
  Result := '';
  SystemDir := ExpandConstant('{sys}');
  TestPath := SystemDir + '\WindowsPowerShell\v1.0\powershell.exe';
  if FileExists(TestPath) then
  begin
    Result := TestPath;
    LogMsg_WU('Found PowerShell at: ' + Result);
    Exit;
  end;
  
  // Fallback to PATH
  LogMsg_WU('PowerShell not found in explicit system path, will rely on PATH for powershell.exe');
  Result := 'powershell.exe'; 
end;

function IsKBInstalledPS_WU(KBID: String): Boolean; // Added _WU suffix
var
  ResultCode: Integer;
  Cmd: String;
begin
  Result := False; 
  if PowerShellExePath_WU = '' then
  begin
    LogMsg_WU('PowerShell executable path not determined. Cannot check KB.');
    Exit;
  end;

  Cmd := AddQuotes(PowerShellExePath_WU) + ' -NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}') + '\ManageWindowsUpdate.ps1" -KBArticleID "' + KBID + '" -Action CheckInstalled';
  LogMsg_WU('Executing: ' + Cmd);
  
  if Exec(Cmd, '', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    LogMsg_WU('PowerShell script (CheckInstalled) executed. Exit Code: ' + IntToStr(ResultCode));
    if ResultCode = 0 then // 0 means installed
    begin
      Result := True;
    end
    else if ResultCode = 1 then // 1 means not installed
    begin
      Result := False; 
    end
    else 
    begin
      LogMsg_WU('PowerShell script (CheckInstalled) for ' + KBID + ' returned an error or unexpected code: ' + IntToStr(ResultCode));
      Result := False; 
    end;
  end
  else
  begin
    LogMsg_WU('Failed to execute PowerShell script (CheckInstalled) for ' + KBID + '. Last Error Code (from Exec): ' + IntToStr(ResultCode));
    Result := False;
  end;
end;

procedure AttemptKBInstallPS_WU(KBID: String); // Added _WU suffix
var
  ResultCode: Integer; 
  Cmd: String;
begin
  if PowerShellExePath_WU = '' then
  begin
    LogMsg_WU('PowerShell executable path not determined. Cannot attempt KB install.');
    Exit;
  end;

  Cmd := AddQuotes(PowerShellExePath_WU) + ' -NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}') + '\ManageWindowsUpdate.ps1" -KBArticleID "' + KBID + '" -Action TriggerInstall';
  LogMsg_WU('Attempting to trigger KB install (no wait): ' + Cmd);

  if Exec(Cmd, '', '', SW_HIDE, ewNoWait, ResultCode) then
  begin
    LogMsg_WU('PowerShell script (TriggerInstall) for ' + KBID + ' launched.');
  end
  else
  begin
    LogMsg_WU('Failed to launch PowerShell script (TriggerInstall) for ' + KBID + '. Last Error Code (from Exec): ' + IntToStr(ResultCode));
  end;
end;

// This InitializeSetup function will be called by Inno Setup.
// Ensure no other InitializeSetup exists or rename this one and call it from the main InitializeSetup.
// For this approach, we assume this IS the main InitializeSetup or it's called by it.
// If setup.iss already has an InitializeSetup, this needs to be integrated.
// For now, let's assume this will be the primary logic or called by the primary.

function InitializeSetup_WindowsUpdatePrerequisites(): Boolean;
begin
  PowerShellExePath_WU := FindPowerShell_WU();
  if PowerShellExePath_WU = 'powershell.exe' then // FindPowerShell_WU is relying on PATH
  begin
    LogMsg_WU('PowerShell.exe path determined as "powershell.exe" by FindPowerShell_WU, attempting to use system PATH.');
    // Explicitly test if 'powershell.exe' is resolvable by FileExists (which should check PATH)
    if not FileExists(PowerShellExePath_WU) then
    begin
      LogMsg_WU('Critical: PowerShell.exe (when relying on PATH) was not found by FileExists. Update checks will be skipped.');
      PowerShellExePath_WU := ''; // Mark as unusable
    end
    else
    begin
      LogMsg_WU('PowerShell.exe (via PATH) confirmed by FileExists.');
    end;
  end
  else if PowerShellExePath_WU <> '' then // This means FindPowerShell_WU returned an explicit, existing path
  begin
    // This is an explicit path. FindPowerShell_WU already confirmed FileExists.
    // Re-check in case the file got deleted since FindPowerShell_WU was called.
    if not FileExists(PowerShellExePath_WU) then
    begin
      LogMsg_WU('Critical: PowerShell.exe was found at explicit path "' + PowerShellExePath_WU + '" but is now missing. Update checks will be skipped.');
      PowerShellExePath_WU := ''; // Mark as unusable
    end;
    // If it's still present, FindPowerShell_WU already logged its discovery.
  end;

  LogMsg_WU('Starting Windows Update prerequisite check.');
  if IsWin10LTSC_WU() then
  begin
    if PowerShellExePath_WU <> '' then // Only proceed if PowerShell is found
    begin
        if not IsKBInstalledPS_WU(KBID_TO_CHECK) then
        begin
          AttemptKBInstallPS_WU(KBID_TO_CHECK);
        end
        else
        begin
          LogMsg_WU(KBID_TO_CHECK + ' is already installed, or an error occurred during check (details above).');
        end;
    end
    else
    begin
        LogMsg_WU('Skipping KB check/install for ' + KBID_TO_CHECK + ' because PowerShell was not found.');
    end;
  end
  else
  begin
    LogMsg_WU('System is not Windows 10 LTSC. Skipping update check for ' + KBID_TO_CHECK + '.');
  end;
  
  LogMsg_WU('Windows Update prerequisite check finished.');
  Result := True; // Always allow main installation to proceed
end;

#endif // WINDOWS_UPDATE_HELPER_ISS