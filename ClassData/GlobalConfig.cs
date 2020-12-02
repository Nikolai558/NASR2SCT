using ClassData.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NASARData
{
    /// <summary>
    /// A Class to hold all of our "Global" configurations. ie. Temp Path.
    /// The reason we want this is so that we don't have to copy and paste these variables
    /// all throughout our program.
    /// </summary>
    public class GlobalConfig
    {
        // Current version of the program.
        public static readonly string ProgramVersion = "V-0.4.3-beta";
        
        // Github's Most recent release version
        public static string GithubVersion = "";

        // XML Serializer for our Waypoints.xml file.
        private static XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("Waypoints");
        public static XmlSerializer WaypointSerializer = new XmlSerializer(typeof(Waypoint[]), xmlRootAttribute);

        // Global Waypoints storage so we can collect all airports, fixes, vors, and ndbs for the Waypoints.xml file.
        public static Waypoint[] waypoints = new Waypoint[] { };

        // Store the Current and Next AIRAC effective Date.
        public static string currentAiracDate;
        public static string nextAiracDate;

        // Store the users Output directory choice. 
        public static string outputDirectory;

        // Store the users Choice if they want to convert East Cordinates.
        public static bool Convert = false;

        // Temp path for the user. ie: C:\Users\nik\AppData\Local\Temp\NASR_TO_SCT
        public static readonly string tempPath = $"{Path.GetTempPath()}NASR_TO_SCT";

        /// <summary>
        /// Get the Github Version from: https://raw.githubusercontent.com/Nikolai558/NASR2SCT/main/tempLatestVersion.txt
        /// </summary>
        public static void githubVersion()
        {
            // Create Webclient
            var client = new WebClient();

            // Download Github Version and store it in this variable.
            GithubVersion = client.DownloadString("https://raw.githubusercontent.com/Nikolai558/NASR2SCT/main/tempLatestVersion.txt");

            // The downloaded info has a /n at the end of it. We only want the Version info with no \n
            GithubVersion = GithubVersion.Split()[0];
        }

        /// <summary>
        /// Convert the Lat AND Lon from the FAA Data into a standard [N-S-E-W]000.00.00.000 format.
        /// </summary>
        /// <param name="value">Needs to have 3 decimal points, and one of the following Letters [N-S-E-W]</param>
        /// <param name="Lat">Is this value a Lat, if so Put true</param>
        /// <param name="ConvertEast">Do you need ALL East Coords converted, if so put true.</param>
        /// <returns>standard [N-S-E-W]000.00.00.000 lat/lon format</returns>
        public string CorrectLatLon(string value, bool Lat, bool ConvertEast) 
        {
            // Valid format is N000.00.00.000 W000.00.000
            string correctedValue = "";

            // Split the value based on these char
            char[] splitValue = new char[] { '.', '-'};

            // Declare our variables.
            string degrees;
            string minutes;
            string seconds;
            string milSeconds;
            string declination = "";

            // If the value is a Lat
            if (Lat)
            {
                // Find the Character "N"
                if (value.IndexOf("N", 0, value.Length) != -1)
                {
                    // Set Declination to N
                    declination = "N";

                    // Delete the N from the Value
                    value = value.Replace("N", "");
                }

                // Find the Char "S"
                else if (value.IndexOf("S", 0, value.Length) != -1)
                {
                    // Set declination to S
                    declination = "S";

                    // Remove the S from our Value
                    value = value.Replace("S", "");
                }

                // Split the Value by our chars we defined above.
                string[] valueSplit = value.Split(splitValue);

                // Set our Variables
                degrees = valueSplit[0];
                minutes = valueSplit[1];
                seconds = valueSplit[2];

                // Check the length of our Milliseconds. 
                if (valueSplit[3].Length > 3)
                {
                    // If its greater then 3 we only want to keep the first three.
                    milSeconds = valueSplit[3].Substring(0, 3);
                }
                else
                {
                    // our the length is less than or equal to three so just set it. 
                    milSeconds = valueSplit[3];
                }

                // Correct Format is now set.
                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";
            }

            // Value is a Lon
            else
            {
                // Check for "E" Char
                if (value.IndexOf("E", 0, value.Length) != -1)
                {
                    // Set Declination to E
                    declination = "E";

                    //Remove the E from our Value
                    value = value.Replace("E", "");
                }

                // Check for "W" char
                else if (value.IndexOf("W", 0, value.Length) != -1)
                {
                    // Set Declination to W
                    declination = "W";

                    // Remove the W from our Value.
                    value = value.Replace("W", "");
                }

                // Value does not have an E or W. 
                else
                {
                    // There has been an error in the value passed in.
                    return value;
                }

                // Split the value by our char we defined above.
                string[] valueSplit = value.Split(splitValue);

                // Set all of our variables.
                degrees = valueSplit[0];
                minutes = valueSplit[1];
                seconds = valueSplit[2];
                milSeconds = valueSplit[3];

                // Set the Corrected Value
                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";
            }

            // Check to see if Convert E is True and Check our Value's Declination to make sure it is an E Coord.
            if (ConvertEast && declination == "E")
            {
                // Make sure our miliseconds does not have more then 3 digits.
                if (milSeconds.Length >= 4)
                {
                    // if it does only gram the first three digits.
                    milSeconds = milSeconds.Substring(0, 3);
                }

                // Do some math for all of our Variables.
                degrees = ((179 - int.Parse(degrees)) + 179).ToString();
                minutes = ((59 - int.Parse(minutes)) + 59).ToString();
                seconds = ((59 - int.Parse(minutes)) + 59).ToString();
                milSeconds = ((1000 - int.Parse(milSeconds)) + 1000).ToString();
                
                // set the declination to W
                declination = "W";

                // Make sure our Variables are in the correct range. Loop 3 times to verify this. (i.e. Miliseconds can be no larger than 999)
                for (int i = 0; i < 3; i++)
                {
                    // Check the length of Miliseconds.
                    if (int.Parse(milSeconds) >= 1000)
                    {
                        milSeconds = (int.Parse(milSeconds) - 1000).ToString();
                        seconds = (int.Parse(seconds) + 1).ToString();
                    }

                    // Check the Length of Seconds
                    if (int.Parse(seconds) >= 60)
                    {
                        seconds = (int.Parse(seconds) - 60).ToString();
                        minutes = (int.Parse(minutes) + 1).ToString();
                    }

                    // Check the length of minutes
                    if (int.Parse(minutes) >= 60)
                    {
                        minutes = (int.Parse(minutes) - 60).ToString();
                        degrees = (int.Parse(degrees) + 1).ToString();
                    }
                }

                // Set the corrected value
                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";

                // Return the corrected value. 
                return correctedValue;
            }

            // No Conversion needed
            else
            {
                // Return the corrected Value
                return correctedValue;
            }

        }

        /// <summary>
        /// Do some math to convert lat/lon to decimal format
        /// </summary>
        /// <param name="value">lat OR Lon</param>
        /// <returns>Decimal Format of the value past in.</returns>
        public string createDecFormat(string value) 
        {
            // Split the value at decimal points.
            string[] splitValue = value.Split('.');

            // set our values
            string declination = splitValue[0].Substring(0, 1);
            string degrees = splitValue[0].Substring(1, 3);
            string minutes = splitValue[1];
            string seconds = splitValue[2];
            string miliSeconds = splitValue[3];
            string decFormatSeconds = $"{seconds}.{miliSeconds}";

            // Do some math with all of our variables.
            string decFormat = (double.Parse(degrees) + (double.Parse(minutes) / 60) + (double.Parse(decFormatSeconds) / 3600)).ToString();

            // Check the Declination
            if (declination == "S" || declination == "W")
            {
                // if it is S or W it needs to be a negative number.
                decFormat = $"-{decFormat}";
            }

            // Round the Decimal format to 6 places after the decimal.
            decFormat = Math.Round(double.Parse(decFormat), 6).ToString();

            // Return the Decimal Format.
            return decFormat;
        }

        /// <summary>
        /// Get the Airac Effective Dates and Set our Global Variables to it.
        /// </summary>
        public static void GetAiracDateFromFAA() 
        {
            // FAA URL that contains the effective dates for both Current and Next AIRAC Cycles.
            string url = "https://www.faa.gov/air_traffic/flight_info/aeronav/aero_data/NASR_Subscription/";

            // Create web client to connect to the URL.
            var client = new WebClient();

            // Get the HTML version of the site.
            var response = client.DownloadString(url);

            // Trim the the White Space.
            response.Trim();

            // Find the two strings that contain the effective date and set our Global Variables.
            nextAiracDate = response.Substring(response.IndexOf("NASR_Subscription_") + 18, 10);
            currentAiracDate = response.Substring( response.LastIndexOf("NASR_Subscription_") + 18, 10);
        }

        /// <summary>
        /// Create our Output directories inside the directory the user chose.
        /// </summary>
        public static void createDirectories() 
        {
            Directory.CreateDirectory($"{outputDirectory}\\ISR");
            Directory.CreateDirectory($"{outputDirectory}\\VRC");
            Directory.CreateDirectory($"{outputDirectory}\\VSTARS");
            Directory.CreateDirectory($"{outputDirectory}\\VERAM");
        }

        /// <summary>
        /// Write the Waypoints.xml File. NOTE: The Global Variable that contains all the waypoints has to be 
        /// completely filled in before we call this function!
        /// </summary>
        public static void WriteWaypointsXML() 
        {
            // File path for the waypoints.xml that we want to store to.
            string filePath = $"{outputDirectory}\\VERAM\\Waypoints.xml";
            
            // Write the XML Serializer to the file.
            TextWriter writer = new StreamWriter(filePath);
            WaypointSerializer.Serialize(writer, waypoints);
            writer.Close();
        }

        /// <summary>
        /// Add Coment to the end of the XML file for Kyle's Batch File
        /// </summary>
        /// <param name="AiracDate">Correct Format: YYYY-MM-DD</param>
        public static void AppendCommentToXML(string AiracDate) 
        {
            // File path to the Waypoints.xml File.
            string filepath = $"{outputDirectory}\\VERAM\\Waypoints.xml";

            // Add the Airac effective date in comment form to the end of the file.
            File.AppendAllText(filepath, $"\n<!--AIRAC_EFFECTIVE_DATE {AiracDate}-->");

            // Copy the file from VERAM to VSTARS. - They use the same format. 
            File.Copy($"{outputDirectory}\\VERAM\\Waypoints.xml", $"{outputDirectory}\\VSTARS\\Waypoints.xml");
        }

        /// <summary>
        /// Write a test sector file for VRC.
        /// </summary>
        public static void WriteTestSctFile() 
        {
            // Write the file INFO section.
            File.WriteAllText($"{outputDirectory}\\Test_Sct_File.sct2", $"[INFO]\nTEST_SECTOR\nTST_CTR\nXXXX\nN043.31.08.418\nW112.03.50.103\n60.043\n43.536\n- 11.8\n1.000\n\n\n\n\n");
        }
    }
}
