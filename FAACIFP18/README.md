# FAACIFP18

## Authors: 
- Jason Cochran - [Github Profile](https://github.com/FriedrichKayak)

### FUNCTION:
The purpose of this C# class library is to read parsed FAACIFP18 data from an assumed complete sqlite3 database and output aliases based on those procedures.

This assumes that the sqlite3 database is produced by a separate process. The GitHub repo below by JanC called parseCifp contains all the necessary parts to downloan the raw FAACIFP18 file from the FAA, then process it into the sqlite3 database. Once the sqlite3 database exists, it is used as an input into this library.

parseCifp by JanC on GitHuib:
https://github.com/JanC/parseCifp

### DEPENDENCIES:
This class library depends on Microsoft.Data.Sqlite commonly available from public NuGet repos.

### HELPFUL TOOLS:
To inspect and perform data analysis on the sqlite3 database, one may use SQLiteStudio, a free/open-source database analysis tool.
https://sqlitestudio.pl/

### REQUIREMENTS:
- Windows OS


