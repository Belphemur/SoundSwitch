[Code]


function IsVcRedistInstalled(version: String; build: Integer): boolean;
var
  regBuildNumber: cardinal;
  regKey: String;
  regInstalled: cardinal;
begin
  if(IsX64()) then begin
    regKey := 'SOFTWARE\WOW6432Node\Microsoft\VisualStudio\'+ version +'\VC\Runtimes\x64';
  end else begin
    regKey := 'SOFTWARE\Microsoft\VisualStudio\'+ version +'\VC\VCRedist\x86';
  end;
  RegQueryDWordValue(HKLM, regKey, 'Installed', regInstalled);
  if(regInstalled <> 1) then begin
    Result := false
    Exit;
  end;
  RegQueryDWordValue(HKLM, regKey, 'Bld', regBuildNumber);
  Result := (regBuildNumber >= build);    
end;
