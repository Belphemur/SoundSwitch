// SoundSwitch command line parameter utility functions
// Copyright © 2010-2025 SoundSwitch

#ifndef commandLineUtilsIss
#define commandLineUtilsIss

[Code]
// Check if a command line parameter exists
function CmdLineParamExists(const Value: string): Boolean;
var
  I: Integer;  
begin
  Result := False;
  for I := 1 to ParamCount do
    if CompareText(ParamStr(I), Value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

// Check if the /VERYSILENT parameter exists
function IsVerySilent: Boolean;
begin
  Result := CmdLineParamExists('/VERYSILENT');
end;

// Check if the donation page should be shown (not if /NODONATE is specified)
function ShowDonate: Boolean;
begin
  Result := not CmdLineParamExists('/NODONATE');
end;
#endif