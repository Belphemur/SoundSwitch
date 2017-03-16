@echo off
REM *
REM * Removes various informations (like absolute file paths) from the Zeta Resource Editor project file.
REM * Run this script if you opened the *.zreproj project file at least once.
REM *
set filename="SoundSwitch.zreproj"

setlocal
setlocal enableDelayedExpansion
if not exist %filename% call :error "file"
findstr /v "<userSetting </userSetting>" %filename% >%filename%.nouserdata || call :error %errorlevel%, "user"
type "%filename%.nouserdata"|repl "absoluteFilePath=\q.*\q (/>)" "$1" x >%filename%.clean
if not %errorlevel%==0 call :error %errorlevel%, "abspath"
move /y %filename%.clean %filename% || call :error
del %filename%.nouserdata || call :error

echo %filename% successfully cleaned.
goto :end

:error
if "%~1"=="file" (
    echo %filename% doesn't exist.
) else if "%~1"=="1" (
    if "%~2"=="user" (
	    echo userSettings not found, skipped.
	) else if "%~2"=="abspath" (
	    echo absoluteFilePath attribute not found, skipped.
	)
) else (
    echo Unknown error occured.
	goto :end
)
exit /b 0

:end
pause
exit