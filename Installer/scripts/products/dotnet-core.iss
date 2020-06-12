[CustomMessages]
dotnet_core_title=.NET Core

dotnet_core_size=53 MB

;http://www.microsoft.com/globaldev/reference/lcid-all.mspx
en.dotnet_core_lcid=
de.dotnet_core_lcid=/lcid 1031

dotnet_core_url_3_1_4=https://download.visualstudio.microsoft.com/download/pr/2d4b7600-5f32-4a1f-abd5-47cdb2d1362b/7b8b7635e3bb63f6b2cc9a1c624b5325/windowsdesktop-runtime-3.1.4-win-x86.exe
dotnet_core_url_x64_3_1_4=https://download.visualstudio.microsoft.com/download/pr/d8cf1fe3-21c2-4baf-988f-f0152996135e/0c00b94713ee93e7ad5b4f82e2b86607/windowsdesktop-runtime-3.1.4-win-x64.exe

dotnet_core_url_3_1_5=https://download.visualstudio.microsoft.com/download/pr/df7b90d9-b93e-4974-85ef-c1de418bc186/e380e58bbd8505ebaee6c3abb23baade/windowsdesktop-runtime-3.1.5-win-x86.exe
dotnet_core_url_x64_3_1_5=https://download.visualstudio.microsoft.com/download/pr/86835fe4-93b5-4f4e-a7ad-c0b0532e407b/f4f2b1239f1203a05b9952028d54fc13/windowsdesktop-runtime-3.1.5-win-x64.exe
[Code]
procedure dotnetCore(version:string);
var
	underscoreVersion: string;
begin	
    underscoreVersion := version;
	StringChangeEx(underscoreVersion, '.', '_', True);
	if (not hasDotNetCore(version)) then
		AddProduct('windowsdesktop-runtime-' + version + '.exe',
			CustomMessage('dotnet_core_lcid') + ' /passive /norestart /showrmui',
			CustomMessage('dotnet_core_title') + ' ' +  version,
			CustomMessage('dotnet_core_size'),
			GetString(CustomMessage('dotnet_core_url_' + underscoreVersion), CustomMessage('dotnet_core_url_x64_' + underscoreVersion), ''),
			false, false);
end;
