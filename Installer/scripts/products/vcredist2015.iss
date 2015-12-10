// requires Windows 10, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2003 Service Pack 2, Windows Server 2008 R2 SP1, Windows Server 2008 Service Pack 2, Windows Server 2012, Windows Vista Service Pack 2, Windows XP Service Pack 3
// http://www.microsoft.com/en-US/download/details.aspx?id=48145

[CustomMessages]
vcredist2015_title=Visual C++ 2015 Redistributable
vcredist2015_title_x64=Visual C++ 2015 64-Bit Redistributable

en.vcredist2015_size=12.8 MB
de.vcredist2015_size=12,8 MB

en.vcredist2015_size_x64=13.9 MB
de.vcredist2015_size_x64=13,9 MB


[Code]
const
	vcredist2015_url = 'http://download.microsoft.com/download/C/E/5/CE514EAE-78A8-4381-86E8-29108D78DBD4/VC_redist.x86.exe';
	vcredist2015_url_x64 = 'http://download.microsoft.com/download/C/E/5/CE514EAE-78A8-4381-86E8-29108D78DBD4/VC_redist.x64.exe';

	vcredist2015_productcode = '{74D0E5DB-B326-4DAE-A6B2-445B9DE1836E}';
	vcredist2015_productcode_x64 = '{0D3E9E15-DE7A-300B-96F1-B4AF12B96488}';

  vcredist2015_productcode2 = '{65AD78AD-D23D-3A1E-9305-3AE65CD522C2}';
	vcredist2015_productcode2_x64 = '{A1C31BA5-5438-3A07-9EEE-A5FB2D0FDE36}';

procedure vcredist2015();
begin
	if (not IsIA64()) then begin
		if (not msiproduct(GetString(vcredist2015_productcode, vcredist2015_productcode_x64, '')) and 
        not msiproduct(GetString(vcredist2015_productcode2, vcredist2015_productcode2_x64, ''))) then
			AddProduct('vcredist2015' + GetArchitectureString() + '.exe',
				'/passive /norestart',
				CustomMessage('vcredist2015_title' + GetArchitectureString()),
				CustomMessage('vcredist2015_size' + GetArchitectureString()),
				GetString(vcredist2015_url, vcredist2015_url_x64, ''),
				false, false);
	end;
end;
