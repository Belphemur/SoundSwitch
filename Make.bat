@echo off
rem SoundSwitch Make file
rem
rem Compiles SoundSwitch binaries and installer.
rem
rem When the Final\ directory already exists (populated by
rem tools\Download-Release.ps1), the build and HTML generation steps are
rem skipped and only signing + installer creation are performed.
rem
rem Requires:
rem   - Python 3 with the 'markdown' package (pip install markdown)
rem   - Inno Setup 6 (for installer creation)

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

set ARCH=win-x64
set finalDir=%FILE_DIR%Final

rem ── Check if Final\ was already populated by Download-Release.ps1 ──
if exist "%finalDir%\SoundSwitch.exe" (
    echo.
    echo Final\ directory already contains binaries.
    echo Skipping build and HTML generation ^(use tools\Download-Release.ps1 to refresh^).
    echo.
    goto SIGN
)

echo Building SoundSwitch %buildPlatform% for %FRAMEWORK%

echo Cleaning build directories
rmdir /q /s bin >nul 2>nul
rmdir /q /s obj >nul 2>nul
rmdir /q /s Release >nul 2>nul

rmdir /q /s %finalDir% >nul 2>nul
mkdir %finalDir% >nul 2>nul

echo Build AnyCPU
dotnet publish -c %buildPlatform% %FILE_DIR%SoundSwitch.CLI\SoundSwitch.CLI.csproj -o %finalDir% || (set errorMessage=Build CLI failed & goto ERROR_QUIT)
dotnet publish -c %buildPlatform% %FILE_DIR%SoundSwitch\SoundSwitch.csproj -o %finalDir% || (set errorMessage=Build SoundSwitch failed & goto ERROR_QUIT)

echo.

rem ── Generate HTML documentation using Python markdown tool ──
where python >nul 2>nul
if %ERRORLEVEL% == 0 (
    echo Generate Changelog
    python tools\markdown_to_html.py CHANGELOG.md -o %finalDir%\Changelog.html || (set errorMessage=Changelog HTML generation failed & goto ERROR_QUIT)

    echo Generate Terms
    python tools\markdown_to_html.py Terms.md -o %finalDir%\Terms.html || (set errorMessage=Terms HTML generation failed & goto ERROR_QUIT)

    echo Generate README
    python tools\markdown_to_html.py README.md -o %finalDir%\Readme.html || (set errorMessage=README HTML generation failed & goto ERROR_QUIT)

    if exist README.de.md (
        python tools\markdown_to_html.py README.de.md -o %finalDir%\Readme.de.html || (set errorMessage=README.de HTML generation failed & goto ERROR_QUIT)
    )
) else (
    echo WARNING: Python not found, generating dummy HTML files
    echo ^<html^>^<body^>^<h1^>Dummy Changelog, Python with markdown is required^</h1^>^</body^>^</html^> > %finalDir%\Changelog.html
    echo ^<html^>^<body^>^<h1^>Dummy README, Python with markdown is required^</h1^>^</body^>^</html^> > %finalDir%\Readme.html
)

echo Copy soundSwitched image
xcopy /y img\soundSwitched.png %finalDir% >nul 2>nul

echo Copy CLI README
xcopy /y  SoundSwitch.CLI\README.md %finalDir% >nul 2>nul

echo Copy LICENSE
xcopy /y LICENSE.txt %finalDir% >nul 2>nul
xcopy /y Terms.txt %finalDir% >nul 2>nul

:SIGN
echo.
echo Signing binaries...
rem Sign main executables if signinfo.txt exists
if exist "%FILE_DIR%signinfo.txt" (
    call "%FILE_DIR%Sign.bat" "%finalDir%\SoundSwitch.exe"
    call "%FILE_DIR%Sign.bat" "%finalDir%\SoundSwitch.CLI.exe"
) else (
    echo WARNING: signinfo.txt not found, skipping code signing.
)

echo.
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
