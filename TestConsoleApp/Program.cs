using ClassData.DataAccess;
using NASARData;
using NASRData.DataAccess;
using System;
using System.IO;
using System.Linq;

namespace NASR2SctConsole
{
    /// <summary>
    /// This is how our program will start. This is just a console APP
    /// No fancy GUI will be able to be ran from anything INSIDE this file.
    /// This is only to get the program started.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Anthing in this function will be run VERY FIRST. From here it will go where you need it to.
        /// ie. Start parsing fixes.
        /// </summary>
        /// <param name="args">No Arguments are required, but we do have to have this in here.</param>
        static void Main(string[] args)
        {
            // Call the function to print the program begining messages.
            PrintBeginingMsgs();

            // Store the user Artcc Code
            string Artcc = GetArtcc();

            // Store the Effective Date as a variable.
            string AiracEffectiveDate = GetAiracEffDate();

            // get the Users choice of Converting the East Cordinates. 
            ConvertEastCoords();

            // Get users choice if they want a Test File Or not. 
            CreateTestSctFile();

            // Get users choice of Project Dir.
            ChooseProjectFolder(AiracEffectiveDate);

            // Create Output Directorys
            CheckAndCreateDir();

            // Write the Info section on the Test Sector File. 
            GlobalConfig.WriteTestSctFile();

            // Start Processing all the data.
            StartProcess(AiracEffectiveDate, Artcc);

            // Print the final Message
            PrintFinalMessage();
        }

        private static void PrintFinalMessage() 
        {
            // Clear the Console Screen
            Console.Clear();

            // Print Message to Console Screen
            Console.WriteLine
                (
                "\n\n" +
                "\n  ********" +
                "\n    DONE" +
                "\n  ********\n" +
                "\n  You may find your files HERE:\n" +
                $"\n  {GlobalConfig.outputDirectory}" +
                $"\n       {GlobalConfig.outputDirectory.Split('\\').Last()}\\ISR" +
                $"\n       {GlobalConfig.outputDirectory.Split('\\').Last()}\\VRC" +
                $"\n       {GlobalConfig.outputDirectory.Split('\\').Last()}\\VSTARS" +
                $"\n       {GlobalConfig.outputDirectory.Split('\\').Last()}\\VERAM" +
                $"\n       {GlobalConfig.outputDirectory.Split('\\').Last()}\\Test_Sct_File.sct2\n\n" +
                "\n * * * * * * * * * * * * * * * * * * * * * * * * " +
                "\n  AUTHORS:\n" +
                "\n          Kyle Sanders  (VATSIM CID 1187148)" +
                "\n          https://github.com/KSanders7070 \n\n" +
                "\n          Nikolas Boling  (VATSIM CID 1474952)" +
                "\n          https://github.com/Nikolai558 \n" +
                "\n * * * * * * * * * * * * * * * * * * * * * * * * \n\n" +
                "\n ...PRESS ENTER TO EXIT"
                );

            // Wait for user to hit Enter
            Console.ReadLine();
        }

        /// <summary>
        /// Function to tell the user What part of the program we are at.
        /// </summary>
        /// <param name="ParseingWhat">String for where we are.</param>
        private static void TellUserWhatWeAreDoing(string ParseingWhat) 
        {
            // Clear the Console Screen
            Console.Clear();

            // Write a message to the user telling them what we are doing. 
            Console.WriteLine
                (
                "\n\n" +
                $"\n  {string.Concat(Enumerable.Repeat("*", ParseingWhat.Length + 21))}" +
                $"\n  PARSING & EXPORTING: {ParseingWhat}" +
                $"\n  {string.Concat(Enumerable.Repeat("*", ParseingWhat.Length + 21))}\n\n"
                );
        }

        /// <summary>
        /// Quarterback function of our Console Command. This will start the Download, Parse, and Export of our program.
        /// </summary>
        /// <param name="AiracEffectiveDate">Format: YYYY-MM-DD</param>
        /// <param name="Artcc">User Facility ID</param>
        private static void StartProcess(string AiracEffectiveDate, string Artcc) 
        {
            // Print Custom Message to Console Screen.
            TellUserWhatWeAreDoing("FIXES");

            // We need to INSTANTIATE our GetFixData CLASS, SO THAT we can call the functions INSIDE that class.
            GetFixData ParseFixes = new GetFixData();
            ParseFixes.FixQuarterbackFunc(AiracEffectiveDate);

            // Print Custom Message to Console Screen.
            TellUserWhatWeAreDoing("HIGH AND LOW AIRWAYS");

            // Instantiate and call our functions that deal with AWY's 
            GetAwyData ParseAWY = new GetAwyData();
            ParseAWY.AWYQuarterbackFunc(AiracEffectiveDate);

            // Print Custom Message to Console Screen.
            TellUserWhatWeAreDoing("NDBS AND VORS");

            // Instantiate and call our functions that deal with NDB's / VOR's
            GetNavData ParseNavData = new GetNavData();
            ParseNavData.NAVQuarterbackFunc(AiracEffectiveDate, Artcc);

            // Print Custom Message to Console Screen.
            TellUserWhatWeAreDoing("AIRPORTS AND WEATHER STATIONS");

            // Instantiate and call our functions that deal with APT / WXL 
            GetAptData ParseAPT = new GetAptData();
            ParseAPT.APTQuarterbackFunc(AiracEffectiveDate, Artcc, "11579568");

            // Print Custom Message to Console Screen.
            TellUserWhatWeAreDoing("WAYPOINTS.XML");

            // Write the Waypoints.xml File. 
            GlobalConfig.WriteWaypointsXML();
            GlobalConfig.AppendCommentToXML(AiracEffectiveDate);
        }

        /// <summary>
        /// Check to see if the Directories needed for this exist already
        /// If they do then the user needs to Move, Rename, or Delete them
        /// Then Creates the directories we need in the ouput folder.
        /// If we dont do this then the Program will overwrite anything in these folders. 
        /// </summary>
        private static void CheckAndCreateDir() 
        {
            // Boolean to keep track if the dir exist. 
            bool dirExists = false;

            // MSG to display the folders that DO exists
            string msg = "\n";

            // Check all the directories that this program uses.
            if (Directory.Exists($"{GlobalConfig.outputDirectory}\\ISR"))
            {
                msg += $"\n  {GlobalConfig.outputDirectory}\\ISR";
                dirExists = true;
            }
            if (Directory.Exists($"{GlobalConfig.outputDirectory}\\VRC"))
            {
                msg += $"\n  {GlobalConfig.outputDirectory}\\VRC";
                dirExists = true;
            }
            if (Directory.Exists($"{GlobalConfig.outputDirectory}\\VSTARS"))
            {
                msg += $"\n  {GlobalConfig.outputDirectory}\\VSTARS";
                dirExists = true;
            }
            if (Directory.Exists($"{GlobalConfig.outputDirectory}\\VERAM"))
            {
                msg += $"\n  {GlobalConfig.outputDirectory}\\VERAM";
                dirExists = true;
            }

            // Check the boolean set up above.
            if (dirExists)
            {
                // Clear the Console Screen
                Console.Clear();

                // Display message to the user.
                Console.WriteLine
                    (
                    "\n\n" +
                    "\n  * * * * * * * * * * * *" +
                    "\n  *         STOP        *" +
                    "\n  * * * * * * * * * * * *\n\n" +
                    "\n  THE FOLLOWING FOLDER(S) HAVE BEEN FOUND IN THE DIRECTORY YOU HAVE CHOOSEN.\n\n\n" +
                    "\n  --------------------------" +
                    $"{msg}\n" +
                    "\n  --------------------------\n\n" +
                    "\n  PLEASE DELETE or RENAME THOSE FOLDER(S) PRIOR TO CONTINUING.\n\n\n\n" +
                    "\n  Press Enter to Continue"
                    );

                // Wait for the user to hit Enter
                Console.ReadLine();

                // Check the directories again. 
                CheckAndCreateDir();
            }
            else
            {
                // No directories found that conflict with our program, create them now.
                GlobalConfig.createDirectories();
            }
        }

        /// <summary>
        /// Grab the Project Directory from the user.
        /// </summary>
        /// <param name="EffectDate">Airac Effective Date</param>
        private static void ChooseProjectFolder(string EffectDate) 
        {
            // Display message to user
            Console.WriteLine
                (
                "\n * * * * * * * * * * * * *" +
                "\n   AIRAC EFFECTIVE DATE:" +
                $"\n   {EffectDate}" +
                "\n * * * * * * * * * * * * *\n\n" +
                "\n -----------------------------\n" +
                "\n  Choose Project Folder\n" +
                "\n  Input Should look like:" +
                "\n  C:\\Users\\user\\Downloads\n" +
                "\n -----------------------------\n\n" +
                "\n  All files will be saved here.\n\n" +
                "\n  Copy and Paste or Type out the" +
                "\n  directory path:"
                );

            // get the directory from the user
            string outputPath = Console.ReadLine();

            // Clear the Console Screen
            Console.Clear();

            // Check the user input.
            if (Directory.Exists(outputPath))
            {
                GlobalConfig.outputDirectory = outputPath;
            }
            else if (Directory.Exists(outputPath) == false)
            {
                Console.Clear();
                Console.WriteLine
                    (
                    "\n  Directory Does Not Exist:" +
                    $"\n    {outputPath}\n\n" +
                    "\n  Press Enter To Try Again."
                    );
                Console.ReadLine();
                ChooseProjectFolder(EffectDate);
            }
            else
            {
                Console.Clear();
                Console.WriteLine
                    (
                    "\n  Directory Does Not Exist:" +
                    $"\n    {outputPath}\n\n" +
                    "\n  Press Enter To Try Again."
                    );
                Console.ReadLine();
                ChooseProjectFolder(EffectDate);
            }
        }

        /// <summary>
        /// Get the users input if they want a test sector file. this does nothing right now. 
        /// </summary>
        private static void CreateTestSctFile() 
        {
            // TODO - Actually Do something with CreateTestSctFile

            // Display message to user
            Console.WriteLine
                (
                "\n\n" +
                "\n -----------------------------------------------\n" +
                "\n  Do you wish to have this batch file create a" +
                "\n  'TEST_SECTOR.sct2' at the end of the process?\n\n" +
                "\n  This program ALWAYS creates a Test file" +
                "\n  No Mater what you choose here.\n" +
                "\n -----------------------------------------------\n\n" +
                "\n  Type Y or N and press Enter:"
                );

            // Get the users choice
            string choice = Console.ReadLine();

            // Clear the Console Screen
            Console.Clear();

            // Check the users input.
            if (choice.Trim().ToLower() == "y")
            {
                // TODO - Set Global Variable to Create the Test Sector File to True
            }
            else if (choice.Trim().ToLower() == "n")
            {
                // TODO - Set Global Variable to Create the Test Sector File to False
            }
            else
            {
                CreateTestSctFile();
            }
        }

        /// <summary>
        /// Get user input on weather or not they want to Convert East coordinates to West Cordinates.
        /// </summary>
        /// <returns></returns>
        private static void ConvertEastCoords() 
        {
            // Print a message and wait for user input
            Console.WriteLine
                (
                "\n\n" +
                "\n -------------------------------------------------------------\n" +
                "\n  VRC and other similar virtual RADAR Clients have a tendency" +
                "\n  to put East Delincation points on the far right side of the" +
                "\n  scope.\n" +
                "\n  Do you wish to convert these East declinations to 'West'" +
                "\n  and the associated coordinates in order to 'Trick' your" +
                "\n  RADAR client into putting them on the left side of your" +
                "\n  scope?\n\n" +
                "\n  Example:\n" +
                "\n          ORIGINAL:   AAMYY N051.30.18.800 E171.09.11.700\n" +
                "\n         CONVERTED:   AAMYY N051.30.18.800 W188.50.48.300\n" +
                "\n -------------------------------------------------------------\n\n" +
                "\n  Type Y or N and press Enter:"
                );

            // Get the users choice.
            string convert = Console.ReadLine();

            // Clear the Console Screen.
            Console.Clear();

            // Check the users input.
            if (convert.Trim().ToLower() == "y")
            {
                GlobalConfig.Convert = true;
            }
            else if (convert.Trim().ToLower() == "n")
            {
                GlobalConfig.Convert = false;
            }
            else
            {
                ConvertEastCoords();
            }
        }

        /// <summary>
        /// Console Input from User, Get the Effective Airacc Date from the user
        /// </summary>
        /// <returns>Returns string in the format of "MM/DD/YYYY"</returns>
        private static string GetAiracEffDate() 
        {
            // Tell C# what all of our variables are going to be but don't set/INSTANTIATE them.
            string choice;
            string fullEffectiveDate = "";

            // Tell the user we are getting the dates from FAA
            Console.WriteLine("\n Getting AIRAC Effective Dates from FAA, Please wait.\n");

            // Check to see if we already have the Airac Effective dates.
            if (GlobalConfig.currentAiracDate == null || GlobalConfig.nextAiracDate == null)
            {
                // get the Airac effective dates from the FAA Website.
                GlobalConfig.GetAiracDateFromFAA();
            }

            // Clear the message about getting the dates.
            Console.Clear();

            // Write header message to the CMD.
            Console.WriteLine
                ("\n\n\n\n" +
                "\n ----------------------------------\n" +
                "\n  CHOOSE APPROPRIATE DATA TO PARSE\n" +
                "\n ----------------------------------\n\n\n\n" +
                $"\n   C  -  Current AIRAC   (Effective Date: {GlobalConfig.currentAiracDate})\n" +
                $"\n   N  -  Next AIRAC      (Effective Date: {GlobalConfig.nextAiracDate})\n\n\n\n" +
                "\n Type either C or N and press ENTER:"
                );

            // get the users choice.
            choice = Console.ReadLine();

            if (choice.Trim().ToLower() == "c")
            {
                fullEffectiveDate = GlobalConfig.currentAiracDate;
            }
            else if (choice.Trim().ToLower() == "n")
            {
                fullEffectiveDate = GlobalConfig.nextAiracDate;
            }
            else
            {
                GetAiracEffDate();
            }

            // Clear the Console Screen
            Console.Clear();

            // Return the Effective date in the format of YYYY-MM-DD
            return fullEffectiveDate;
        }

        /// <summary>
        /// Ask the user what their artcc code is.
        /// </summary>
        /// <returns>Returns the string the user put in.</returns>
        private static string GetArtcc() 
        {
            // Variable to store what the user inputs. Don't set it yet. 
            string artcc;

            // Display information to the user
            Console.WriteLine
                ("\n\n\n\n" +
                "\n ------------------------------------\n" +
                "\n  Type the ID of your ARTCC/Facility\n" +
                "\n ------------------------------------\n\n" +
                "\n - Blank spaces will be removed automatically.\n" +
                "\n - This is only used to help name your output files and ISR returns.\n\n" +
                "\n Type 3 letter ID of your ARTCC and press Enter:"
                );

            // Grab the data from the user input.
            artcc = Console.ReadLine();

            // Clear the Console Screen
            Console.Clear();

            if (string.IsNullOrEmpty(artcc.Trim()))
            {
                GetArtcc();
            }

            // return the data
            return artcc.Trim();
        }

        /// <summary>
        /// Prints the begining messages for the Console.
        /// </summary>
        private static void PrintBeginingMsgs() 
        {
            // Print the very begining message.
            Console.WriteLine
                (
                "\n  THIS CONSOLE PROGRAM WILL:\n" +
                "\n  1) Pull data from the FAA NASR site and create Sector Files (.SCT) for virtual Radar Clients" +
                "\n     such as VRC by MetaCraft.\n" +
                "\n  2) When appropriate, it will also create an In-Scope Reference (ISR) alias (.TXT) file for" +
                "\n     the data parsed.\n\n" +
                "\n  NOTES:\n" +
                "\n   - DME Only stations are placed into the VOR list.\n" +
                "\n   - All Airways (LOW/HIGH) are placed into the same SCT File. Seeing how the intent would never" +
                "\n     be to see all airways at the same time on the scope, but rather drawn as needed, it is" +
                "\n     acceptable to put them all under either the HIGH or LOW airways header.\n" +
                "\n   - This parser can take 10 min or longer to complete. Factors such as individual computer" +
                "\n     performance come into play with the duration of this process.\n" +
                "\n   - Added Wx Stations as a 'Label' for VRC STATIC TEXT menu. Note- due to limitations of .BAT files," +
                "\n     Weather Stations will appear anywhere between 0.1 to 2.0 miles offset.\n" +
                "\n  Press Enter to continue."
                );

            // Wait for the user to press enter.
            Console.ReadLine();

            // Clear the Console Window.
            Console.Clear();
        }
    }
}
