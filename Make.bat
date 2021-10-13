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

set buildPlatform=Release
if "%~1" neq "" (
    set buildPlatform=%~1
)


set FILE_DIR=%~dp0
set FRAMEWORK=net6.0-windows
set ARCH=win-x64
set BIN_DIR=%FILE_DIR%SoundSwitch\bin\%buildPlatform%\%FRAMEWORK%\%ARCH%\publish

set finalDir=%FILE_DIR%Final

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

rmdir /q /s %finalDir% >nul 2>nul
mkdir %finalDir% >nul 2>nul



echo.
echo Building SoundSwitch %buildPlatform%
echo.
echo Build AnyCPU
dotnet publish -c %buildPlatform% %FILE_DIR%SoundSwitch\SoundSwitch.csproj || (set errorMessage=Build %ARCH% failed & goto ERROR_QUIT)
echo.

if %buildChangelogAndReadme%==1 (
    echo Generate Changelog
    cmd.exe /c markdown-html CHANGELOG.md -o %finalDir%\Changelog.html > NUL
    echo Generate Terms
    cmd.exe /c markdown-html Terms.md -o %finalDir%\Terms.html > NUL

    echo Generate README
    cmd.exe /c markdown-html README.md -o %finalDir%\Readme.html > NUL
    cmd.exe /c markdown-html README.de.md -o %finalDir%\Readme.de.html > NUL
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
xcopy /y Terms.txt %finalDir% >nul 2>nul

echo Copy Published
xcopy /y %BIN_DIR% %finalDir% /E/H/C/I >nul 2>nul
)

rem echo Update Icon
rem tools\ResourceHacker.exe  -open %finalDir%\SoundSwitch.exe -save %finalDir%\SoundSwitch.exe -action addoverwrite -res SoundSwitch\Resources\Switch-SoundWave.ico -mask ICONGROUP,MAINICON,

echo Build Installer
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
