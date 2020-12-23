## Steps to release and push out a new update.

1. Change program Assembly info inside of Visual Studios.
	- This is located in NASR_GUI under Properties
2. Change Program version info inside of GlobalConfig.
	- This is located in NASRData
3. Clean Solution, Build Solution
	- Be sure Releases is checked and NOT debug.
4. Create NUGET package
	- Open "NuGet Package Exploror
	- Open previous version NuGet package
		- Change Version Number
		- Drag and drop EVERYTHING in the NASR_GUI/Bin/Releases into the NuGet lib/net45 folder. (DO NOT INCLUDE .pdb files)
		- It will yell at you saying are you sure you want to override the current files in the NUGET package.
		- Click Yes and Repeat.
        - Verify that the Properties Folder made it into the NUGET Package
	- Save the NuGet Package as "NASR2SCT.x.x.x.nupkg" where x.x.x is the New Version Number
5. Go back into Visual Studios and inside the Package Manager, type the command "Squirrel --releasify NASR2SCT.x.x.x.nupkg" where x.x.x is the New Version Number
	- This will create or overide anything in the Solution Directory/Releases Directory.
6. Go to github
    - Publish source code on the DEVELOPMENT branch
        - NEVER PUBLISH SOURCE CODE TO RELEASE BRANCH. THIS MUST BE DONE THROUGH A MERGE.
    - Create a pull request for RELEASES BRANCH
    - Merge DEVELOPMENT into the RELEASE Branch. 
7. Ready to publish the release
    - Go to Releases on github, draft new release.
    - Put in the info.
    - Copy ALL FILES from the Solution/Releases Directory into the github release
    - Publish the release.