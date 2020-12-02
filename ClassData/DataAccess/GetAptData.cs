﻿using ClassData.Models;
using NASARData;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace ClassData.DataAccess
{
    /// <summary>
    /// Download, Unzip, Parse, Make SCT2, Make Airports.xml, and Make Lables sector Files from FAA APT data.
    /// </summary>
    public class GetAptData
    {
        // List of ALL Airport Models
        private List<AptModel> allAptModels = new List<AptModel>();

        // File paths for files we download 
        private string zipFolder;
        private string unzipedFolder;

        // List of all Weather Station Models
        private List<WxStationModel> allWxStationsInData = new List<WxStationModel>();

        // File paths for Weather Station Files We downloaded.
        private string WxzipFolder;
        private string WxunzipedFolder;

        /// <summary>
        /// Call all the needed functions.
        /// </summary>
        /// <param name="effectiveDate">Airac Effective Date, Format: "YYYY-MM-DD"</param>
        public void APTQuarterbackFunc(string effectiveDate, string artcc, string color) 
        {
            DownloadAptData(effectiveDate);
            ParseAptData();
            WriteAptISR(artcc);
            WriteAptSctData();
            deleteUnneededDir();

            WriteEramAirportsXML(effectiveDate);
            StoreWaypointsXMLData();

            DownloadWxStationData(effectiveDate);
            ParseWxStationData(color);
            WriteWxStationSctData();
            deleteUnneededWXDir();
        }

        /// <summary>
        /// Download and unzip the FAA Data
        /// </summary>
        /// <param name="effectiveDate">Airac Effective Date, Format: "YYYY-MM-DD"</param>
        private void DownloadAptData(string effectiveDate)
        {
            // Create Web Client to connect to FAA
            var client = new WebClient();

            // Download the APT.ZIP file from FAA
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/APT.zip", $"{GlobalConfig.tempPath}\\apt.zip");

            // Store our path for the zip folder we just downloaded so we can delete later
            zipFolder = $"{GlobalConfig.tempPath}\\apt.zip";

            // Unzip FAA apt.zip
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\apt.zip", $"{GlobalConfig.tempPath}\\apt");

            // Store our path for the unziped folder so we can delete later.
            unzipedFolder = $"{GlobalConfig.tempPath}\\apt";
        }

        /// <summary>
        /// Download and Unzip the Wx Station Data from the FAA
        /// </summary>
        /// <param name="effectiveDate">Airacc Effective Date: "YYYY-MM-DD"</param>
        private void DownloadWxStationData(string effectiveDate)
        {
            // Create Connection to FAA Site
            var client = new WebClient();

            // Download the Weather Stations Data
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/WXL.zip", $"{GlobalConfig.tempPath}\\wxStations.zip");

            // INSTANTIATE AND ASSIGN our zipFolder Variable to the file we just downloaded.
            WxzipFolder = $"{GlobalConfig.tempPath}\\wxStations.zip";

            // Extract the ZIP file that we just downloaded.
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\wxStations.zip", $"{GlobalConfig.tempPath}\\wxStations");

            // INSTANTIATE AND ASSIGN our unzipedFolder Variable to the file we just downloaded.
            WxunzipedFolder = $"{GlobalConfig.tempPath}\\wxStations";
        }

        /// <summary>
        /// Parse through the apt.txt file from the FAA
        /// </summary>
        private void ParseAptData()
        {
            // Characters to be removed from each line TRAILING AND LEADING
            char[] removeChars = { ' ', '.' };

            // Set Airport Model to be null. First time this runs it will overide it.
            AptModel airport = null ;

            // Read the apt.txt file
            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\apt\\APT.txt"))
            {
                // If the line starts with "APT" create our Airport Model.
                if (line.Substring(0, 3) == "APT")
                {
                    // If we have a full Airport Model and the status of the airport is Operational, add it to our list.
                    if (airport != null && airport.Status == "O")
                    {
                        // Add the Airport to our List of Airports
                        allAptModels.Add(airport);
                    }

                    // Create a new Airport Model.
                    airport = new AptModel();

                    // Create an EMPTY list of runwayModels.
                    airport.Runways = new List<RunwayModel>();

                    // Create our Airport Model
                    airport.Type = line.Substring(14, 13).Trim(removeChars);
                    airport.Id = line.Substring(27, 4).Trim(removeChars);
                    airport.Name = line.Substring(133, 50).Trim(removeChars);
                    airport.Lat = new GlobalConfig().CorrectLatLon(line.Substring(523, 15).Trim(removeChars), true, GlobalConfig.Convert);
                    airport.Elv = Math.Round(double.Parse(line.Substring(578, 7).Trim(removeChars)),0).ToString();
                    airport.ResArtcc = line.Substring(674, 4).Trim(removeChars);
                    airport.Status = line.Substring(840, 2).Trim(removeChars);
                    airport.Twr = line.Substring(980, 1).Trim(removeChars);
                    airport.Ctaf = line.Substring(988, 7).Trim(removeChars);
                    airport.Icao = line.Substring(1210, 7).Trim(removeChars);
                    airport.Lon = new GlobalConfig().CorrectLatLon(line.Substring(550, 15).Trim(removeChars), false, GlobalConfig.Convert);
                    airport.Lat_Dec = new GlobalConfig().createDecFormat(airport.Lat);
                    airport.Lon_Dec = new GlobalConfig().createDecFormat(airport.Lon);
                    airport.magVariation = line.Substring(586, 3).Trim();

                    // If Magnetic Variation is NOT empty Continue with the airport and runway building.
                    if (airport.magVariation != string.Empty)
                    {
                        // If the Mag variation Declination is W
                        if (airport.magVariation[2] == 'W')
                        {
                            // Remove the 'W' and make positive number.
                            airport.magVariation = $"{airport.magVariation.Substring(0, 2)}";
                        }
                        else
                        {
                            // If it is not 'W', Make Negetive number
                            airport.magVariation = $"-{airport.magVariation.Substring(0, 2)}";
                        }
                    }
                }

                // If the Line starts with "RWY", This Airport has runway information.
                else if (line.Substring(0,3) == "RWY")
                {
                    // Create a new Runway Model
                    RunwayModel rwy = new RunwayModel();

                    // Populate that runway model with our data from the FAA
                    rwy.RwyGroup = line.Substring(16, 7).Trim();
                    rwy.RwyLength = line.Substring(23, 5).Trim();
                    rwy.RwyWidth = line.Substring(28, 4).Trim();
                    rwy.BaseRwyHdg = line.Substring(68, 3).Trim();
                    rwy.RecRwyHdg = line.Substring(290, 3).Trim();

                    // Make sure the Base Rwy heading is not empty and make sure mag variation is not empty 
                    if (rwy.BaseRwyHdg != string.Empty && airport.magVariation != string.Empty)
                    {
                        // Set base Rwy Hdg from true to mag variation accounted for hdg.
                        rwy.BaseRwyHdg = (double.Parse(rwy.BaseRwyHdg) + double.Parse(airport.magVariation)).ToString();
                        
                        // Make sure the newly calculated value is between 1 and 360.
                        if (double.Parse(rwy.BaseRwyHdg) > 360)
                        {
                            rwy.BaseRwyHdg = (double.Parse(rwy.BaseRwyHdg) - 360).ToString();
                        }
                        else if (double.Parse(rwy.BaseRwyHdg) <= 0)
                        {
                            rwy.BaseRwyHdg = (double.Parse(rwy.BaseRwyHdg) + 360).ToString();
                        }
                    }

                    // Make sure the REC Rwy heading is not empty and make sure mag variation is not empty
                    if (rwy.RecRwyHdg != string.Empty && airport.magVariation != string.Empty)
                    {
                        // Set REC Rwy Hdg from true to mag variation accounted for hdg.
                        rwy.RecRwyHdg = (double.Parse(rwy.RecRwyHdg) + double.Parse(airport.magVariation)).ToString();
                        
                        // Make sure the newly calculated value is between 1 and 360.
                        if (double.Parse(rwy.RecRwyHdg) > 360)
                        {
                            rwy.RecRwyHdg = (double.Parse(rwy.RecRwyHdg) - 360).ToString();
                        }
                        else if (double.Parse(rwy.RecRwyHdg) <= 0)
                        {
                            rwy.RecRwyHdg = (double.Parse(rwy.RecRwyHdg) + 360).ToString();
                        }
                    }

                    // Make sure the Base Start Lat and Lon are not empty.
                    if (line.Substring(88, 15).Trim() != string.Empty && line.Substring(115, 15).Trim() != string.Empty)
                    {
                        // Get and set the Lat and Lon for Start
                        rwy.BaseStartLat = new GlobalConfig().CorrectLatLon(line.Substring(88, 15).Trim(), true, GlobalConfig.Convert);
                        rwy.BaseStartLon = new GlobalConfig().CorrectLatLon(line.Substring(115, 15).Trim(), false, GlobalConfig.Convert);
                    }

                    // Make sure the Base End Lat and Lon are not empty.
                    if (line.Substring(310, 15).Trim() != string.Empty && line.Substring(337, 15).Trim() != string.Empty)
                    {
                        // Get and set the Lat and Lon for End
                        rwy.BaseEndLat = new GlobalConfig().CorrectLatLon(line.Substring(310, 15).Trim(), true, GlobalConfig.Convert);
                        rwy.BaseEndLon = new GlobalConfig().CorrectLatLon(line.Substring(337, 15).Trim(), false, GlobalConfig.Convert);
                    }

                    // Add the runway info to the current airport runway list.
                    airport.Runways.Add(rwy);
                }
            }

            // If the airport is operational add it to our list of Airports.
            if (airport.Status == "O")
            {
                allAptModels.Add(airport);
            }
        }

        /// <summary>
        /// Parse the Wx Station Data
        /// </summary>
        /// <param name="color">Color in Number format, needed for VRC to draw.</param>
        private void ParseWxStationData(string color)
        {
            // Read each line of the WXL.TXT File
            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\wxStations\\WXL.txt"))
            {
                // We only want the weather stations that are this type
                string[] keepTypes = new string[] { "FT", "METAR", "SA" };

                // Remove unwanted characters
                char[] removeChar = new char[] { ' ', '.' };

                // Create the Wx Station Model
                WxStationModel station = new WxStationModel
                {
                    Id = line.Substring(0, 5).Trim(removeChar),
                    Lat_N_S = line.Substring(12, 1).Trim(removeChar),
                    LatDeg = $"0{line.Substring(5, 2).Trim(removeChar)}",
                    LatMin = line.Substring(7, 2).Trim(removeChar),
                    LatSec = line.Substring(9, 2).Trim(removeChar),
                    Lon_E_W = line.Substring(21, 1).Trim(removeChar),
                    LonDeg = line.Substring(13, 3).Trim(removeChar),
                    LonMin = line.Substring(16, 2).Trim(removeChar),
                    LonSec = line.Substring(18, 2).Trim(removeChar),
                    Type = line.Substring(59, 60).Trim(removeChar),
                    ColorDefine = color
                };

                // Loop through the Airports in our list
                foreach (AptModel apt in allAptModels)
                {
                    // Check the station ID against the Airport ID. If it is the same add the station Name.
                    if (station.Id == apt.Id)
                    {
                        station.Name = apt.Name;
                        break;
                    }
                }

                // Check the type to see if it is in our wanted types.
                string[] stationType = station.Type.Split();
                
                // Boolean to let us know if we want to keep the station. For the start we set it to False.
                bool keep = false;

                // Loop through all of our Types that we want to keep.
                foreach (string tp in stationType)
                {
                    // If the station type contains any type we want to keep set our boolean to true.
                    if (keepTypes.Contains(tp))
                    {
                        keep = true;
                        break;
                    }
                }

                // If keep is TRUE
                if (keep)
                {
                    // We set the name of the weather station based on if it matches a APT ID. If a Name exists then we want it. Add it to the list.
                    if (station.Name != "" && station.Name != null)
                    {
                        // Set the Station's Lat and Lon
                        station.LatCorrect = new GlobalConfig().CorrectLatLon(station.Lat, true, GlobalConfig.Convert);
                        station.LonCorrect = new GlobalConfig().CorrectLatLon(station.Lon, false, GlobalConfig.Convert);

                        // Set the stations Decimal Verison of Lat and Lon
                        station.Dec_Lat = new GlobalConfig().createDecFormat(station.LatCorrect);
                        station.Dec_Lon = new GlobalConfig().createDecFormat(station.LonCorrect);

                        // Add the Wx Station Model to our List of ALL station Models.
                        allWxStationsInData.Add(station);
                    }
                }
            }
        }

        /// <summary>
        /// Write the VERAM and VSTARS File
        /// </summary>
        /// <param name="effectiveDate">Airacc Effective Date: "YYYY-MM-DD"</param>
        public void WriteEramAirportsXML(string effectiveDate)
        {
            // File path to where we want to store the Airports.xml file.
            string filePath = $"{GlobalConfig.outputDirectory}\\VERAM\\Airports.xml";

            // Create an Empty list for our Airports in the format we need.
            List<Airport> allAptForXML = new List<Airport>();

            // Using the XML Serializer - this creates and gets the Serializer ready to use.
            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("Airports");
            XmlSerializer serializer = new XmlSerializer(typeof(Airport[]), xmlRootAttribute);

            // Loop through all of our Airports that we have collected
            foreach (AptModel aptModel in allAptModels)
            {
                // Boolean to know if we want to use the runway.
                bool doNotUseThisRwy = false;

                // We need a temp string variable for our Airport ID
                string aptIdTempVar;

                // Create an Empty list of Runway data for the XML
                List<Runway> rwysTempVar = new List<Runway>();

                // Check to see if the ICAO exists.
                if (aptModel.Icao != string.Empty)
                {
                    // If it does, set our temp apt id var to the ICAO 
                    aptIdTempVar = aptModel.Icao;
                }
                else
                {
                    // if it doesn't, set our temp apt id var to the FAA id. 
                    aptIdTempVar = aptModel.Id;
                }

                // Loop through the runway list inside of our Airport Model.
                foreach (RunwayModel runwayModel in aptModel.Runways)
                {
                    // This list is used to make sure nothing in here is EMPTY or Null.
                    List<string> rwyProperties = new List<string>() 
                    {
                        runwayModel.RwyGroup,
                        runwayModel.BaseRwy,
                        runwayModel.RecRwy,
                        runwayModel.RwyLength,
                        runwayModel.RwyWidth,
                        runwayModel.BaseStartLat,
                        runwayModel.BaseStartLon,
                        runwayModel.BaseEndLat,
                        runwayModel.BaseEndLon,
                        runwayModel.RecStartLat,
                        runwayModel.RecStartLon,
                        runwayModel.RecEndLat,
                        runwayModel.RecEndLon,
                        runwayModel.BaseRwyHdg,
                        runwayModel.RecRwyHdg
                    };

                    // Loop through All the Properties
                    foreach (string stringProperty in rwyProperties)
                    {
                        // Make sure it does NOT equal "" or Null.
                        if (string.IsNullOrEmpty(stringProperty))
                        {
                            // If it does, tell our program to not use this runway data info. 
                            doNotUseThisRwy = true;
                            break;
                        }
                    }

                    // If our boolean is False, this means the Runway data is good and complete and we can use it.
                    if (doNotUseThisRwy == false)
                    {
                        // Set our Base Runway Model.
                        Runway rwy1 = new Runway
                        {
                            ID = runwayModel.BaseRwy,
                            Heading = runwayModel.BaseRwyHdg,
                            Length = runwayModel.RwyLength,
                            Width = runwayModel.RwyWidth,
                            StartLoc = new StartLoc
                            {
                                Lon = new GlobalConfig().createDecFormat(runwayModel.BaseStartLon),
                                Lat = new GlobalConfig().createDecFormat(runwayModel.BaseStartLat)
                            },
                            EndLoc = new EndLoc
                            {
                                Lon = new GlobalConfig().createDecFormat(runwayModel.BaseEndLon),
                                Lat = new GlobalConfig().createDecFormat(runwayModel.BaseEndLat)
                            }
                        };

                        // Set our REC Runway Model
                        Runway rwy2 = new Runway
                        {
                            ID = runwayModel.RecRwy,
                            Heading = runwayModel.RecRwyHdg,
                            Length = runwayModel.RwyLength,
                            Width = runwayModel.RwyWidth,
                            StartLoc = new StartLoc
                            {
                                Lon = new GlobalConfig().createDecFormat(runwayModel.RecStartLon),
                                Lat = new GlobalConfig().createDecFormat(runwayModel.RecStartLat)
                            },
                            EndLoc = new EndLoc
                            {
                                Lon = new GlobalConfig().createDecFormat(runwayModel.RecEndLon),
                                Lat = new GlobalConfig().createDecFormat(runwayModel.RecEndLat)
                            }
                        };

                        // Add Both "Runways" to our runways list.
                        rwysTempVar.Add(rwy1);
                        rwysTempVar.Add(rwy2);
                    }
                }

                // Create our Airport Model for XML
                Airport aptXMLFormat = new Airport
                {
                    ID = aptIdTempVar,
                    Name = aptModel.Name,
                    Elevation = aptModel.Elv,
                    MagVar = aptModel.magVariation,
                    Frequency = "0",
                    Location = new Location
                    {
                        Lon = aptModel.Lon_Dec,
                        Lat = aptModel.Lat_Dec
                    },
                    Runways = new Runways
                    {
                        Runway = rwysTempVar
                    }
                };

                // Make sure we have 1 or more runways in our list and also make sure Mag Variation is not empty.
                if (rwysTempVar.Count >= 1 && aptXMLFormat.MagVar != "")
                {
                    // Add our Complete Airport information to our list of Airport Models for XML.
                    allAptForXML.Add(aptXMLFormat);
                }
            }

            // We have to convert our List into an Array.
            Airport[] aptArrayForXML = allAptForXML.ToArray();

            // This is going to write the XML serializer to the xml file.
            TextWriter writer = new StreamWriter(filePath);
            serializer.Serialize(writer, aptArrayForXML);
            writer.Close();

            // At the very end of the file add the AIRAC Effective date in XML comment form. 
            File.AppendAllText(filePath, $"\n<!--AIRAC_EFFECTIVE_DATE {effectiveDate}-->");

            // VSTARS and VERAM use the same file format. Copy the file to the VSTARS output File.
            File.Copy($"{GlobalConfig.outputDirectory}\\VERAM\\Airports.xml", $"{GlobalConfig.outputDirectory}\\VSTARS\\Airports.xml");
        }

        /// <summary>
        /// Store Waypoints for XML Data. We can't write the file yet since Waypoints contains all of the following:
        /// Airports, Fixxes, NDB, VOR. So we Store it to the Global list. Once all of these have been added, 
        /// then we can write the xml file.
        /// </summary>
        public void StoreWaypointsXMLData() 
        {
            // Create an Empty list of Waypoints.
            List<Waypoint> waypointList = new List<Waypoint>();

            // Loop through all of our Airport Models.
            foreach (AptModel apt in allAptModels)
            {
                // Create our Location Model for XML
                Location loc = new Location { Lat = apt.Lat_Dec, Lon = apt.Lon_Dec};

                // Create our Waypoint Model for XML
                Waypoint wpt = new Waypoint
                {
                    Type = "Airport",
                    Location = loc
                };

                // Check to see if ICAO exists
                if (apt.Icao != string.Empty)
                {
                    // If it does, set the ID to the ICAO
                    wpt.ID = apt.Icao;
                }
                else
                {
                    // If it doesnt, set the ID to the FAA ID
                    wpt.ID = apt.Id;
                }

                // Add the waypoint to our list.
                waypointList.Add(wpt);
            }

            // Loop through All previous Waypoint data that we have stored and add it to our List
            foreach (Waypoint globalWaypoint in GlobalConfig.waypoints)
            {
                waypointList.Add(globalWaypoint);
            }

            // Make the Global Waypoints List equal to our current list with everything we added and had.
            GlobalConfig.waypoints = waypointList.ToArray();
        }

        /// <summary>
        /// Write the APT In scope reference.
        /// </summary>
        /// <param name="Artcc">ARTCC Code</param>
        private void WriteAptISR(string Artcc) 
        {
            // File path to save the ISR
            string filePath = $"{GlobalConfig.outputDirectory}\\ISR\\ISR_APT.txt";

            // String builder for all the ISR realitive information
            StringBuilder sb = new StringBuilder();
            foreach (AptModel dataforEachApt in allAptModels)
            {
                // Variables that are in the MODEL but need some manipulating
                string icao;
                string tower;
                string elvation;

                // .Icao is just stored as an empty string "", we want it to say "N/A". If it is an empty string change it to say N/A
                if (dataforEachApt.Icao == "") { icao = "N/A"; } else { icao = dataforEachApt.Icao; }

                // .Twr is stored as a "Y" or "N". We want it to say "Towered" or "Not Towered" respectivly.
                if (dataforEachApt.Twr == "Y") { tower = "Towered"; } else { tower = "Not Towered"; }

                // .Elv is stored with a Decimal point. We want only the number before the decimal point.
                elvation = dataforEachApt.Elv;

                // Add the Line for Every Airport Possible based on the .Id
                sb.AppendLine($".APT{dataforEachApt.Id} .MSG {Artcc}_ISR *** FAA-{dataforEachApt.Id} : ICAO-{icao} ___ {dataforEachApt.Name} {dataforEachApt.Type} ___ {elvation}'MSL ___ {tower} ___ {dataforEachApt.ResArtcc}");

                // Add another command to account for the ICAO Command.
                if (dataforEachApt.Icao != "")
                {
                    sb.AppendLine($".APT{dataforEachApt.Icao} .MSG {Artcc}_ISR *** FAA-{dataforEachApt.Id} : ICAO-{icao} ___ {dataforEachApt.Name} {dataforEachApt.Type} ___ {elvation}'MSL ___ {tower} ___ {dataforEachApt.ResArtcc}");
                }
            }

            // Write the String Builder to the file.
            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Create the Sct2 file for Airports
        /// </summary>
        private void WriteAptSctData()
        {
            // File path and file name of the file we are about to create.
            string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\APT.sct2";

            // Populate this new string builder with the Airport information for each airport model in our list.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[AIRPORT]");
            foreach (AptModel dataforEachApt in allAptModels) 
            {
                // Variables to keep track of what to use in our sct2 file for the "id/ICAO" and "ctaf"
                string id_icao;
                string ctaf;

                // If the airport/facility does not have an ICAO use the FAA 3LD
                if (dataforEachApt.Icao == ""){id_icao = dataforEachApt.Id;}else{id_icao = dataforEachApt.Icao;}

                // If the airport/facility does not have a CTAF, insert "000.000".
                if (dataforEachApt.Ctaf == ""){ctaf = "000.000";}else{ctaf = dataforEachApt.Ctaf;}

                // Add the information to our string builder so we can write it to the file when we are done.
                sb.AppendLine($"{id_icao.PadRight(5)}{ctaf.PadRight(8)}{dataforEachApt.Lat} {dataforEachApt.Lon} ;{dataforEachApt.Name} {dataforEachApt.Type}");
            }

            // Write all the data to our apt.sct2 file.
            File.WriteAllText(filePath, sb.ToString());

            // Write some New lines to the end of the file.
            File.AppendAllText(filePath, $"\n\n\n\n\n\n");

            // Add this file to our TEST SECTOR FILE.
            File.AppendAllText($"{GlobalConfig.outputDirectory}\\Test_Sct_File.sct2", File.ReadAllText(filePath));
        }

        /// <summary>
        /// Write the WX Station Sct File
        /// </summary>
        private void WriteWxStationSctData()
        {
            // File Path to the file we want to write to
            string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\WxStations.sct2";

            // String Builder to store all the lines we want to write to the file.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[LABELS]");
            sb.AppendLine("; Weather Stations");

            // Loop through all the Wx Station Models.
            foreach (WxStationModel stationModel in allWxStationsInData)
            {
                // Add the line to the string builder.
                sb.AppendLine($"\"{stationModel.Id} {stationModel.Name}\" {stationModel.LatCorrect} {stationModel.LonCorrect} {stationModel.ColorDefine}");
            }

            // Write to the file.
            File.WriteAllText(filePath, sb.ToString());

            // Add some new lines to the end of the file.
            File.AppendAllText(filePath, $"\n\n\n\n\n\n");

            // Add this file to our TEST SECTOR file.
            File.AppendAllText($"{GlobalConfig.outputDirectory}\\Test_Sct_File.sct2", File.ReadAllText(filePath));
        }

        /// <summary>
        /// Removes downloaded FAA data. 
        /// </summary>
        private void deleteUnneededDir()
        {
            // Delete our Zip folder for Airports we downloaded from FAA
            File.Delete(zipFolder);

            // Delete our unziped folder and Apt document.
            Directory.Delete(unzipedFolder, true);
        }

        /// <summary>
        /// Delete the WX Donloaded Files.
        /// </summary>
        private void deleteUnneededWXDir()
        {
            // Delete WX Zip Folder
            File.Delete(WxzipFolder);

            // Delete WX Unziped folder
            Directory.Delete(WxunzipedFolder, true);
        }
    }
}
