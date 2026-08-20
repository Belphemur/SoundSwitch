// Issue #2353: robustly reuse the prior install location.
// UsePreviousAppDir (keyed on AppId) is the primary mechanism; this scripted
// constant is a belt-and-suspenders safety net so the installer still lands in
// the real install directory when launched without a /DIR (e.g. a transitional
// update run by an older updater).
// We skip our own stable key (UsePreviousAppDir handles it), then match any
// other uninstall entry whose DisplayName is exactly {#MyAppSetupName} or begins
// with "{#MyAppSetupName} " (name + space + version), and whose InstallLocation
// actually contains SoundSwitch.exe. The ownership check avoids hijacking
// unrelated entries such as "SoundSwitch Helper". Falls back to Default.

[Code]

function GetInstallDir(Default: string): string;
var
  Roots: array of Integer;
  Root, I, Count: Integer;
  BaseKey, FullKey, KeyName, DisplayName, InstallLoc, ExePath: string;
  SubKeys: TArrayOfString;
begin
  // Default is the value Inno would have used; fall back to it (or the
  // conventional Program Files path) if no existing install is found.
  Result := Default;
  if Result = '' then
    Result := ExpandConstant('{autopf}\{#MyAppSetupName}');

  SetArrayLength(Roots, 2);
  Roots[0] := HKLM;
  Roots[1] := HKCU;

  for Root := 0 to 1 do
  begin
    BaseKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall';
    if not RegGetSubkeyNames(Roots[Root], BaseKey, SubKeys) then
      Continue;

    Count := GetArrayLength(SubKeys);
    for I := 0 to Count - 1 do
    begin
      KeyName := SubKeys[I];
      // Skip our own stable key; UsePreviousAppDir already reuses it.
      if CompareText(KeyName, '{#MyAppSetupName}') = 0 then
        Continue;

      FullKey := BaseKey + '\' + KeyName;
      if not RegQueryStringValue(Roots[Root], FullKey, 'DisplayName', DisplayName) then
        Continue;

      if (CompareText(DisplayName, '{#MyAppSetupName}') = 0) or
         ((Pos('{#MyAppSetupName}', DisplayName) = 1) and
          (Copy(DisplayName, Length('{#MyAppSetupName}') + 1, 1) = ' ')) then
      begin
        if RegQueryStringValue(Roots[Root], FullKey, 'InstallLocation', InstallLoc) then
        begin
          if (InstallLoc <> '') and DirExists(InstallLoc) then
          begin
            // Ownership check: only reuse directories that contain SoundSwitch.exe.
            ExePath := InstallLoc + '\SoundSwitch.exe';
            if FileExists(ExePath) then
            begin
              Log('GetInstallDir: reusing existing install location ' + InstallLoc);
              Result := InstallLoc;
              Exit;
            end;
          end;
        end;
      end;
    end;
  end;
end;
