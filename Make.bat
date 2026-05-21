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

setlocal enabledelayedexpansion
cd /d "%~dp0"

set buildPlatform=Release
if "%~1" neq "" (
    set buildPlatform=%~1
)

set FILE_DIR=%~dp0

rem Extract TargetFramework from .csproj using XML parsing
for /f "delims=" %%a in ('findstr /C:"<TargetFramework>" "%FILE_DIR%SoundSwitch\SoundSwitch.csproj"') do (
    set "line=%%a"
    set "line=!line:*<TargetFramework>=!"
    set "FRAMEWORK=!line:</TargetFramework>=!"
)

echo Building SoundSwitch %buildPlatform% for %FRAMEWORK%

set ARCH=win-x64

set finalDir=%FILE_DIR%Final

@REM rem Check if required commands exist
@REM where markdown-html >nul 2>nul
@REM set /a buildChangelogAndReadme=!ERRORLEVEL!
@REM if %buildChangelogAndReadme% == 0 (
@REM     echo markdown-html command not found, building with dummy Changelog and README!
@REM )
set /a buildChangelogAndReadme=1

echo Cleaning build directories
rmdir /q /s bin >nul 2>nul
rmdir /q /s obj >nul 2>nul
rmdir /q /s Release >nul 2>nul

rmdir /q /s "%finalDir%" >nul 2>nul
mkdir "%finalDir%" >nul 2>nul

for %%R in (win-x64 win-arm64) do (
    mkdir "%finalDir%\%%R" >nul 2>nul

    echo Build SoundSwitch.CLI %%R publish
    dotnet publish -c %buildPlatform% -r %%R --self-contained false "%FILE_DIR%SoundSwitch.CLI\SoundSwitch.CLI.csproj" -o "%finalDir%\SoundSwitch.CLI-publish-%%R" || (set errorMessage=Build CLI %%R failed & goto ERROR_QUIT)
    xcopy /y /e "%finalDir%\SoundSwitch.CLI-publish-%%R\*" "%finalDir%\%%R\" >nul 2>nul || (set errorMessage=Copy CLI %%R payload failed & goto ERROR_QUIT)
    rmdir /q /s "%finalDir%\SoundSwitch.CLI-publish-%%R" >nul 2>nul || (set errorMessage=Cleanup CLI %%R payload failed & goto ERROR_QUIT)

    echo Build SoundSwitch %%R publish
    dotnet publish -c %buildPlatform% -r %%R --self-contained false "%FILE_DIR%SoundSwitch\SoundSwitch.csproj" -o "%finalDir%\SoundSwitch-publish-%%R" || (set errorMessage=Build SoundSwitch %%R failed & goto ERROR_QUIT)
    xcopy /y /e "%finalDir%\SoundSwitch-publish-%%R\*" "%finalDir%\%%R\" >nul 2>nul || (set errorMessage=Copy SoundSwitch %%R payload failed & goto ERROR_QUIT)
    rmdir /q /s "%finalDir%\SoundSwitch-publish-%%R" >nul 2>nul || (set errorMessage=Cleanup SoundSwitch %%R payload failed & goto ERROR_QUIT)
)


echo.

if %buildChangelogAndReadme% == 1 (
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

echo Copy CLI README
xcopy /y  SoundSwitch.CLI\README.md %finalDir% >nul 2>nul

echo Copy LICENSE
xcopy /y LICENSE.txt %finalDir% >nul 2>nul
xcopy /y Terms.txt %finalDir% >nul 2>nul

echo Build Installer
rem Run installer compiler script
call ./Installer/Make-Installer.bat %buildPlatform% "%~2"
if not %ERRORLEVEL% == 0 (set errorMessage=Make-installer.bat failed or not found & goto ERROR_QUIT)

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
