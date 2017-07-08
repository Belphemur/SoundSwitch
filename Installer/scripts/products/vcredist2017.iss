// requires Windows 10, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2003 Service Pack 2, Windows Server 2008 R2 SP1, Windows Server 2008 Service Pack 2, Windows Server 2012, Windows Vista Service Pack 2, Windows XP Service Pack 3
// http://www.microsoft.com/en-US/download/details.aspx?id=48145

[CustomMessages]
vcredist2017_title=Visual C++ 2017 Redistributable
vcredist2017_title_x64=Visual C++ 2017 64-Bit Redistributable

en.vcredist2017_size=13.55 MB
de.vcredist2017_size=13,55 MB
fr.vcredist2017_size=13,55 MB

en.vcredist2017_size_x64=14.55 MB
de.vcredist2017_size_x64=14,55 MB
fr.vcredist2017_size_x64=14,55 MB

[Code]
const
	vcredist2017_url = 'https://download.microsoft.com/download/1/f/e/1febbdb2-aded-4e14-9063-39fb17e88444/vc_redist.x86.exe';
	vcredist2017_url_x64 = 'https://download.microsoft.com/download/3/b/f/3bf6e759-c555-4595-8973-86b7b4312927/vc_redist.x64.exe';
  vc_redist_version = '14.0';
  vc_redist_build = 25017;

procedure vcredist2017();
begin
	if (not IsIA64()) then begin
		if (not IsVcRedistInstalled(vc_redist_version, vc_redist_build)) then
			AddProduct('vcredist2017' + GetArchitectureString() + '.exe',
				'/passive /norestart /showrmui',
				CustomMessage('vcredist2017_title' + GetArchitectureString()),
				CustomMessage('vcredist2017_size' + GetArchitectureString()),
				GetString(vcredist2017_url, vcredist2017_url_x64, ''),
				false, false);
	end;
end;
