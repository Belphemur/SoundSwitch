[Code]
procedure Explode(var Dest: TArrayOfString; Text: String; Separator: String);
var
  i, p: Integer;
begin
  i := 0;
  repeat
    SetArrayLength(Dest, i+1);
    p := Pos(Separator,Text);
    if p > 0 then begin
      Dest[i] := Copy(Text, 1, p-1);
      Text := Copy(Text, p + Length(Separator), Length(Text));
      i := i + 1;
    end else begin
      Dest[i] := Text;
      Text := '';
    end;
  until Length(Text)=0;
end;

function IsVcRedistInstalled(version: String; build: Integer): boolean;
var
  regBuildNumber: cardinal;
  regKey: String;
  regInstalled: cardinal;
  regStringBuildNumber: String;
  strArrayBuildNumber: TArrayOfString;
begin
  if(IsX64()) then begin
    regKey := 'SOFTWARE\WOW6432Node\Microsoft\VisualStudio\'+ version +'\VC\Runtimes\x64';
  end else begin
    regKey := 'SOFTWARE\Microsoft\VisualStudio\'+ version +'\VC\VCRedist\x86';
  end;
  RegQueryDWordValue(HKLM, regKey, 'Installed', regInstalled);
  if(regInstalled <> 1) then begin
    if(IsX64()) then begin
      regKey := 'SOFTWARE\Wow6432Node\Microsoft\DevDiv\vc\Servicing\'+ version +'\RuntimeAdditional';
    end else begin
      regKey := 'SOFTWARE\Microsoft\DevDiv\vc\Servicing\'+ version +'\VC\VCRedist\x86';
    end;
    RegQueryDWordValue(HKLM, regKey, 'Install', regInstalled);
    if(regInstalled <> 1) then begin
      Result := false;
      Exit;
    end;
    RegQueryStringValue(HKLM, regKey, 'Version', regStringBuildNumber);
    Explode(strArrayBuildNumber, regStringBuildNumber, '.');
    regBuildNumber := StrToInt(strArrayBuildNumber[2]);
    Result:= (regBuildNumber >= build);

  end else begin
    RegQueryDWordValue(HKLM, regKey, 'Bld', regBuildNumber);
    Result := (regBuildNumber >= build);
    Exit;
  end;
  
end;
