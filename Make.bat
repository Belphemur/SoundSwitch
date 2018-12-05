@echo off
rem SoundSwitch Make file
rem 
rem Compiles SoundSwitch binaries and installer.
rem 
rem Requires Visual Studio >=2017 or add switch
rem -legacy to force use of MSBuild tools from VS2015.
rem
rem Requires the npm package markdown-html:
rem https://github.com/Belphemur/markdown-html
rem
rem You may run this script without markdown-html,
rem but a dummy Changelog and README is created then.

setlocal
cd /d "%~dp0"

set FILE_DIR=%~dp0
set BIN_DIR=%FILE_DIR%bin
set LANGS=(fr de es nb pt-BR it-IT zh-CHS)

if ["%~1"]==["-legacy"] set USE_LEGACY_VS2015=1

set finalDir=%FILE_DIR%Final
set x86Release=%finalDir%\x86
set x64Release=%finalDir%\x64

rem Check if required commands exist
where markdown-html >nul 2>nul
set /a buildChangelogAndReadme=!%ERRORLEVEL%
if %buildChangelogAndReadme%==0 (
    echo markdown-html command not found, building with dummy Changelog and README!
)

echo Cleaning build directories
rmdir /q /s bin >nul 2>nul
rmdir /q /s obj >nul 2>nul
rmdir /q /s Release >nul 2>nul

rmdir /q /s %x86Release% >nul 2>nul
rmdir /q /s %x64Release% >nul 2>nul
mkdir %x86Release% >nul 2>nul
mkdir %x64Release% >nul 2>nul

set buildPlatform=Release
if "%~1" neq "" (
    set buildPlatform=%~1
)

echo.
echo Determine MSBuild.exe...
if defined USE_LEGACY_VS2015 (
    for /f "usebackq tokens=*" %%i in (`tools\vswhere -legacy -version [14.0^,15.0] -property installationPath`) do (
        set msBuildExe=%%i
    )
    set msBuildVersion=14.0
) else (
    for /f "usebackq tokens=*" %%i in (`tools\vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
        set msBuildExe=%%i
    )
    set msBuildVersion=15.0
)
set msBuildExe="%msBuildExe%\MSBuild\%msBuildVersion%\Bin\MSBuild.exe"

if not exist %msBuildExe% (set errorMessage=MSBuild.exe not found in %msBuildExe% & goto ERROR_QUIT)

echo %msBuildExe%
echo.
echo Building SoundSwitch %buildPlatform%
echo.
echo Build x86
%msBuildExe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x86" /v:q /t:rebuild || (set errorMessage=Build x86 failed & goto ERROR_QUIT)
echo Build x64
%msBuildExe% SoundSwitch.sln /m /p:Configuration=%buildPlatform% /p:Platform="x64" /v:q /t:rebuild || (set errorMessage=Build x64 failed & goto ERROR_QUIT)
echo.

if %buildChangelogAndReadme%==1 (
    echo Generate Changelog
    cmd.exe /c markdown-html CHANGELOG.md -o %finalDir%\Changelog.html > NUL

    echo Generate README
    cmd.exe /c markdown-html README.md -o %finalDir%\Readme.html > NUL
) else (
    echo Generate dummy Changelog
    echo ^<html^>^<body^>^<h1^>Dummy Changelog, markdown-html is required^</h1^>^</body^>^</html^> > %finalDir%\Changelog.html
    
    echo Generate dummy README
    echo ^<html^>^<body^>^<h1^>Dummy README, markdown-html is required^</h1^>^</body^>^</html^> > %finalDir%\Readme.html
)

echo Copy soundSwitched image
xcopy /y img\soundSwitched.png %finalDir% >nul 2>nul

echo Copy LICENSE
xcopy /y LICENSE.txt %finalDir% >nul 2>nul

echo Copy x64
xcopy /y %BIN_DIR%\x64\Release\*.pdb %x64Release% >nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\*.dll %x64Release% >nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\SoundSwitch.exe %x64Release% >nul 2>nul
xcopy /y %BIN_DIR%\x64\Release\SoundSwitch.exe.config %x64Release% >nul 2>nul
for %%l in %LANGS% DO (
    mkdir %x64Release%\%%l\ 
    xcopy /y %BIN_DIR%\x64\Release\%%l\SoundSwitch.resources.dll %x64Release%\%%l\ >nul 2>nul
)

echo Copy x86
xcopy /y %BIN_DIR%\Win32\Release\*.pdb %x86Release% >nul 2>nul
xcopy /y %BIN_DIR%\Win32\Release\*.dll %x86Release% >nul 2>nul
xcopy /y %BIN_DIR%\Win32\Release\SoundSwitch.* %x86Release% >nul 2>nul
xcopy /y %BIN_DIR%\Win32\Release\SoundSwitch.exe.config %x86Release% >nul 2>nul
for %%l in %LANGS% DO (
    mkdir %x86Release%\%%l\
    xcopy /y %BIN_DIR%\Win32\Release\%%l\SoundSwitch.resources.dll %x86Release%\%%l\ >nul 2>nul
)

echo START
rem Run installer compiler script
call ./Installer/Make-Installer.bat %buildPlatform%
if not %ERRORLEVEL%==0 (set errorMessage=Make-installer.bat failed or not found & goto ERROR_QUIT)

echo.
echo All operations completed successfully.
echo.
pause
exit /b 0

:ERROR_QUIT
echo.
if defined errorMessage (
    echo Error: %errorMessage%!
) else (
    echo An unknown error occurred.
)
echo.
pause
exit /b 1
