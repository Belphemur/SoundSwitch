// requires Windows 10, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2008 R2 SP1, Windows Server 2008 Service Pack 2, Windows Server 2012, Windows Server 2012 R2, Windows Vista Service Pack 2
// WARNING: express setup (downloads and installs the components depending on your OS) if you want to deploy it on cd or network download the full bootsrapper on website below
// https://www.microsoft.com/en-US/download/details.aspx?id=49982

[CustomMessages]
dotnetfx46_title=.NET Framework 4.6.2

dotnetfx46_size=1 MB - 65 MB

;http://www.microsoft.com/globaldev/reference/lcid-all.mspx
en.dotnetfx46_lcid=
de.dotnetfx46_lcid=/lcid 1031


[Code]
const
	dotnetfx462_url = 'https://download.microsoft.com/download/F/9/4/F942F07D-F26F-4F30-B4E3-EBD54FABA377/NDP462-KB3151800-x86-x64-AllOS-ENU.exe';

procedure dotnetfx46(minVersion: integer);
begin
	if (not netfxinstalled(NetFx46, '') or (netfxspversion(NetFx46, '') < minVersion)) then
		AddProduct('dotnetfx46.exe',
			CustomMessage('dotnetfx46_lcid') + ' /passive /norestart',
			CustomMessage('dotnetfx46_title'),
			CustomMessage('dotnetfx46_size'),
			dotnetfx462_url,
			false, false);
end;