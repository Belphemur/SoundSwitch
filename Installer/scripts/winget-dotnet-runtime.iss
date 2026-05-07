[Code]
var
  _dotnetRebootRequired: Boolean;

// Checks if Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.x is already installed
function IsDotNetDesktopRuntimeInstalled(): Boolean;
var
  FindRec: TFindRec;
  SharedDir: String;
begin
  Result := false;

  // Check machine-wide install: look for any {#DotNetMajorVersion}.0.* directory
  SharedDir := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App\');
  if FindFirst(SharedDir + '*', FindRec) then
  begin
    repeat
      if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0) and
         (Pos('{#DotNetMajorVersion}.0', FindRec.Name) = 1) then
      begin
        Result := true;
        FindClose(FindRec);
        exit;
      end;
    until not FindNext(FindRec);
    FindClose(FindRec);
  end;

  // Check per-user install
  SharedDir := ExpandConstant('{localappdata}\Microsoft\dotnet\shared\Microsoft.WindowsDesktop.App\');
  if FindFirst(SharedDir + '*', FindRec) then
  begin
    repeat
      if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0) and
         (Pos('{#DotNetMajorVersion}.0', FindRec.Name) = 1) then
      begin
        Result := true;
        FindClose(FindRec);
        exit;
      end;
    until not FindNext(FindRec);
    FindClose(FindRec);
  end;
end;

// Installs .NET {#DotNetMajorVersion} Desktop Runtime via winget
// Returns True on success, False on failure (shows error message)
function InstallDotNetDesktopRuntime(): Boolean;
var
  Scope: String;
  Args: String;
  ResultCode: Integer;
  WingetPath: String;
begin
  Result := false;

  // .NET Desktop Runtime is system-wide only. The installer already
  // requires admin privileges (PrivilegesRequired=admin in setup.iss).
  Scope := 'machine';

  // Find winget: search PATH first, then known location
  WingetPath := FileSearch('winget.exe', GetEnv('PATH'));
  if WingetPath = '' then
    WingetPath := ExpandConstant('{localappdata}\Microsoft\WindowsApps\winget.exe');

  if WingetPath = '' then
  begin
    MsgBox('Windows Package Manager (winget) is not available on this system.' + #13#10 +
           'Please install .NET {#DotNetMajorVersion} Desktop Runtime manually from:' + #13#10 +
           'https://dotnet.microsoft.com/en-us/download/dotnet', mbError, MB_OK);
    Result := false;
    exit;
  end;

  // Build winget arguments
  Args := 'install --exact --id Microsoft.DotNet.DesktopRuntime.{#DotNetMajorVersion}' +
          ' --scope ' + Scope +
          ' --silent' +
          ' --accept-package-agreements' +
          ' --accept-source-agreements' +
          ' --source winget';

  Log('Running: ' + WingetPath + ' ' + Args);

  if Exec(WingetPath, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    case ResultCode of
      0: begin
        Log('.NET {#DotNetMajorVersion} Desktop Runtime installed successfully.');
        Result := true;
      end;
      3010: begin
        Log('.NET {#DotNetMajorVersion} Desktop Runtime installed (reboot required).');
        _dotnetRebootRequired := true;
        Result := true; // Success, but needs reboot later
      end;
      else begin
        Log('winget failed with exit code: ' + IntToStr(ResultCode));
        MsgBox('Failed to install .NET {#DotNetMajorVersion} Desktop Runtime (exit code: ' + IntToStr(ResultCode) + ').' + #13#10 +
               'Please install it manually from:' + #13#10 +
               'https://dotnet.microsoft.com/en-us/download/dotnet', mbError, MB_OK);
        Result := false;
      end;
    end;
  end
  else
  begin
    Log('Failed to execute winget.');
    MsgBox('Failed to run Windows Package Manager.' + #13#10 +
           'Please install .NET {#DotNetMajorVersion} Desktop Runtime manually from:' + #13#10 +
           'https://dotnet.microsoft.com/en-us/download/dotnet', mbError, MB_OK);
    Result := false;
  end;
end;

// Attempts to upgrade .NET {#DotNetMajorVersion} Desktop Runtime to latest version via winget
// Non-fatal: if nothing to upgrade or winget unavailable, just logs and continues
procedure UpgradeDotNetDesktopRuntime();
var
  Args: String;
  ResultCode: Integer;
  WingetPath: String;
begin
  WingetPath := FileSearch('winget.exe', GetEnv('PATH'));
  if WingetPath = '' then
    WingetPath := ExpandConstant('{localappdata}\Microsoft\WindowsApps\winget.exe');

  if WingetPath = '' then
  begin
    Log('winget not found, skipping runtime upgrade check.');
    exit;
  end;

  Args := 'upgrade --id Microsoft.DotNet.DesktopRuntime.{#DotNetMajorVersion}' +
          ' --silent' +
          ' --accept-package-agreements' +
          ' --accept-source-agreements' +
          ' --source winget';

  Log('Checking for .NET {#DotNetMajorVersion} Desktop Runtime updates: ' + WingetPath + ' ' + Args);

  if Exec(WingetPath, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    case ResultCode of
      0: Log('.NET {#DotNetMajorVersion} Desktop Runtime upgrade completed successfully.');
      3010: begin
        Log('.NET {#DotNetMajorVersion} Desktop Runtime upgraded (reboot required).');
        _dotnetRebootRequired := true;
      end;
      else Log('winget upgrade exited with code: ' + IntToStr(ResultCode) + ' (may indicate no updates available).');
    end;
  end
  else
  begin
    Log('Failed to execute winget upgrade check.');
  end;
end;

function NeedRestart(): Boolean;
begin
  Result := _dotnetRebootRequired;
end;