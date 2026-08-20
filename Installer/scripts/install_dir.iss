// Issue #2353: robustly reuse the prior install location.
// UsePreviousAppDir (keyed on AppId) is the primary mechanism; this scripted
// constant is a belt-and-suspenders safety net so the installer still lands in
// the real install directory when launched without a /DIR (e.g. a transitional
// update run by an older updater).
// We enumerate uninstall entries, skipping our own stable key(s) (UsePreviousAppDir
// already reuses them) and any entry lacking SoundSwitch.exe, then prefer the
// machine-wide (HKLM) install for determinism. Falls back to Default.

[Code]

function GetInstallDir(Default: string): string;
var
  Roots: array of Integer;
  Root, I, Count: Integer;
  RootsCount: Integer;
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
      // Skip our own stable uninstall key(s): the bare AppId and the _is1-suffixed
      // registry key Inno creates from it. UsePreviousAppDir already reuses them,
      // and we must not let a legacy entry override the stable path.
      if (CompareText(KeyName, '{#MyAppSetupName}') = 0) or
         (CompareText(KeyName, '{#MyAppSetupName}_is1') = 0) then
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
            // Ownership check: only reuse directories that contain SoundSwitch.exe,
            // so unrelated entries like "SoundSwitch Helper" are ignored.
            ExePath := InstallLoc + '\SoundSwitch.exe';
            if FileExists(ExePath) then
            begin
              Log('GetInstallDir: reusing existing install location ' + InstallLoc);
              // Prefer the machine-wide (HKLM) install for determinism: return
              // immediately on HKLM, otherwise remember and keep scanning HKCU.
              if Root = 0 then
              begin
                Result := InstallLoc;
                Exit;
              end;
              Result := InstallLoc;
            end;
          end;
        end;
      end;
    end;
  end;
end;
