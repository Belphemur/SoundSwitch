@echo off
setlocal
cd /d "%~dp0"

SET FILE_DIR=%~dp0
SET BIN_DIR=%FILE_DIR%bin

set finalDir=%FILE_DIR%Final
set x86Release=%finalDir%\x86
set x64Release=%finalDir%\x64

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

set msbuildexe="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

Echo Making SoundSwitch...
Echo.
Echo "Build x86"
%msbuildexe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x86" /v:q /t:rebuild
Echo "Build x64"
%msbuildexe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x64" /v:q /t:rebuild
if not "%ERRORLEVEL%"=="0" (set builderror=1)
Echo.

if "%builderror%"=="1" echo error: build failed & goto Quit

echo "Copy Changelog"
xcopy /y CHANGELOG.md %finalDir% 1>nul 2>nul

Echo "Copy x64"
xcopy /y %BIN_DIR%\x64\Release\*.dll %x64Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\SoundSwitch.exe %x64Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\SoundSwitch.exe.config %x64Release% 1>nul 2>nul

Echo "Copy x86"
xcopy /y %BIN_DIR%\x86\Release\*.dll %x86Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x86\Release\SoundSwitch.* %x86Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x86\Release\SoundSwitch.exe.config %x86Release% 1>nul 2>nul

IF EXIST "%FILE_DIR%..\signinfo.txt" (
    echo Signing release...
    Echo.
    call Sign.bat %x86Release%\SoundSwitch.exe
    call Sign.bat %x64Release%\SoundSwitch.exe
    call Sign.bat %x86Release%\AudioEndPointLibrary.dll
    call Sign.bat %x64Release%\AudioEndPointLibrary.dll
    call Sign.bat %x86Release%\Audio.EndPoint.Controller.Wrapper.dll
    call Sign.bat %x64Release%\Audio.EndPoint.Controller.Wrapper.dll
)


call ./Installer/Make-Installer.bat
if not "%ERRORLEVEL%"=="0" echo error: make installer failed & goto Quit

echo.
Echo All operations completed successfully.

:Quit
Echo.
Pause