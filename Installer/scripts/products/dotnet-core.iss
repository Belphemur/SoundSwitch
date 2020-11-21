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

dotnet_core_url_3_1_6=https://download.visualstudio.microsoft.com/download/pr/d5fc4fb5-7374-4886-8815-68b7bf788b5b/3aeb172d4a3c5e01078738440442f4c7/windowsdesktop-runtime-3.1.6-win-x86.exe
dotnet_core_url_x64_3_1_6=https://download.visualstudio.microsoft.com/download/pr/3eb7efa1-96c6-4e97-bb9f-563ecf595f8a/7efd9c1cdd74df8fb0a34c288138a84f/windowsdesktop-runtime-3.1.6-win-x64.exe

dotnet_core_url_3_1_10=https://download.visualstudio.microsoft.com/download/pr/c0a1f953-81d3-4a1a-a584-a627b518c434/16e1af0d3ebe6edacde1eab155dd4d90/aspnetcore-runtime-3.1.10-win-x86.exe
dotnet_core_url_x64_3_1_10=https://download.visualstudio.microsoft.com/download/pr/c1ea0601-abe4-4c6d-96ed-131764bf5129/a1823d8ff605c30af412776e2e617a36/aspnetcore-runtime-3.1.10-win-x64.exe

dotnet_core_url_5_0_0=https://download.visualstudio.microsoft.com/download/pr/b2780d75-e54a-448a-95fc-da9721b2b4c2/62310a9e9f0ba7b18741944cbae9f592/windowsdesktop-runtime-5.0.0-win-x86.exe
dotnet_core_url_x64_5_0_0=https://download.visualstudio.microsoft.com/download/pr/1b3a8899-127a-4465-a3c2-7ce5e4feb07b/1e153ad470768baa40ed3f57e6e7a9d8/windowsdesktop-runtime-5.0.0-win-x64.exe
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
