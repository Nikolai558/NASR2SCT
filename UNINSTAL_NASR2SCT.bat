@echo off

ECHO.
ECHO.

CD "%temp%"
	if NOT exist NASR2SCT ECHO COULD NOT REMOVE BECAUSE WAS NOT FOUND:  temp\NASR2SCT
	if exist NASR2SCT ( RD /Q /S "NASR2SCT" && ECHO Removed:  temp\NASR2SCT )

ECHO.
ECHO.

CD "%userprofile%\AppData\Local"
	if NOT exist NASR2SCT ECHO COULD NOT REMOVE BECAUSE WAS NOT FOUND:  AppData\Local\NASR2SCT
	if exist NASR2SCT ( RD /Q /S "NASR2SCT" && ECHO Removed:  AppData\Local\NASR2SCT )

ECHO.
ECHO.

CD "%userprofile%\Desktop"
	if NOT exist NASR2SCT.lnk ECHO COULD NOT REMOVE BECAUSE WAS NOT FOUND:  Desktop\NASR2SCT.lnk -Shortcut-
	if exist NASR2SCT.lnk ( DEL /Q "NASR2SCT.lnk" && ECHO Removed:  Desktop\NASR2SCT.lnk -SHORTCUT- )

ECHO.
ECHO.
ECHO.
ECHO.
ECHO.
ECHO ...PRESS ANY KEY TO EXIT

PAUSE>NUL
