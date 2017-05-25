@echo off
setlocal
cd /d "%~dp0"

SET FILE_DIR=%~dp0
SET BIN_DIR=%FILE_DIR%bin
SET LANGS=(fr de)

set finalDir=%FILE_DIR%Final
set x86Release=%finalDir%\x86
set x64Release=%finalDir%\x64

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
IF "%~1" neq "" (
    set buildPlatform=%~1
)


set VS2017Ent=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe
set VS2017Pro=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe
set VS2017Community=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe
set VS2015=%ProgramFiles(x86)%\MSBuild\14.0\bin\msbuild.exe

if exist "%VS2017Ent%" (
  echo Using VS2017Ent
  set msbuildexe="%VS2017Ent%"
) else (
    if exist "%VS2017Community%" (
      echo Using VS2017Community
      set msbuildexe="%VS2017Community%"
    ) else (
        if exist "%VS2017Pro%" (
          echo Using VS2017Pro
          set msbuildexe="%VS2017Pro%"
        ) else (
            echo Fallback to VS2015
            set msbuildexe="%VS2015%"
        )
    )
)

Echo Making SoundSwitch %buildPlatform%
Echo.
Echo "Build x86"
%msbuildexe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x86" /v:q /t:rebuild
Echo "Build x64"
%msbuildexe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x64" /v:q /t:rebuild
if not "%ERRORLEVEL%"=="0" (set builderror=1)
Echo.

if "%builderror%"=="1" echo error: build failed & goto Quit

echo "Generate Changelog"
cmd.exe /c markdown-html CHANGELOG.md -o %finalDir%\Changelog.html > NUL

echo "Generate README"
cmd.exe /c markdown-html README.md -o %finalDir%\Readme.html > NUL

echo "Copy img"
xcopy /y img\soundSwitched.png %finalDir% 1>nul 2>nul

echo "Copy LICENSE"
xcopy /y LICENSE.txt %finalDir% 1>nul 2>nul

Echo "Copy x64"
xcopy /y %BIN_DIR%\x64\Release\*.pdb %x64Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\*.dll %x64Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\SoundSwitch.exe %x64Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\SoundSwitch.exe.config %x64Release% 1>nul 2>nul
for %%l in %LANGS% DO (
    mkdir %x64Release%\%%l\ 
    xcopy /y %BIN_DIR%\x64\Release\%%l\SoundSwitch.resources.dll %x64Release%\%%l\ 1>nul 2>nul
)

Echo "Copy x86"
xcopy /y %BIN_DIR%\x86\Release\*.pdb %x86Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x86\Release\*.dll %x86Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x86\Release\SoundSwitch.* %x86Release% 1>nul 2>nul
xcopy /y %BIN_DIR%\x86\Release\SoundSwitch.exe.config %x86Release% 1>nul 2>nul
for %%l in %LANGS% DO (
    mkdir %x86Release%\%%l\
    xcopy /y %BIN_DIR%\x86\Release\%%l\SoundSwitch.resources.dll %x86Release%\%%l\ 1>nul 2>nul
)

rem IF EXIST "%FILE_DIR%..\signinfo.txt" (
rem     echo Signing release...
rem     Echo.
rem     call Sign.bat %x86Release%\SoundSwitch.exe
rem     call Sign.bat %x64Release%\SoundSwitch.exe
rem      call Sign.bat %x86Release%\AudioEndPointLibrary.dll
rem      call Sign.bat %x64Release%\AudioEndPointLibrary.dll
rem      call Sign.bat %x86Release%\Audio.EndPoint.Controller.Wrapper.dll
rem      call Sign.bat %x64Release%\Audio.EndPoint.Controller.Wrapper.dll
rem      for %%l in %LANGS% DO (
rem          call Sign.bat %x86Release%\%%l\SoundSwitch.resources.dll
rem          call Sign.bat %x64Release%\%%l\SoundSwitch.resources.dll
rem      )
rem )


call ./Installer/Make-Installer.bat %buildPlatform%
if not "%ERRORLEVEL%"=="0" echo error: make installer failed & goto Quit

echo.
Echo All operations completed successfully.

:Quit
Echo.
Pause