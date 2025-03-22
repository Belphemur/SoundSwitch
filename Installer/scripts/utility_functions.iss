// SoundSwitch installer utility functions
// Copyright © 2010-2025 SoundSwitch

#ifndef utilityFunctionsIss
#define utilityFunctionsIss

[Code]
// Constants
const
  WM_SETTINGCHANGE = 26;
  SMTO_ABORTIFHUNG = 2;

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

function ShowDonate: Boolean;
begin
  Result := not CmdLineParamExists('/NODONATE');
end;

function IsVerySilent: Boolean;
begin
  Result := CmdLineParamExists('/VERYSILENT');
end;

function IsX86: boolean;
begin
	Result := (ProcessorArchitecture = paX86) or (ProcessorArchitecture = paUnknown);
end;

function IsX64: boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paX64);
end;

function GetString(x86, x64, ia64: String): String;
begin
	if IsX64() and (x64 <> '') then begin
		Result := x64;
	end else begin
		Result := x86;
	end;
end;

function GetArchitectureString(): String;
begin
	if IsX64() then begin
		Result := '_x64';
	end else begin
		Result := '';
	end;
end;

#endif