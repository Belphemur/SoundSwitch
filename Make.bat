@echo off
setlocal
cd /d "%~dp0"

set x86Release="Final\x86"
set x64Release="Final\x64"

net session 1>nul 2>nul
if not "%ERRORLEVEL%"=="0" (
    echo Error: Restricted access. Please run Make.bat as administrator.
    goto Quit
)

git describe --abbrev=0 --tags > latestTag.txt
for /f "delims=" %%i in ('git rev-list HEAD --count') do set commitCount=%%i
set /p latestTag=<latestTag.txt
del latestTag.txt
set releaseVersion=%latestTag%.%commitCount%

rmdir /q /s bin 1>nul 2>nul
rmdir /q /s obj 1>nul 2>nul
rmdir /q /s Release 1>nul 2>nul

rmdir /q /s %x86Release% 1>nul 2>nul
rmdir /q /s %x64Release% 1>nul 2>nul
mkdir %x86Release% 1>nul 2>nul
mkdir %x64Release% 1>nul 2>nul

set buildPlatform=Release

set zipper="%ProgramFiles%\7-zip\7z.exe"
if not exist %zipper% (
  echo Error: 7-zip (native version^) is not installed
  goto Quit
)

set msbuildexe="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

Echo Making SoundSwitch...
Echo.
%msbuildexe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="Win32" /v:q /t:rebuild
%msbuildexe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x64" /v:q /t:rebuild
if not "%ERRORLEVEL%"=="0" (set builderror=1)
Echo.

if "%builderror%"=="1" echo error: build failed & goto Quit

Echo "Copy x64"
xcopy /y x64\Release\*.dll %x64Release% 1>nul 2>nul
xcopy /y x64\Release\SoundSwitch.exe %x64Release% 1>nul 2>nul
xcopy /y x64\Release\SoundSwitch.exe.config %x64Release% 1>nul 2>nul

Echo "Copy x86"
xcopy /y Release\*.dll %x86Release% 1>nul 2>nul
xcopy /y Release\SoundSwitch.* %x86Release% 1>nul 2>nul
xcopy /y Release\SoundSwitch.exe.config %x64Release% 1>nul 2>nul

if exist Sign.bat (
    echo Signing release...
    Echo.
    call Sign.bat %x86Release%\SoundSwitch.exe
    call Sign.bat %x64Release%\SoundSwitch.exe
)


call ./Installer/Make-Installer.bat
if not "%ERRORLEVEL%"=="0" echo error: make installer failed & goto Quit

echo.
Echo All operations completed successfully.

:Quit
Echo.
Pause