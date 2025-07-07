@echo off

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo "*** Elevated privileges needed!! (Run as Administrator) ***"
	pause
    exit /b
)

setlocal

set "ReplayProgId=MvLO-QTools.Replay"
set "DefaultIconPath=%~dp0mvlo1.ico,0"
set "ExecutablePath=%2"

if "%ExecutablePath%"=="" (
	echo "*** Please specify second argument: path to NSMBVS executable. ***"
	pause
	exit /b
)

if "%1"=="-reg" (
    reg add "HKEY_CLASSES_ROOT\.mvlreplay" /ve /d "%ReplayProgId%" /f
    reg add "HKEY_CLASSES_ROOT\%ReplayProgId%" /ve /d "MvLO Match Replay" /f
    reg add "HKEY_CLASSES_ROOT\%ReplayProgId%\DefaultIcon" /ve /d "%DefaultIconPath%" /f
    reg add "HKEY_CLASSES_ROOT\%ReplayProgId%\shell\open\command" /ve /d "\"%ExecutablePath%\" -replay \"%%1\"" /f
    echo "*** Registration process finished!! See status above. ***"
    goto askexplorerrestart
)

if "%1"=="-dereg" (
    reg delete "HKEY_CLASSES_ROOT\.mvlreplay" /f
    reg delete "HKEY_CLASSES_ROOT\%ReplayProgId%" /f
    echo "*** Unregistration process finished!! See status above. ***"
    goto askexplorerrestart
)

echo "*** Please specify first argument: '-reg' to add file type associations, '-dereg' to remove. ***"
pause
exit /b


:askexplorerrestart
	set /P c=File Explorer must be restarted for changes to apply. Do so now? [Y] for Yes.
	if /I "%c%" EQU "y" (
		taskkill /im explorer.exe /f
		start explorer.exe
	)
	exit /b
