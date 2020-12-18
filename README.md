# NASR2SCT

## Authors: 
- Kyle Sanders - [Github Profile](https://github.com/KSanders7070)
- Nikolas Boling - [Github Profile](https://github.com/Nikolai558)

### THIS PROGRAM WILL:
- Pull data from the FAA NASR site and create Sector Files (.SCT) for virtual Radar Clients such as VRC by MetaCraft. As of now, it will create ALL: Weather Stations, Fixes, Airports, NAVAIDs, & Airways.
- It will also create the VERAM and VSTARS Airports.xml and Waypoints.xml files. 
- When appropriate, it will also create an In-Scope Reference (ISR) alias (.TXT) file for the data parsed.

### NOTES:
- Startup time is significantly reduced if "CUrl" is installed on your machine.
- DME Only stations are placed into the VOR list.
- All Airways (LOW/HIGH) are placed into the same SCT File with a header of "HIGH AIRWAY". Seeing how the intent would never be to see all airways at the same time on the scope, but rather drawn as needed, it is acceptable to put them all under either the HIGH or LOW airways header.
- Weather Stations will appear anywhere between 0.1 to 2.0 miles offset. Only stations that corrispond with an airport will be shown.

### REQUIREMENTS:
- Windows OS only, (Will implement a MAC and Linux OS version Later)
- If your selected "Project Folder" already has previously output files in it, be sure to remove these or rename them prior to running the program again, otherwise it will overwrite the current files.
