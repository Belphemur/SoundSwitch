// requires Windows 10, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2008 R2 SP1, Windows Server 2008 Service Pack 2, Windows Server 2012, Windows Server 2012 R2, Windows Vista Service Pack 2
// WARNING: express setup (downloads and installs the components depending on your OS) if you want to deploy it on cd or network download the full bootsrapper on website below
// https://www.microsoft.com/en-US/download/details.aspx?id=49982

[CustomMessages]
dotnetfx47_title=.NET Framework 4.7.2

dotnetfx47_size=1 MB - 65 MB

;http://www.microsoft.com/globaldev/reference/lcid-all.mspx
en.dotnetfx47_lcid=
de.dotnetfx47_lcid=/lcid 1031


[Code]
const
	dotnetfx47_url = 'https://download.microsoft.com/download/0/5/C/05C1EC0E-D5EE-463B-BFE3-9311376A6809/NDP472-KB4054531-Web.exe';

procedure dotnetfx47(minVersion: integer);
begin
	if (not netfxinstalled(NetFx47, '') or (netfxspversion(NetFx47, '') < minVersion)) then
		AddProduct('dotnetfx47_web.exe',
			CustomMessage('dotnetfx47_lcid') + ' /passive /norestart /showrmui',
			CustomMessage('dotnetfx47_title'),
			CustomMessage('dotnetfx47_size'),
			dotnetfx47_url,
			false, false);
end;
