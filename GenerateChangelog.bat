REM Use the following tool: https://github.com/skywinder/github-changelog-generator

SETLOCAL
SET "FILE_DIR=%~dp0"

IF NOT EXIST "%FILE_DIR%..\github-key.txt" (
  ECHO %~nx0: %FILE_DIR%..\github-key.txt is not present!
  SET SIGN_ERROR=True
  GOTO END
)

SET KEY=
SET /P KEY=<"%FILE_DIR%..\github-key.txt"

github_changelog_generator -t %KEY%


:END
IF /I "%SIGN_ERROR%" == "True" (
  IF "%~1" == "" PAUSE
  ENDLOCAL
  EXIT /B 1
)
ENDLOCAL
EXIT /B