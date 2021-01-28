@echo off

TITLE NASR_2_SCT UNINSTALL

SET /A NOT_FOUND_COUNT=0

CD "%temp%"
	if NOT exist NASR2SCT (
		SET /A NOT_FOUND_COUNT=%NOT_FOUND_COUNT% + 1
		SET NASR_TEMP_FOLDER=NOT_FOUND
	)
	
	if exist NASR2SCT (
		SET NASR_TEMP_FOLDER=FOUND
		RD /Q /S "NASR2SCT"
	)

CD "%userprofile%\AppData\Local"
	if NOT exist NASR2SCT (
		SET /A NOT_FOUND_COUNT=%NOT_FOUND_COUNT% + 1
		SET NASR_APPDATA_FOLDER=NOT_FOUND
	)
	
	if exist NASR2SCT (
		SET NASR_APPDATA_FOLDER=FOUND
		RD /Q /S "NASR2SCT"
	)

CD "%userprofile%\Desktop"
	if NOT exist NASR2SCT.lnk (
		SET /A NOT_FOUND_COUNT=%NOT_FOUND_COUNT% + 1
		SET NASR_SHORTCUT=NOT_FOUND
	)

	if exist NASR2SCT.lnk (
		SET NASR_SHORTCUT=FOUND
		DEL /Q "NASR2SCT.lnk"
	)

IF %NOT_FOUND_COUNT%==0 SET UNINSTALL_STATUS=COMPLETE
IF %NOT_FOUND_COUNT%==1 SET UNINSTALL_STATUS=PARTIAL
IF %NOT_FOUND_COUNT%==2 SET UNINSTALL_STATUS=PARTIAL
IF %NOT_FOUND_COUNT%==3 SET UNINSTALL_STATUS=FAIL

IF %UNINSTALL_STATUS%==COMPLETE GOTO UNINSTALLED
IF %UNINSTALL_STATUS%==PARTIAL GOTO UNINSTALLED
IF %UNINSTALL_STATUS%==FAIL GOTO FAILED

CLS

:UNINSTALLED

ECHO.
ECHO.
ECHO SUCCESSFULLY UNINSTALLED THE FOLLOWING:
ECHO.
IF %NASR_TEMP_FOLDER%==FOUND ECHO        -temp\NASR2SCT
IF %NASR_APPDATA_FOLDER%==FOUND ECHO        -AppData\Local\NASR2SCT
IF %NASR_SHORTCUT%==FOUND ECHO        -Desktop\NASR2SCT Shortcut

:FAILED

IF NOT %NOT_FOUND_COUNT%==0 (
	ECHO.
	ECHO.
	ECHO.
	ECHO.
	IF %UNINSTALL_STATUS%==PARTIAL ECHO NOT ABLE TO COMPLETELY UNINSTALL BECAUSE THE FOLLOWING COULD NOT BE FOUND:
	IF %UNINSTALL_STATUS%==FAIL ECHO UNINSTALL FAILED COMPLETELY BECAUSE THE FOLLOWING COULD NOT BE FOUND:
	ECHO.
	IF %NASR_TEMP_FOLDER%==NOT_FOUND ECHO        -temp\NASR2SCT
	IF %NASR_APPDATA_FOLDER%==NOT_FOUND ECHO        -AppData\Local\NASR2SCT
	IF %NASR_SHORTCUT%==NOT_FOUND (
		ECHO        -Desktop\NASR2SCT Shortcut
		ECHO             --If the shortcut was renamed, delete the shortcut manually.
	)
)

ECHO.
ECHO.
ECHO.
ECHO.
ECHO.
ECHO ...PRESS ANY KEY TO EXIT

PAUSE>NUL
