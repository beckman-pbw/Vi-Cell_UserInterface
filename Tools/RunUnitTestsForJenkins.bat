@ECHO off 
powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command %CD%\tools\RunUnitTestsForJenkins.ps1
ECHO RunUnitTestsForJenkins.ps1 ExitCode - %ERRORLEVEL%
Exit %ERRORLEVEL% /b