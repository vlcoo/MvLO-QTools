@echo off

:: registering file types (3 goals):
:: * - file type description
:: ** - file icon in explorer
:: *** - open with a specific executable
:: 1. add to HKEY_CLASSES_ROOT a key with the extension, with value a meaningful name A (ProgID)
:: 2. add to HKEY_CLASSES_ROOT a key with the name A, with the file type description as value *
:: 3. add to HKEY_CLASSES_ROOT a key with the name A\DefaultIcon, with icon path as value **
:: 4. add to HKEY_CLASSES_ROOT a key with the name A\shell\open\command, with the executable path as value (followed by something like \"%1\") ***

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo "*** Elevated privileges needed!! (Run as Administrator) ***"
	pause
    exit /b
)

setlocal

set "ReplayProgId=MvLO-QTools.Replay"
set "DefaultIconPath=C:\ShellExtensions\mvlo1.ico,0"
set "ExecutablePath=C:\Users\Victor\Projects\Unity\VicMvsLO\Build\vanilla-patched\NSMB-MarioVsLuigi.exe"

if "%1"=="-reg" (
    reg add "HKEY_CLASSES_ROOT\.mvlreplay" /ve /d "%ReplayProgId%" /f
    reg add "HKEY_CLASSES_ROOT\%ReplayProgId%" /ve /d "MvLO Match Replay" /f
    reg add "HKEY_CLASSES_ROOT\%ReplayProgId%\DefaultIcon" /ve /d "%DefaultIconPath%" /f
    reg add "HKEY_CLASSES_ROOT\%ReplayProgId%\shell\open\command" /ve /d "\"%ExecutablePath%\" -replay \"%%1\"" /f
    echo "*** Registration process finished!! See status above. ***"
    goto askexplorerrestart
)

if "%1"=="-dereg" (
    echo Unregistering file type...
    reg delete "HKEY_CLASSES_ROOT\.mvlreplay" /f
    reg delete "HKEY_CLASSES_ROOT\%ReplayProgId%" /f
    echo "*** Unregistration process finished!! See status above. ***"
    goto askexplorerrestart
)

echo "*** Please specify argument: '-reg' to add file type associations, '-dereg' to remove. ***"
pause
exit /b


:askexplorerrestart
	set /P c=File Explorer must be restarted for changes to apply. Do so now? [Y] for Yes.
	if /I "%c%" EQU "y" (
		taskkill /im explorer.exe /f
		start explorer.exe
	)
	exit /b
