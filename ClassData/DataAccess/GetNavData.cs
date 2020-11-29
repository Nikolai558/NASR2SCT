using ClassData.Models;
using NASARData;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.DataAccess
{
    /// <summary>
    /// Download, Unzip, Parse, and Make SCT2 File for FAA NAV data.
    /// </summary>
    public class GetNavData
    {
        // Create a LIST of the two types of Models that we need.
        private List<NDBModel> allNDBData = new List<NDBModel>();
        private List<VORModel> allVORData = new List<VORModel>();

        // Variables to keep track of the files/directories that we need to delete.
        private string zipFolder;
        private string unzipedFolder;

        /// <summary>
        /// Call all the related functions to get the NAV data from the FAA.
        /// </summary>
        /// <param name="effectiveDate">Airac Effective Date, Format: "MM/DD/YYYY"</param>
        public void NAVQuarterbackFunc(string effectiveDate, string Artcc) 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "NAVQuarterbackFunc()")}: {DateTime.Now}");

            DownloadNAVData(effectiveDate);
            ParseNAVData();
            StoreXMLData();
            WriteNAVSctData();
            WriteNavISR(Artcc);
            deleteUnneededDir();
        }

        /// <summary>
        /// Download and Unzip the NAV data from FAA
        /// </summary>
        /// <param name="effectiveDate">Airac Effective Date, Format: "MM/DD/YYYY"</param>
        private void DownloadNAVData(string effectiveDate) 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadNAVData()")}: {DateTime.Now}");

            // Create a web client to download the data.
            var client = new WebClient();

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadNAVData().DOWNLOADING")}: {DateTime.Now}");

            // Go to the FAA website and download the NAV zip folder.
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/NAV.zip", $"{GlobalConfig.tempPath}\\nav.zip");
            zipFolder = $"{GlobalConfig.tempPath}\\nav.zip";

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadNAVData().UNZIPING")}: {DateTime.Now}");

            // Extract the zip folder we downloaded.
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\nav.zip", $"{GlobalConfig.tempPath}\\nav");
            unzipedFolder = $"{GlobalConfig.tempPath}\\nav";
        }

        /// <summary>
        /// Parse through the NAV data.
        /// This data contains two data types, NDB and VOR
        /// </summary>
        private void ParseNAVData() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "ParseNAVData()")}: {DateTime.Now}");

            // FAA Provides data for ALL types of NDB and VOR, we only need certain types. Exclude the one we are on if it has any of these.
            List<string> excludeTypes = new List<string>{ "VOT", "FAN MARKER", "CONSOLAN", "MARINE NDB", "DECOMMISSIONED", "MARINE NDB/DME"};
            
            // We are looking for these types, need to catagorize NDB or VOR depending on what the current NAVE type is.
            List<string> NDBTypes = new List<string>{ "NDB", "NDB/DME", "UHF/NDB" };
            List<string> VORTypes = new List<string> { "DME", "VOR", "TACAN", "VOR/DME", "VORTAC"};

            // Variable to Remove all LEADING AND TRAILING characters.
            char[] removeChars = { ' ', '.' };

            // Read the NAV.txt file
            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\nav\\NAV.txt"))
            {
                // Bool to tell our program if it is to exclude this current line. To start we set it to false.
                bool exclude = false;

                // Check to see if the first part of the line is "NAV1"
                if (line.IndexOf("NAV1", 0, 4) != -1)
                {
                    // If the current line we are working in has a string that matches excludeTypes, then set bool exclude to TRUE
                    foreach (string x in excludeTypes)
                    {
                        if (line.IndexOf(x) != -1)
                        {
                            exclude = true;
                        }
                    }

                    // Check to see if we need to exclude it.
                    if (exclude != true)
                    {
                        // Checks to see if the TYPE is an NDB
                        if (NDBTypes.Contains(line.Substring(8, 20).Trim(removeChars)))
                        {
                            // Create our NDB Model
                            NDBModel individualNDB = new NDBModel
                            {
                                Id = line.Substring(28, 4).Trim(removeChars),
                                Type = line.Substring(8, 20).Trim(removeChars),
                                Name = line.Substring(42, 30).Trim(removeChars),
                                Freq = line.Substring(533, 6).Trim(removeChars),
                                Lat = new GlobalConfig().CorrectLatLon(line.Substring(371, 14).Trim(removeChars),true, GlobalConfig.Convert),
                                Lon = new GlobalConfig().CorrectLatLon(line.Substring(396, 14).Trim(removeChars),false, GlobalConfig.Convert)
                            };

                            individualNDB.Dec_Lat = new GlobalConfig().createDecFormat(individualNDB.Lat);
                            individualNDB.Dec_Lon = new GlobalConfig().createDecFormat(individualNDB.Lon);


                            // Add the NDB model we just created to our LIST of NDB Models
                            allNDBData.Add(individualNDB);
                        }

                        // Check to see if TYPE is VOR
                        else if (VORTypes.Contains(line.Substring(8, 20).Trim(removeChars)))
                        {
                            // Create our VOR Model
                            VORModel individualVOR = new VORModel
                            {
                                Id = line.Substring(28, 4).Trim(removeChars),
                                Type = line.Substring(8, 20).Trim(removeChars),
                                Name = line.Substring(42, 30).Trim(removeChars),
                                Freq = line.Substring(533, 6).Trim(removeChars),
                                Lat = new GlobalConfig().CorrectLatLon(line.Substring(371, 14).Trim(removeChars),true, GlobalConfig.Convert),
                                Lon = new GlobalConfig().CorrectLatLon(line.Substring(396, 14).Trim(removeChars),false, GlobalConfig.Convert)
                            };

                            individualVOR.Dec_Lat = new GlobalConfig().createDecFormat(individualVOR.Lat);
                            individualVOR.Dec_Lon = new GlobalConfig().createDecFormat(individualVOR.Lon);


                            // Add the VOR model we just created to our LIST of VOR Models.
                            allVORData.Add(individualVOR);
                        }
                    }
                }
            }
        }
        private void StoreXMLData()
        {
            List<Waypoint> waypointList = new List<Waypoint>();

            foreach (NDBModel ndb in allNDBData)
            {
                Location loc = new Location { Lat = ndb.Dec_Lat, Lon = ndb.Dec_Lon };

                Waypoint wpt = new Waypoint
                {
                    Type = "NDB",
                    Location = loc
                };

                wpt.ID = ndb.Id;

                waypointList.Add(wpt);
            }

            foreach (VORModel vor in allVORData)
            {
                Location loc = new Location { Lat = vor.Dec_Lat, Lon = vor.Dec_Lon };

                Waypoint wpt = new Waypoint
                {
                    Type = "VOR",
                    Location = loc
                };

                wpt.ID = vor.Id;

                waypointList.Add(wpt);
            }

            foreach (Waypoint globalWaypoint in GlobalConfig.waypoints)
            {
                waypointList.Add(globalWaypoint);
            }

            GlobalConfig.waypoints = waypointList.ToArray();
        }

        /// <summary>
        /// Write the Nav In scope referecne commands
        /// </summary>
        /// <param name="Artcc">User Artcc Code</param>
        private void WriteNavISR(string Artcc) 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "WriteNavISR()")}: {DateTime.Now}");

            // File path to save the ISR file
            string filePath = $"{GlobalConfig.outputDirectory}\\ISR\\ISR_NAVAID.txt";
            
            // String builder to store all the Lines for the file
            StringBuilder sb = new StringBuilder();

            // Loop through all of the NDB's
            foreach (NDBModel ndb in allNDBData)
            {
                // Grab the name of the NDB
                string shortName = ndb.Name;

                // Bad Characters in the name, we want to remove these
                string[] removeChar = { " ", "=", "-", "." };

                // loop through the remove characters to replace them in our ShortName Variable
                foreach (var bad in removeChar)
                {
                    // Set shortName = shortName with out the bad character.
                    shortName = shortName.Replace(bad, string.Empty);
                }

                // Add both the .NAV{ID} and .NAV{Name} to the String builder.
                sb.AppendLine($".NAV{ndb.Id} .MSG {Artcc}_ISR *** {ndb.Id} {ndb.Freq} {ndb.Name} {ndb.Type}");
                sb.AppendLine($".NAV{shortName} .MSG {Artcc}_ISR *** {ndb.Id} {ndb.Freq} {ndb.Name} {ndb.Type}");
            }

            // Loop through all of the VOR's we have
            foreach (VORModel vor in allVORData)
            {
                // Grab the VOR Name
                string shortName = vor.Name;

                // Characters to be removed from the name
                string[] removeChar = { " ", "=", "-", "." };

                // Loop through the bad characters
                foreach (var bad in removeChar)
                {
                    // Remove the character and set the new string back to shortName
                    shortName = shortName.Replace(bad, string.Empty);
                }

                // Add both the .NAV{ID} and .NAV{Name} to the String builder.
                sb.AppendLine($".NAV{vor.Id} .MSG {Artcc}_ISR *** {vor.Id} {vor.Freq} {vor.Name} {vor.Type}");
                sb.AppendLine($".NAV{shortName} .MSG {Artcc}_ISR *** {vor.Id} {vor.Freq} {vor.Name} {vor.Type}");
            }

            // Write the string builder to the file. Both the NDB and VOR commands are inside ONE string builder.
            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Create the SCT2 File for NDB and VOR
        /// </summary>
        private void WriteNAVSctData() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "WriteNAVSctData()")}: {DateTime.Now}");

            // Variable for the full file path for our two types.
            string NDBfilePath = $"{GlobalConfig.outputDirectory}\\VRC\\NDB.sct2";
            string VORfilePath = $"{GlobalConfig.outputDirectory}\\VRC\\VOR.sct2";

            // Create NDB String builder with all the required data.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[NDB]");
            foreach (NDBModel ndb in allNDBData)
            {
                sb.AppendLine($"{ndb.Id.PadRight(4)}{ndb.Freq.PadRight(8)}{ndb.Lat} {ndb.Lon} ;{ndb.Name} {ndb.Type}");
            }
            File.WriteAllText(NDBfilePath, sb.ToString());

            // Overide the previous NDB String Builder with a new one and create VOR String builder with all the required data.
            sb = new StringBuilder();
            sb.AppendLine("[VOR]");
            foreach (VORModel vor in allVORData)
            {
                sb.AppendLine($"{vor.Id.PadRight(4)}{vor.Freq.PadRight(8)}{vor.Lat} {vor.Lon} ;{vor.Name} {vor.Type}");
            }
            File.WriteAllText(VORfilePath, sb.ToString());
        }

        /// <summary>
        /// Again Probably useless function but never hurts to be explicit. This deletes the data we downloaded from the FAA.
        /// </summary>
        private void deleteUnneededDir() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "deleteUnneededDir()")}: {DateTime.Now}");

            // Delete the Zip "File"
            File.Delete(zipFolder);

            // Delete the unziped folder and data.
            Directory.Delete(unzipedFolder, true);
        }
    }
}
