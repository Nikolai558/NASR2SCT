using ClassData.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.CodeDom.Compiler;

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
        public static readonly string ProgramVersion = "0.6.8";

        public static readonly string testSectorFileName = $"\\VRC\\TestSectorFile.sct2";

        public static string GithubVersion = "";

        public static List<AssetsModel> AllAssetsToDownload = new List<AssetsModel>();

        public static string ReleaseBody = "";

        public static StringBuilder AwyGeoMap = new StringBuilder();
        public static string AwyGeoMapFileName = "AWY_GEOMAP.xml";
        
        // XML Serializer for our Waypoints.xml file.
        private static XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("Waypoints");
        public static XmlSerializer WaypointSerializer = new XmlSerializer(typeof(Waypoint[]), xmlRootAttribute);

        // Global Waypoints storage so we can collect all airports, fixes, vors, and ndbs for the Waypoints.xml file.
        public static Waypoint[] waypoints = new Waypoint[] { };

        // Store the Current and Next AIRAC effective Date.
        public static string currentAiracDate;
        public static string nextAiracDate;

        // Store the users Output directory choice. 
        public static string outputDirectory = null;
        public static string outputDirBase;

        // Store the users Choice if they want to convert East Cordinates.
        public static bool Convert = false;

        // Temp path for the user. ie: C:\Users\nik\AppData\Local\Temp\NASR_TO_SCT
        public static readonly string tempPath = $"{Path.GetTempPath()}NASR2SCT";

        public static void CreateAwyGeomapHeadersAndEnding(bool CreateStart)
        {
            if (CreateStart)
            {
                AwyGeoMap.AppendLine("        <GeoMapObject Description=\"AIRWAYS\" TdmOnly=\"false\">");
                AwyGeoMap.AppendLine("          <LineDefaults Bcg=\"4\" Filters=\"4\" Style=\"ShortDashed\" Thickness=\"1\" />");
                AwyGeoMap.AppendLine("          <Elements>");
            }
            else
            {
                AwyGeoMap.AppendLine("          </Elements>");
                AwyGeoMap.AppendLine("        </GeoMapObject>");
                File.WriteAllText($"{outputDirectory}\\VERAM\\{AwyGeoMapFileName}", AwyGeoMap.ToString());
            }
        }

        public static void DownloadAssets() 
        {
            foreach (AssetsModel asset in GlobalConfig.AllAssetsToDownload)
            {
                var client = new WebClient();
                client.DownloadFile(asset.DownloadURL, $"{tempPath}\\{asset.Name}");
            }
        }

        public static void CheckTempDir() 
        {
            // Check to see if the TEMP Directory Exists
            if (Directory.Exists(tempPath))
            {
                // This variable holds all information for the temp path ie. Directories and files.
                DirectoryInfo di = new DirectoryInfo(tempPath);

                // Loop through the Files in our TempPath
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    // Delete each file it finds inside of this directory. IE Temp Path
                    file.Delete();
                }

                // Loop through the Directories in our TempPath
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    // Delete the folder it finds.
                    dir.Delete(true);
                }
            }
            else
            {
                // The file does not exist, we need to create it. 
                Directory.CreateDirectory(tempPath);
            }
        }

        public static void UpdateCheck()
        {
            // Set our Repository Link Details
            string owner = "Nikolai558";
            string repo = "NASR2SCT";

            // Set our full API Repository Link.
            string latestReleaseURL = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";

            // Get the JSON Response back from the API Call
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(latestReleaseURL);
            request.Accept = "application/vnd.github.v3+json";
            request.UserAgent = "request";
            request.AllowAutoRedirect = true;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Create our Variables for what we get from the JSON API Call to Github
            List<JObject> listOfAssests = new List<JObject>();

            // Read the Response we get from API Call.
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                // Get the Content from the response.
                string content = reader.ReadToEnd();

                // Parse the JSON response we get back.
                var jsonobj = JObject.Parse(content);

                // Assign our Temp Variables to our Latest release values. 
                GithubVersion = jsonobj["tag_name"].ToString();
                ReleaseBody = jsonobj["body"].ToString();

                // Get all the Assets in this release
                foreach (JObject asset in jsonobj["assets"])
                {
                    listOfAssests.Add(asset);
                }
            }

            // Create our Assets Models.
            foreach (JObject asset in listOfAssests)
            {
                AssetsModel downloadAsset = new AssetsModel() { Name = asset["name"].ToString(), DownloadURL = asset["browser_download_url"].ToString() };
                AllAssetsToDownload.Add(downloadAsset);
            }
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


                // Set the Corrected Value
                correctedValue = $"{declination}{degrees.PadLeft(3, '0')}.{minutes.PadRight(2, '0')}.{seconds.PadRight(2, '0')}.{milSeconds.PadRight(3, '0')}";
            }

            // Check to see if Convert E is True and Check our Value's Declination to make sure it is an E Coord.
            if (ConvertEast && declination == "E")
            {
                double oldDecForm = double.Parse(createDecFormat(correctedValue, false));

                double newDecForm = 180 - oldDecForm;

                newDecForm = (newDecForm + 180) * -1;

                correctedValue = createDMS(newDecForm, false);

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

        public string createDMS(double value, bool lat) 
        {
            
            int degrees = 0;
            decimal degreeFloat = 0;

            int minutes = 0;
            decimal minuteFloat = 0;

            int seconds = 0;
            decimal secondFloat = 0;

            string miliseconds = "0";

            string dms; 

            degrees = int.Parse(value.ToString().Split('.')[0]);

            if (value.ToString().Split('.').Count() > 1)
            {
                degreeFloat = decimal.Parse("0." + value.ToString().Split('.')[1]);
            }

            minutes = int.Parse((degreeFloat * 60).ToString().Split('.')[0]);

            if ((degreeFloat * 60).ToString().Split('.').Count() > 1)
            {
                minuteFloat = decimal.Parse("0." + (degreeFloat * 60).ToString().Split('.')[1]);
            }

            seconds = int.Parse((minuteFloat * 60).ToString().Split('.')[0]);

            if ((minuteFloat * 60).ToString().Split('.').Count() > 1)
            {
                secondFloat = decimal.Parse("0." + (minuteFloat * 60).ToString().Split('.')[1]);
            }

            secondFloat = Math.Round(secondFloat, 3);

            if (secondFloat.ToString().Split('.').Count() > 1)
            {
                miliseconds = secondFloat.ToString().Split('.')[1];
            }


            if (lat)
            {
                if (degrees < 0)
                {
                    degrees = degrees * -1;
                    dms = $"S{degrees.ToString().PadLeft(3, '0')}.{minutes.ToString().PadRight(2, '0')}.{seconds.ToString().PadRight(2, '0')}.{miliseconds.ToString().PadRight(3, '0')}";
                }
                else
                {
                    dms = $"N{degrees.ToString().PadLeft(3, '0')}.{minutes.ToString().PadRight(2, '0')}.{seconds.ToString().PadRight(2, '0')}.{miliseconds.ToString().PadRight(3, '0')}";
                }
            }
            else
            {
                if (degrees < 0)
                {
                    degrees = degrees * -1;
                    dms = $"W{degrees.ToString().PadLeft(3, '0')}.{minutes.ToString().PadRight(2, '0')}.{seconds.ToString().PadRight(2, '0')}.{miliseconds.ToString().PadRight(3, '0')}";
                }
                else
                {
                    dms = $"E{degrees.ToString().PadLeft(3, '0')}.{minutes.ToString().PadRight(2, '0')}.{seconds.ToString().PadRight(2, '0')}.{miliseconds.ToString().PadRight(3, '0')}";
                }
            }

            return dms;
        }

        /// <summary>
        /// Do some math to convert lat/lon to decimal format
        /// </summary>
        /// <param name="value">lat OR Lon</param>
        /// <returns>Decimal Format of the value past in.</returns>
        public string createDecFormat(string value, bool roundSixPlaces) 
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

            if (roundSixPlaces)
            {
                // Round the Decimal format to 6 places after the decimal.
                decFormat = Math.Round(double.Parse(decFormat), 6).ToString();
            }

            // Return the Decimal Format.
            return decFormat;
        }

        private static void CreateBatchFile() 
        {
            string filePath = $"{tempPath}\\getAiraccEff.bat";
            string writeMe = $"cd \"{tempPath}\"\n" +
                "curl \"https://www.faa.gov/air_traffic/flight_info/aeronav/aero_data/NASR_Subscription/\">FAA_NASR.HTML";
            File.WriteAllText(filePath, writeMe);
        }

        private static void ExecuteCommand()
        {
            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + $"\"{tempPath}\\getAiraccEff.bat\"");
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;

            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();

            ExitCode = Process.ExitCode;
            Process.Close();

            //MessageBox.Show("ExitCode: " + ExitCode.ToString(), "ExecuteCommand");
        }

        /// <summary>
        /// Get the Airac Effective Dates and Set our Global Variables to it.
        /// </summary>
        public static void GetAiracDateFromFAA() 
        {
            // FAA URL that contains the effective dates for both Current and Next AIRAC Cycles.
            string url = "https://www.faa.gov/air_traffic/flight_info/aeronav/aero_data/NASR_Subscription/";

            string response;

            CreateBatchFile();
            
            ExecuteCommand();

            if (File.Exists($"{tempPath}\\FAA_NASR.HTML")  && File.ReadAllText($"{tempPath}\\FAA_NASR.HTML").Length > 10)
            {
                response = File.ReadAllText($"{tempPath}\\FAA_NASR.HTML");
            }
            else
            {
                // If we get here the user does not have Curl, OR Curl returned a file that is not longer than 10 Characters.
                using (var client = new System.Net.WebClient())
                {
                    client.Proxy = null;

                    //client.Proxy = GlobalProxySelection.GetEmptyWebProxy();
                    response = client.DownloadString(url);
                }
            }

            response.Trim();

            // Find the two strings that contain the effective date and set our Global Variables.
            nextAiracDate = response.Substring(response.IndexOf("NASR_Subscription_") + 18, 10);
            currentAiracDate = response.Substring(response.LastIndexOf("NASR_Subscription_") + 18, 10);
        }

        /// <summary>
        /// Create our Output directories inside the directory the user chose.
        /// </summary>
        public static void createDirectories() 
        {
            


            Directory.CreateDirectory(outputDirectory);

            Directory.CreateDirectory($"{outputDirectory}\\ALIAS");
            Directory.CreateDirectory($"{outputDirectory}\\VRC");
            Directory.CreateDirectory($"{outputDirectory}\\VSTARS");
            Directory.CreateDirectory($"{outputDirectory}\\VERAM");

            Directory.CreateDirectory($"{GlobalConfig.outputDirectory}\\VRC\\[SID]");
            Directory.CreateDirectory($"{GlobalConfig.outputDirectory}\\VRC\\[STAR]");


        }

        /// <summary>
        /// Create only the Temp Directory that we need
        /// </summary>
        /// <param name="onlyTempFile">OnlyTempDir Bool</param>
        public static void createDirectories(bool onlyTempFile) 
        {
            Directory.CreateDirectory($"{tempPath}");
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

        public static void WriteNavXmlOutput() 
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("        <GeoMapObject Description=\"NAVAIDS\" TdmOnly=\"false\">");
            sb.AppendLine("          <SymbolDefaults Bcg=\"13\" Filters=\"13\" Style=\"VOR\" Size=\"1\" />");
            sb.AppendLine("          <Elements>");


            string readFilePath = $"{outputDirectory}\\VERAM\\Waypoints.xml";
            string saveFilePath = $"{ outputDirectory}\\VERAM\\NAVAID_GEOMAP.xml";

            bool grabLocation = false;

            foreach (string line in File.ReadAllLines(readFilePath))
            {
                if (line.Length > 13)
                {

                    if (line.Substring(3, 8) == "Waypoint")
                    {
                        if (line.Substring(18, 7) != "Airport" && line.Substring(18, 12) != "Intersection")
                        {
                            grabLocation = true;
                        }
                        else
                        {
                            grabLocation = false;
                        }
                    }
                    else if (line.Substring(5, 8) == "Location" && grabLocation == true)
                    {
                        int locLength = line.Length - 17;
                        List<string> latLonSplitValue = line.Substring(14, locLength).Split(' ').ToList();

                        string printString = $"            <Element xsi:type=\"Symbol\" Filters=\"\" {latLonSplitValue[1]} {latLonSplitValue[0]} />";

                        sb.AppendLine(printString);
                    }
                }
            }

            sb.AppendLine("          </Elements>");
            sb.AppendLine("        </GeoMapObject>");

            File.WriteAllText(saveFilePath, sb.ToString());
        }

        public static void WriteAptXmlOutput()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("        <GeoMapObject Description=\"APT\" TdmOnly=\"false\">");
            sb.AppendLine("          <SymbolDefaults Bcg=\"10\" Filters=\"10\" Style=\"Airport\" Size=\"1\" />");
            sb.AppendLine("          <Elements>");


            string readFilePath = $"{outputDirectory}\\VERAM\\Airports.xml";
            string saveFilePath = $"{ outputDirectory}\\VERAM\\AIRPORTS_GEOMAP.xml";

            foreach (string line in File.ReadAllLines(readFilePath))
            {
                if (line.Length > 13)
                {
                    if (line.Substring(5, 8) == "Location")
                    {
                        int locLength = line.Length - 17;
                        List<string> latLonSplitValue = line.Substring(14, locLength).Split(' ').ToList();

                        string printString = $"            <Element xsi:type=\"Symbol\" Filters=\"\" Size=\"2\" {latLonSplitValue[1]} {latLonSplitValue[0]} />";

                        sb.AppendLine(printString);
                    }
                }
            }

            sb.AppendLine("          </Elements>");
            sb.AppendLine("        </GeoMapObject>");

            File.WriteAllText(saveFilePath, sb.ToString());
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
            File.WriteAllText($"{outputDirectory}\\{GlobalConfig.testSectorFileName}", $"[INFO]\nTEST_SECTOR\nTST_CTR\nXXXX\nN043.31.08.418\nW112.03.50.103\n60.043\n43.536\n-11.8\n1.000\n\n\n\n\n");
        }
    }
}
