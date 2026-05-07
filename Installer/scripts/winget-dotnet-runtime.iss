[Code]
var
  _dotnetRebootRequired: Boolean;

// Checks if Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.x is already installed
// Uses dotnet --list-runtimes for reliable detection across all install locations
function IsDotNetDesktopRuntimeInstalled(): Boolean;
var
  TempFile: String;
  Output: AnsiString;
  ResultCode: Integer;
  DotNetExe: String;
  CmdLine: String;
begin
  Result := false;

  // Find dotnet.exe on system PATH
  DotNetExe := FileSearch('dotnet.exe', GetEnv('PATH'));
  if DotNetExe = '' then
  begin
    Log('dotnet CLI not found on PATH. Cannot check for runtime.');
    exit;
  end;

  TempFile := ExpandConstant('{tmp}\dotnet_runtimes.txt');

  // Run dotnet --list-runtimes, redirect output to temp file
  CmdLine := '/C "' + DotNetExe + '" --list-runtimes > "' + TempFile + '"';
  if not Exec(ExpandConstant('{cmd}'), CmdLine, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Failed to execute dotnet --list-runtimes.');
    exit;
  end;

  if ResultCode <> 0 then
  begin
    Log('dotnet --list-runtimes returned non-zero exit code: ' + IntToStr(ResultCode));
    exit;
  end;

  // Read captured output
  if not LoadStringFromFile(TempFile, Output) then
  begin
    Log('Failed to read dotnet --list-runtimes output from temp file.');
    DeleteFile(TempFile);
    exit;
  end;

  DeleteFile(TempFile);

  // Check if Microsoft.WindowsDesktop.App {#DotNetMajorVersion}. is in the output
  Result := Pos('Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.', Output) > 0;
  if Result then
    Log('Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.x found via dotnet --list-runtimes.')
  else
    Log('Microsoft.WindowsDesktop.App {#DotNetMajorVersion}.x not found in dotnet --list-runtimes output.');
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