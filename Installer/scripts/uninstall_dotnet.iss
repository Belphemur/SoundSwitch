#ifndef uninstallDotnetIss
#define uninstallDotnetIss

[Code]
procedure UninstallOlderDotNetRuntimes(Major, Minor, TargetRevision: Word);
var
  UninstallKeys: TArrayOfString;
  RootKeys: array of Integer;
  RootIndex, KeyIndex: Integer;
  RootKey: Integer;
  SubKeyName, DisplayName, DisplayVersion, QuietUninstallString: String;
  PackedVersion: Int64;
  V_Major, V_Minor, V_Revision, V_Build: Word;
  ResultCode: Integer;
  TargetNamePrefix: String;
begin
  SetArrayLength(RootKeys, 2);
  RootKeys[0] := HKLM;
  RootKeys[1] := HKLM32; // InnoSetup maps this to WOW6432Node

  TargetNamePrefix := 'Microsoft Windows Desktop Runtime - ' + IntToStr(Major) + '.' + IntToStr(Minor) + '.';

  for RootIndex := 0 to 1 do
  begin
    RootKey := RootKeys[RootIndex];
    if RegGetSubkeyNames(RootKey, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall', UninstallKeys) then
    begin
      for KeyIndex := 0 to GetArrayLength(UninstallKeys) - 1 do
      begin
        SubKeyName := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + UninstallKeys[KeyIndex];
        
        if RegQueryStringValue(RootKey, SubKeyName, 'DisplayName', DisplayName) then
        begin
          if Pos(TargetNamePrefix, DisplayName) = 1 then
          begin
            if RegQueryStringValue(RootKey, SubKeyName, 'DisplayVersion', DisplayVersion) then
            begin
              if StrToVersion(DisplayVersion, PackedVersion) then
              begin
                UnpackVersionComponents(PackedVersion, V_Major, V_Minor, V_Revision, V_Build);
                
                if (V_Major = Major) and (V_Minor = Minor) and (V_Revision < TargetRevision) then
                begin
                  if RegQueryStringValue(RootKey, SubKeyName, 'QuietUninstallString', QuietUninstallString) then
                  begin
                    Log('Uninstalling older .NET Desktop Runtime: ' + DisplayName);
                    Exec(ExpandConstant('{cmd}'), '/c ' + QuietUninstallString, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
                  end;
                end;
              end;
            end;
          end;
        end;
      end;
    end;
  end;
end;

#endif
