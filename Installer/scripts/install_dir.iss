// Issue #2353: robustly reuse the prior install location.
// UsePreviousAppDir (keyed on AppId) is the primary mechanism. This scripted
// constant is a belt-and-suspenders safety net so the installer still lands in
// the real install directory when launched without a /DIR (e.g. a transitional
// update run by an older updater).
// We resolve the directory by known SoundSwitch uninstall key identities, under
// HKLM then HKCU, in priority order:
//   1. the new AppId identity (me.aaflalo.soundswitch -> "me.aaflalo.soundswitch_is1")
//   2. the legacy AppId identity (SoundSwitch -> "SoundSwitch_is1", plus bare "SoundSwitch"
//      for very old installs) so existing/non-default installs keep upgrading in place.
// Matching the key name (not a DisplayName prefix) keeps selection deterministic and
// unaffected by unrelated entries such as "SoundSwitch Helper". The SoundSwitch.exe
// ownership check is defense-in-depth. Falls back to Default when no identity is found.

[Code]

function TryGetInstallDirFromKey(Root: Integer; const KeyName: string): string;
var
  BaseKey, FullKey, InstallLoc, ExePath: string;
begin
  Result := '';
  BaseKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall';
  FullKey := BaseKey + '\' + KeyName;
  if not RegQueryStringValue(Root, FullKey, 'InstallLocation', InstallLoc) then
    Exit;
  if (InstallLoc = '') or not DirExists(InstallLoc) then
    Exit;
  // Ownership check: only accept directories that contain SoundSwitch.exe.
  ExePath := InstallLoc + '\SoundSwitch.exe';
  if FileExists(ExePath) then
    Result := InstallLoc;
end;

function GetInstallDir(Default: string): string;
var
  Dir: string;
  I, J: Integer;
  Roots: array of Integer;
  KeyNames: array of string;
begin
  Result := Default;
  if Result = '' then
    Result := ExpandConstant('{autopf}\{#MyAppSetupName}');

  // Known uninstall key identities for SoundSwitch, in priority order.
  // New identity first, then legacy identities from before the AppId change.
  SetArrayLength(KeyNames, 3);
  KeyNames[0] := 'me.aaflalo.soundswitch_is1'; // current AppId + _is1 suffix
  KeyNames[1] := '{#MyAppSetupName}_is1';      // legacy AppId "SoundSwitch" + _is1
  KeyNames[2] := '{#MyAppSetupName}';          // bare key (very old installs)

  SetArrayLength(Roots, 2);
  Roots[0] := HKLM;
  Roots[1] := HKCU;

  // Deterministic: machine-wide (HKLM) first, then per-user (HKCU),
  // each trying the known key identities in priority order.
  for I := 0 to 1 do
  begin
    for J := 0 to 2 do
    begin
      Dir := TryGetInstallDirFromKey(Roots[I], KeyNames[J]);
      if Dir <> '' then
      begin
        Log('GetInstallDir: reusing existing install location ' + Dir);
        Result := Dir;
        Exit;
      end;
    end;
  end;
end;
