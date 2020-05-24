// requires Windows 10, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2008 R2 SP1, Windows Server 2008 Service Pack 2, Windows Server 2012, Windows Server 2012 R2, Windows Vista Service Pack 2
// WARNING: express setup (downloads and installs the components depending on your OS) if you want to deploy it on cd or network download the full bootsrapper on website below
// https://www.microsoft.com/en-US/download/details.aspx?id=49982

[CustomMessages]
dotnet_core_314_title=.NET Core 3.1.4

dotnet_core_314_size=53 MB

;http://www.microsoft.com/globaldev/reference/lcid-all.mspx
en.dotnet_core_314_lcid=
de.dotnet_core_314_lcid=/lcid 1031


[Code]
const
	dotnet_core_314_url = 'https://download.visualstudio.microsoft.com/download/pr/2d4b7600-5f32-4a1f-abd5-47cdb2d1362b/7b8b7635e3bb63f6b2cc9a1c624b5325/windowsdesktop-runtime-3.1.4-win-x86.exe';
	dotnet_core_314_url_x64 = 'https://download.visualstudio.microsoft.com/download/pr/d8cf1fe3-21c2-4baf-988f-f0152996135e/0c00b94713ee93e7ad5b4f82e2b86607/windowsdesktop-runtime-3.1.4-win-x64.exe';

procedure dotnet_core_314(minVersion: integer);
begin
	if (not hasDotNetCore("3.1.4")) then
		AddProduct('dotnet_core_314.exe',
			CustomMessage('dotnet_core_314_lcid') + ' /passive /norestart /showrmui',
			CustomMessage('dotnet_core_314_title'),
			CustomMessage('dotnet_core_314_size'),
			GetString(dotnet_core_314_url, dotnet_core_314_url_x64, ''),
			false, false);
end;
