using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NASRData.Models;
using System.Windows.Forms;
using NASARData;
using ClassData.Models;

namespace NASRData.DataAccess
{
    /// <summary>
    /// Download, Unzip, Parse, and Make SCT2 File for FAA Fix data.
    /// </summary>
    public class GetFixData
    {
        // We need a variable to hold all the fixes while we Parse through the data.
        // This variable holds a LIST of FIX MODELS
        private List<FixModel> allFixesInData = new List<FixModel>();

        // Need to keep track of these folders so we can clean up after ourselfs. ie. Delete them when we are done.
        private string zipFolder;
        private string unzipedFolder;

        public void FixQuarterbackFunc(string effectiveDate) 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "FixQuarterbackFunc()")}: {DateTime.Now}");


            // Call the Download function, notice we are just passing in our variables from our quaterback function.
            // This is a little redundent and bad practice, but I want to show you the importance of a "quaterback" function.
            DownloadFixData(effectiveDate);

            // Call the function ParseFixData(), We have the data (finished downloading and unziping), lets parse it now.
            ParseFixData();

            // We have parsed through the fixes and created FixModels for ALL the fixes we want. 
            // Now lets call the WriteFixSctData() function.
            WriteFixSctData();

            StoreXMLData();

            // Delete the Ziped folder and the unziped folder for the fixes. 
            // Yes these are in the temp, but its good to clean up after ourselves.
            deleteUnneededDir();
        }

        /// <summary>
        /// Download and unzip the FAA Fix data from their website.
        /// </summary>
        /// <param name="effectiveDate">Valid format is "MM/DD/YYYY"</param>
        private void DownloadFixData(string effectiveDate) 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadFixData()")}: {DateTime.Now}");

            // Check to see if the TEMP Directory Exists
            // TODO - Don't delete the TEMP Directory HERE! 
            if (Directory.Exists(GlobalConfig.tempPath))
            {
                // This variable holds all information for the temp path ie. Directories and files.
                DirectoryInfo di = new DirectoryInfo(GlobalConfig.tempPath);

                // We need to delete all files and directories.
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
            else
            {
                // The file does not exist, we need to create it. 
                Directory.CreateDirectory(GlobalConfig.tempPath);
            }

            // Web Client used to connect to the FAA website.
            var client = new WebClient();

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadFixData().DOWNLOADING")}: {DateTime.Now}");

            // Download the Fix Data
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/FIX.zip", $"{GlobalConfig.tempPath}\\fixes.zip");

            // INSTANTIATE AND ASSIGN our zipFolder Variable to the file we just downloaded.
            zipFolder = $"{GlobalConfig.tempPath}\\fixes.zip";

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadFixData().UNZIPING")}: {DateTime.Now}");

            // Extract the ZIP file that we just downloaded.
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\fixes.zip", $"{GlobalConfig.tempPath}\\fixes");

            // INSTANTIATE AND ASSIGN our unzipedFolder Variable to the file we just downloaded.
            unzipedFolder = $"{GlobalConfig.tempPath}\\fixes";

        }

        /// <summary>
        /// Parse through and get the data we need for all the fixes.
        /// </summary>
        private void ParseFixData()
        {
            // Variable for all the 'bad' characters. We will remove all these characters from the data.
            char[] removeChars = { ' ', '.' };

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "ParseFixData()")}: {DateTime.Now}");


            // Read ALL the lines in FAA Fix data text file.
            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\fixes\\FIX.txt"))
            {
                // Check to see if the begining of the line starts with "FIX1"
                //if (line.IndexOf("FIX1", 0, 4) != -1)
                if (line.Substring(0, 4) == "FIX1")
                {
                    // Create the FixModel. This is needed to store the data so we can later write the SCT file.
                    // Here we remove all LEADING and TRAILING characters listed above in 'removeChars'. 
                    FixModel individualFixData = new FixModel
                    {
                        Id = line.Substring(4, 5).Trim(removeChars),
                        Lat = new GlobalConfig().CorrectLatLon(line.Substring(66, 14).Trim(removeChars),true, GlobalConfig.Convert),
                        Lon = new GlobalConfig().CorrectLatLon(line.Substring(80, 14).Trim(removeChars),false, GlobalConfig.Convert),
                        Catagory = line.Substring(94, 3).Trim(removeChars),
                        Use = line.Substring(213, 15).Trim(removeChars),
                        HiArtcc = line.Substring(233, 4).Trim(removeChars),
                        LoArtcc = line.Substring(237, 4).Trim(removeChars),
                    };

                    individualFixData.Lat_Dec = new GlobalConfig().createDecFormat(individualFixData.Lat);
                    individualFixData.Lon_Dec = new GlobalConfig().createDecFormat(individualFixData.Lon);

                    // Add this FIX MODEL to the list of all Fixes.
                    allFixesInData.Add(individualFixData);
                }
            }
        }

        private void StoreXMLData() 
        {
            List<Waypoint> waypointList = new List<Waypoint>();

            foreach (FixModel fix in allFixesInData)
            {
                Location loc = new Location { Lat = fix.Lat_Dec, Lon = fix.Lon_Dec };

                Waypoint wpt = new Waypoint
                {
                    Type = "Intersection",
                    Location = loc
                };
                
                wpt.ID = fix.Id;

                waypointList.Add(wpt);
            }

            foreach (Waypoint globalWaypoint in GlobalConfig.waypoints)
            {
                waypointList.Add(globalWaypoint);
            }

            GlobalConfig.waypoints = waypointList.ToArray();
        }

        /// <summary>
        /// Create the Fix.sct2 File for VRC. Right now this is just stored in TEMP.
        /// </summary>
        private void WriteFixSctData() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "WriteFixSctData()")}: {DateTime.Now}");


            // This is where the new SCT2 File will be saved to.
            string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\FIX.sct2";

            // String Builders are super effictient and FAST when manipulating strings in bulk.
            StringBuilder sb = new StringBuilder();

            // The very begining of the file needs to have "[FIXES]" on the first line.
            sb.AppendLine("[FIXES]");

            // Loop through ALL of the fixes we have collected and already parsed through,
            // and add it to our string builder.
            foreach (FixModel dataforEachFix in allFixesInData)
            {
                sb.AppendLine($"{dataforEachFix.Id.PadRight(6)}{dataforEachFix.Lat} {dataforEachFix.Lon} ;{dataforEachFix.HiArtcc}/{dataforEachFix.LoArtcc} {dataforEachFix.Catagory} {dataforEachFix.Use}");
            }

            // TODO - Do we want blank lines at the end of the file?
            for (int i = 0; i < 2; i++)
            {
                sb.AppendLine();
            }
            
            // Write the data to the Fix.sct2 file. 
            File.WriteAllText(filePath, sb.ToString());
            
            File.AppendAllText(filePath, $"\n\n\n\n\n\n");

            File.AppendAllText($"{GlobalConfig.outputDirectory}\\Test_Sct_File.sct2", File.ReadAllText(filePath));

        }

        /// <summary>
        /// Probably an Un-needed function, however, its good to be explicit. 
        /// We don't call this function until after our SCT file is created.
        /// </summary>
        private void deleteUnneededDir() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "deleteUnneededDir()")}: {DateTime.Now}");


            // Delete our Zip folder for Fixes we downloaded from FAA
            File.Delete(zipFolder);

            // Delete our unziped folder and fix document.
            Directory.Delete(unzipedFolder, true);
        }
    }
}
