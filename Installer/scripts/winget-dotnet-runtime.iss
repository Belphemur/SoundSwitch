[Code]
// Checks if Microsoft.WindowsDesktop.App 10.x is already installed
function IsDotNetDesktopRuntime10Installed(): Boolean;
var
  FindRec: TFindRec;
  SharedDir: String;
begin
  Result := false;
  
  // Check machine-wide install: look for any 10.0.* directory
  SharedDir := ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App\');
  if FindFirst(SharedDir + '*', FindRec) then
  begin
    repeat
      if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0) and
         (Pos('10.0', FindRec.Name) = 1) then
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
         (Pos('10.0', FindRec.Name) = 1) then
      begin
        Result := true;
        FindClose(FindRec);
        exit;
      end;
    until not FindNext(FindRec);
    FindClose(FindRec);
  end;
end;

// Installs .NET 10 Desktop Runtime via winget
// Returns True on success, False on failure (shows error message)
function InstallDotNetDesktopRuntime10(): Boolean;
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
  
  // Find winget: try PATH first, then fallback to known location
  WingetPath := 'winget.exe';
  if not FileExists(WingetPath) then
    WingetPath := ExpandConstant('{localappdata}\Microsoft\WindowsApps\winget.exe');
  
  if not FileExists(WingetPath) then
  begin
    MsgBox('Windows Package Manager (winget) is not available on this system.' + #13#10 +
           'Please install .NET 10 Desktop Runtime manually from:' + #13#10 +
           'https://dotnet.microsoft.com/en-us/download/dotnet', mbError, MB_OK);
    Result := false;
    exit;
  end;
  
  // Build winget arguments
  Args := 'install --exact --id Microsoft.DotNet.DesktopRuntime.10' +
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
        Log('.NET 10 Desktop Runtime installed successfully.');
        Result := true;
      end;
      3010: begin
        Log('.NET 10 Desktop Runtime installed (reboot required).');
        Result := true; // Success, but needs reboot later
      end;
      else begin
        Log('winget failed with exit code: ' + IntToStr(ResultCode));
        MsgBox('Failed to install .NET 10 Desktop Runtime (exit code: ' + IntToStr(ResultCode) + ').' + #13#10 +
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
           'Please install .NET 10 Desktop Runtime manually from:' + #13#10 +
           'https://dotnet.microsoft.com/en-us/download/dotnet', mbError, MB_OK);
    Result := false;
  end;
end;