[Code]
type
	NetFXType = (NetFx10, NetFx11, NetFx20, NetFx30, NetFx35, NetFx40Client, NetFx40Full, NetFx45, NetFx46, NetFx47);

const
	netfx11plus_reg = 'Software\Microsoft\NET Framework Setup\NDP\';

function booltostr(B: boolean): string;
begin
  Result:= 'False';
  if B then
    Result:= 'True';    
end;

function hasDotNetCore(version: string) : boolean;
var
	architecture: string;
	runtimes: TArrayOfString;
	I: Integer;
	versionCompare: Integer;
begin
	architecture := 'x64';
	if(not Is64BitInstallMode) then
	   architecture := 'x86';
	   
	Log('[.NET] Look for version ' + version);
	   
	if not RegGetValueNames(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\'+ architecture +'\sharedfx\Microsoft.NETCore.App', runtimes) then
	begin
	  Log('[.NET] Issue getting runtimes from registry');
	  Result := False;
	  Exit;
	end;
	
    for I := 0 to GetArrayLength(runtimes)-1 do
	begin
	  versionCompare := CompareVersion(runtimes[I], version);
	  Log(Format('[.NET] Compare: %s/%s = %d', [runtimes[I], version, versionCompare]));
	  if(not versionCompare = -1) then
	  begin
	    Result := True;
	  	Exit;
	  end;
    end;
	   
	Result := False;
end;

function netfxinstalled(version: NetFXType; lcid: string): boolean;
var
	regVersion: cardinal;
	regVersionString: string;
begin
	if (lcid <> '') then
		lcid := '\' + lcid;

	if (version = NetFx10) then begin
		RegQueryStringValue(HKLM, 'Software\Microsoft\.NETFramework\Policy\v1.0\3705', 'Install', regVersionString);
		Result := regVersionString <> '';
	end else begin
		case version of
			NetFx11:
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v1.1.4322' + lcid, 'Install', regVersion);
			NetFx20:
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v2.0.50727' + lcid, 'Install', regVersion);
			NetFx30:
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.0\Setup' + lcid, 'InstallSuccess', regVersion);
			NetFx35:
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.5' + lcid, 'Install', regVersion);
			NetFx40Client:
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Client' + lcid, 'Install', regVersion);
			NetFx40Full:
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Install', regVersion);
			NetFx45:
			begin
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion);
				// >= 4.5.0        
				Result := (regVersion >= 378389);
				Exit;
      end;
      NetFx46:
			begin
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion);
				// >= 4.6.0        
				Result := (regVersion >= 393295);
				Exit;
      end;
      NetFx47:
			begin
				RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion);
				// >= 4.7.0        
				Result := (regVersion >= 460798);
				Exit;
      end;
		end;
		Result := (regVersion <> 0);
	end;
end;

function netfxspversion(version: NetFXType; lcid: string): integer;
var
	regVersion: cardinal;
begin
	if (lcid <> '') then
		lcid := '\' + lcid;

	case version of
		NetFx10:
			//not supported
			regVersion := -1;
		NetFx11:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v1.1.4322' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx20:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v2.0.50727' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx30:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.0' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx35:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.5' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx40Client:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Client' + lcid, 'Servicing', regVersion)) then
				regVersion := -1;
		NetFx40Full:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Servicing', regVersion)) then
				regVersion := -1;
		NetFx45:
			if (RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion)) then begin
        if (regVersion >= 378389) then
					regVersion := 2 // 4.6.0
				else if (regVersion = 379893) then
					regVersion := 2 // 4.5.2
				else if (regVersion = 378675) or (regVersion = 378758) then
					regVersion := 1 // 4.5.1
				else if (regVersion = 378389) then
					regVersion := 0 // 4.5.0
				else
					regVersion := -1;
			end;
    NetFx46:
			if (RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion)) then begin
				if (regVersion = 393295) or (regVersion = 393297) then
					regVersion := 0 // 4.6.0
				else if (regVersion = 394254) or (regVersion = 394271) then
					regVersion := 1 // 4.6.1
				else if (regVersion >= 394802) then
					regVersion := 2 //4.6.2
				else
					regVersion := -1;
			end;
    NetFx47:
			if (RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion)) then begin
				if (regVersion = 460798) then
					regVersion := 0 // 4.7.0
				else if (regVersion = 461308) then
					regVersion := 1 // 4.7.1
				else if (regVersion >= 461808) then
					regVersion := 2 //4.7.2
				else
					regVersion := -1;
			end;
	end;
	Result := regVersion;
end;
