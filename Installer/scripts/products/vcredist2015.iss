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
	vcredist2015_url = 'https://download.microsoft.com/download/6/D/F/6DF3FF94-F7F9-4F0B-838C-A328D1A7D0EE/vc_redist.x86.exe';
	vcredist2015_url_x64 = 'https://download.microsoft.com/download/6/D/F/6DF3FF94-F7F9-4F0B-838C-A328D1A7D0EE/vc_redist.x64.exe';
  vc_redist_version = '14.0';
  vc_redist_build = 24212;

procedure vcredist2015();
begin
	if (not IsIA64()) then begin
		if (not IsVcRedistInstalled(vc_redist_version, vc_redist_build)) then
			AddProduct('vcredist2015' + GetArchitectureString() + '.exe',
				'/passive /norestart /showrmui',
				CustomMessage('vcredist2015_title' + GetArchitectureString()),
				CustomMessage('vcredist2015_size' + GetArchitectureString()),
				GetString(vcredist2015_url, vcredist2015_url_x64, ''),
				false, false);
	end;
end;
