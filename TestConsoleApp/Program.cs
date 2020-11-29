using ClassData.DataAccess;
using NASARData;
using NASRData.DataAccess;
using System;


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
            //new GlobalConfig().CorrectLatLon("51-53-00.8980N", true);

            // Track timestamps
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "Program started")}: { DateTime.Now}");

            // Call the function to print the program begining messages.
            PrintBeginingMsgs();

            // Store the user Artcc Code
            string Artcc = GetArtcc();
            //string Artcc = "ZLC";

            // Store the Effective Date as a variable.
            string AiracEffectiveDate = GetAiracEffDate();
            //string AiracEffectiveDate = "12/03/2020";

            // TODO - ConvertEastCoord Does nothing right now, and goes nowhere.
            string ConvertEastCoord = ConvertEastCoords();
            //string ConvertEastCoord = "y";

            if (ConvertEastCoord.ToLower().Trim() == "y")
            {
                GlobalConfig.Convert = true;
            }

            // We need to INSTANTIATE our GetFixData CLASS, SO THAT we can call the functions INSIDE that class.
            // Think of this as Batch file. (Terrible example below)
            // 1. You write this amazing batch file. IT DOES EVERYTHING. 
            // 2. You then create a COMPLETELY SEPERATE BATCH FILE to do something else.
            // 3. your SECOND batch file uses the first one but the first one is only on github.
            // 4. IN ORDER To run your second batch file, you have to first download your first batch file and remember where it is.
            GetFixData ParseFixes = new GetFixData();

            // Now that we have INSTANTIATED our CLASS, lets actually call the function that will start everything.
            // This function requires 3 parameters. Below is HARD CODED in, But in reality we want to grab this from the user.
            ParseFixes.FixQuarterbackFunc(AiracEffectiveDate);

            GetAwyData ParseAWY = new GetAwyData();
            ParseAWY.AWYQuarterbackFunc(AiracEffectiveDate);

            // Instantiate and call our functions that deal with NDB's / VOR's
            GetNavData ParseNDBs = new GetNavData();
            ParseNDBs.NAVQuarterbackFunc(AiracEffectiveDate, Artcc);

            // Instantiate and call our functions that deal with APT / WXL 
            GetAptData ParseAPT = new GetAptData();
            ParseAPT.APTQuarterbackFunc(AiracEffectiveDate, Artcc, "11579568");

            // Track timestamps.
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "Program ended")}: {DateTime.Now}");

            // We stored all the data for the timestamps now we just have to write it to the file.
            new GlobalConfig().WriteTimeStamps();
        }

        /// <summary>
        /// Get user input on weather or not they want to Convert East coordinates to West Cordinates.
        /// </summary>
        /// <returns></returns>
        private static string ConvertEastCoords() 
        {
            // Track timestamps.
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "ConvertEastCoords()")}: {DateTime.Now}");

            // Print a message and wait for user input
            Console.WriteLine(GlobalConfig.msg3);
            Console.WriteLine();
            Console.WriteLine("Type Y or N and press Enter:");
            string convert = Console.ReadLine();
            Console.Clear();

            // TODO - Verify user input is good.

            // Return what the user put.
            return convert;
        }

        /// <summary>
        /// Console Input from User, Get the Effective Airacc Date from the user
        /// </summary>
        /// <returns>Returns string in the format of "MM/DD/YYYY"</returns>
        private static string GetAiracEffDate() 
        {
            // Track Time Stamps
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "GetAiracEffDate()")}: {DateTime.Now}");

            // Tell C# what all of our variables are going to be but don't set/INSTANTIATE them.
            string year;
            string month;
            string day;
            string fullEffectiveDate;

            // Write header message to the CMD.
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Type the Effictive date of the AIRAC in the following formats");
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();

            // User input for Airacc Effective Year
            Console.WriteLine("Type the 4 digit year:");
            year = Console.ReadLine();

            // User input for Airacc Effective Month
            Console.WriteLine("Type the 2 digit month:");
            month = Console.ReadLine();

            // User input for Airacc Effective Day
            Console.WriteLine("Type the 2 digit day:");
            day = Console.ReadLine();
            Console.Clear();

            // Combine into one string for our GET FIX function
            fullEffectiveDate = $"{month}/{day}/{year}";

            // TODO - Verify user input is good.

            // Return the Effective date in the format of MM/DD/YYYY
            return fullEffectiveDate;
        }

        /// <summary>
        /// Ask the user what their artcc code is.
        /// </summary>
        /// <returns>Returns the string the user put in.</returns>
        private static string GetArtcc() 
        {
            // Track Timestamps
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "GetArtcc()")}: {DateTime.Now}");

            // Variable to store what the user inputs. Don't set it yet. 
            string artcc;

            // Display information to the user
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Type 3 Letter ID of your ARTCC");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Type 3 letter ID of your ARTCC and press Enter:");

            // Grab the data from the user input.
            artcc = Console.ReadLine();
            Console.Clear();

            // TODO - Verify user input is good.

            // return the data
            return artcc;
        }

        /// <summary>
        /// Prints the begining messages for the program.
        /// </summary>
        private static void PrintBeginingMsgs() 
        {
            // Track Time Stamps
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "PrintBeginingMsgs()")}: {DateTime.Now}");

            // Print the messages.
            Console.WriteLine(GlobalConfig.msg1);
            Console.WriteLine();
            Console.WriteLine(GlobalConfig.msg2);
            Console.WriteLine();
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
