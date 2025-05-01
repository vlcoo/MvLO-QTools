@echo off

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo "*** Elevated privileges needed!! (Run as Administrator) ***"
	pause
    exit /b
)

setlocal

set "DllPath=%~dp0ReplayShellEx.combi.dll"
set "RegasmPath=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"

if "%1"=="-reg" (
    "%RegasmPath%" "%DllPath%" /codebase
    echo "*** Installation process finished!! See status above. ***"
    goto askexplorerrestart
)

if "%1"=="-dereg" (
    "%RegasmPath%" "%DllPath%" /unregister
    echo "*** Uninstallation process finished!! See status above. ***"
    goto askexplorerrestart
)

echo "*** Please specify argument: '-reg' to install shell extension server, '-dereg' to uninstall. ***"
pause
exit /b


:askexplorerrestart
	set /P c=File Explorer must be restarted for changes to apply. Do so now? [Y] for Yes.
	if /I "%c%" EQU "y" (
		taskkill /im prevhost.exe /f
		taskkill /im explorer.exe /f
		start explorer.exe
	)
	exit /b
