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
        public static readonly string ProgramVersion = "V-0.4.3-beta";
        public static string GithubVersion = "";

        private static XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("Waypoints");

        public static XmlSerializer WaypointSerializer = new XmlSerializer(typeof(Waypoint[]), xmlRootAttribute);

        public static Waypoint[] waypoints = new Waypoint[] { };


        public static string currentAiracDate;
        public static string nextAiracDate;

        public static string outputDirectory;


        public static StringBuilder timetracker = new StringBuilder();

        public static bool Convert = false;

        // Temp path for the user. ie: C:\Users\nik\AppData\Local\Temp\NASR_TO_SCT
        public static readonly string tempPath = $"{Path.GetTempPath()}NASR_TO_SCT";

        public static readonly string msg1 = $"THIS BATCH FILE WILL:\n1) Pull data from the FAA NASR site and create Sector Files(.SCT) for virtual Radar Clients\nsuch as VRC by MetaCraft.\n\n2) When appropriate, it will also create an In-Scope Reference(ISR) alias(.TXT) file for\nthe data parsed.";

        public static readonly string msg2 = @"NOTES:
   - DME Only stations are placed into the VOR list.

   - All Airways (LOW/HIGH) are placed into the same SCT File. Seeing how the intent would never
     be to see all airways at the same time on the scope, but rather drawn as needed, it is
     acceptable to put them all under either the HIGH or LOW airways header.

   - This parser can take 10 min or longer to complete. Factors such as individual computer
     performance come into play with the duration of this process.

   - Added Wx Stations as a 'Label for VRC STATIC TEXT menu. Note- due to limitations of .BAT files,
     Weather Stations will appear anywhere between 0.1 to 2.0 miles offset.

   - Requires Powershell v3 or higher.v5.1 or later is recommended.
     You may update Powershell by downloading the Windows Management Framework:

     - https://www.microsoft.com/en-us/download/details.aspx?id=54616";

        public static readonly string msg3 = @"-------------------------------------------------------------

  VRC and other similar virtual RADAR Clients have a tendency
  to put East Delincation points on the far right side of the
  scope.

  Do you wish to convert these East declinations to 'West' 
  and the associated coordinates in order to 'Trick' your
  RADAR client into putting them on the left side of your
  scope?


 Example:

 ORIGINAL:   AAMYY N051.30.18.800 E171.09.11.700

 CONVERTED:   AAMYY N051.30.18.800 W188.50.48.300

-------------------------------------------------------------";

        public static void githubVersion()
        {
            var client = new WebClient();
            GithubVersion = client.DownloadString("https://raw.githubusercontent.com/Nikolai558/NASR2SCT/main/tempLatestVersion.txt");

            GithubVersion = GithubVersion.Split()[0];
        }

        public void WriteTimeStamps() 
        {
            string timefilepath = $"{tempPath}\\TimeStamps.txt";

            File.WriteAllText(timefilepath, timetracker.ToString());
        }

        public string CorrectLatLon(string value, bool Lat, bool ConvertEast) 
        {
            // Valid format is N000.00.00.000 W000.00.000
            string correctedValue = "";

            char[] splitValue = new char[] { '.', '-'};
            string degrees;
            string minutes;
            string seconds;
            string milSeconds;
            string declination = "";

            if (Lat)
            {
                if (value.IndexOf("N", 0, value.Length) != -1)
                {
                    declination = "N";
                    value = value.Replace("N", "");
                }
                else if (value.IndexOf("S", 0, value.Length) != -1)
                {
                    declination = "S";
                    value = value.Replace("S", "");
                }

                string[] valueSplit = value.Split(splitValue);
                degrees = valueSplit[0];
                minutes = valueSplit[1];
                seconds = valueSplit[2];
                if (valueSplit[3].Length > 3)
                {
                    milSeconds = valueSplit[3].Substring(0, 3);
                }
                else
                {
                    milSeconds = valueSplit[3];
                }

                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";
            }
            else
            {
                if (value.IndexOf("E", 0, value.Length) != -1)
                {
                    declination = "E";
                    value = value.Replace("E", "");
                }
                else if (value.IndexOf("W", 0, value.Length) != -1)
                {
                    declination = "W";
                    value = value.Replace("W", "");
                }
                else
                {
                    Console.WriteLine($"{value} is not valid format....");
                    return value;
                }

                string[] valueSplit = value.Split(splitValue);
                degrees = valueSplit[0];
                minutes = valueSplit[1];
                seconds = valueSplit[2];
                milSeconds = valueSplit[3];

                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";
            }

            if (ConvertEast && declination == "E")
            {
                if (milSeconds.Length >= 4)
                {
                    milSeconds = milSeconds.Substring(0, 3);
                }

                degrees = ((179 - int.Parse(degrees)) + 179).ToString();
                minutes = ((59 - int.Parse(minutes)) + 59).ToString();
                seconds = ((59 - int.Parse(minutes)) + 59).ToString();
                milSeconds = ((1000 - int.Parse(milSeconds)) + 1000).ToString();
                declination = "W";

                for (int i = 0; i < 3; i++)
                {
                    if (int.Parse(milSeconds) >= 1000)
                    {
                        milSeconds = (int.Parse(milSeconds) - 1000).ToString();
                        seconds = (int.Parse(seconds) + 1).ToString();
                    }

                    if (int.Parse(seconds) >= 60)
                    {
                        seconds = (int.Parse(seconds) - 60).ToString();
                        minutes = (int.Parse(minutes) + 1).ToString();
                    }

                    if (int.Parse(minutes) >= 60)
                    {
                        minutes = (int.Parse(minutes) - 60).ToString();
                        degrees = (int.Parse(degrees) + 1).ToString();
                    }
                }

                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";

                return correctedValue;
            }
            else
            {
                return correctedValue;
            }

        }

        public string createDecFormat(string value) 
        {
            string[] splitValue = value.Split('.');

            string declination = splitValue[0].Substring(0, 1);
            string degrees = splitValue[0].Substring(1, 3);
            string minutes = splitValue[1];
            string seconds = splitValue[2];
            string miliSeconds = splitValue[3];
            string decFormatSeconds = $"{seconds}.{miliSeconds}";


            string decFormat = (double.Parse(degrees) + (double.Parse(minutes) / 60) + (double.Parse(decFormatSeconds) / 3600)).ToString();

            if (declination == "S" || declination == "W")
            {
                decFormat = $"-{decFormat}";
            }

            decFormat = Math.Round(double.Parse(decFormat), 6).ToString();

            return decFormat;

            // dec = degrees + (minutes / 60) + (seconds.Miliseconds / 3600) 
        }

        public static void GetAiracDateFromFAA() 
        {
            string url = "https://www.faa.gov/air_traffic/flight_info/aeronav/aero_data/NASR_Subscription/";

            var client = new WebClient();

            var response = client.DownloadString(url);

            response.Trim();

            //NASR_Subscription_

            nextAiracDate = response.Substring(response.IndexOf("NASR_Subscription_") + 18, 10);

            currentAiracDate = response.Substring( response.LastIndexOf("NASR_Subscription_") + 18, 10);
        }

        public static void createDirectories() 
        {
            Directory.CreateDirectory($"{outputDirectory}\\ISR");
            Directory.CreateDirectory($"{outputDirectory}\\VRC");
            Directory.CreateDirectory($"{outputDirectory}\\VSTARS");
            Directory.CreateDirectory($"{outputDirectory}\\VERAM");
        }

        public static void WriteWaypointsXML() 
        {
            string filePath = $"{outputDirectory}\\VERAM\\Waypoints.xml";
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
            string filepath = $"{outputDirectory}\\VERAM\\Waypoints.xml";

            File.AppendAllText(filepath, $"\n<!--AIRAC_EFFECTIVE_DATE {AiracDate}-->");
            File.Copy($"{outputDirectory}\\VERAM\\Waypoints.xml", $"{outputDirectory}\\VSTARS\\Waypoints.xml");
        }

        public static void WriteTestSctFile() 
        {
            File.WriteAllText($"{GlobalConfig.outputDirectory}\\Test_Sct_File.sct2", $"[INFO]\nTEST_SECTOR\nTST_CTR\nXXXX\nN043.31.08.418\nW112.03.50.103\n60.043\n43.536\n- 11.8\n1.000\n\n\n\n\n");
        }
    }
}
