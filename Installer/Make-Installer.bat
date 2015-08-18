@echo off
setlocal
cd /d "%~dp0"

IF "%PROCESSOR_ARCHITECTURE%"=="x86" (set progfiles86=C:\Program Files) else (set progfiles86=C:\Program Files ^(x86^))
set innosetup="%progfiles86%\Inno Setup 5\ISCC.exe"
if not exist %innosetup% (
  echo Error: NSIS is not installed (NSIS v3.0b1^)
  goto Quit
)

del ..\Final\*Installer.exe
echo Making installer...

REM The Installer
%innosetup% setup.iss
if not "%ERRORLEVEL%"=="0" echo error: innosetup failed & goto Quit

goto Done
:Quit

exit /b 1

:Done