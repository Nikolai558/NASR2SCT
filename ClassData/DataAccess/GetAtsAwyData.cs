﻿using ClassData.Models;
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
    public class GetAtsAwyData
    {
        // Create empty list to hold All of the Airway Points
        List<atsAwyPointModel> allAtsAwyPoints = new List<atsAwyPointModel>();

        //// Create empty list to hold all of the Airways.
        List<AtsAirwayModel> allAtsAwy = new List<AtsAirwayModel>();

        Dictionary<string, List<string>> awyFixes = new Dictionary<string, List<string>>();

        /// <summary>
        /// Calls All the needed functions.
        /// </summary>
        /// <param name="effectiveDate">Format: YYYY-MM-DD</param>
        public void AWYQuarterbackFunc(string effectiveDate)
        {
            DownloadAwyData(effectiveDate);
            ParseAtsData();
            WriteAwySctData();
            WriteAwyAlias();
        }

        /// <summary>
        /// Download the ATS Awy data from the FAA.
        /// </summary>
        /// <param name="effectiveDate">Format: YYYY-MM-DD</param>
        private void DownloadAwyData(string effectiveDate)
        {
            // Create Web Client to Connect to the FAA website.
            var client = new WebClient();

            // Download the AWY Zip File
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/ATS.zip", $"{GlobalConfig.tempPath}\\atsAwy.zip");

            // Unzip the File we just downloaded
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\atsAwy.zip", $"{GlobalConfig.tempPath}\\atsAwy");
        }

        /// <summary>
        /// Parse the ATS Data from the FAA
        /// </summary>
        private void ParseAtsData()
        {
            atsAwyPointModel atsPoint = new atsAwyPointModel();

            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\atsAwy\\ATS.txt"))
            {
                if (line.Substring(0, 4) == "ATS1")
                {
                    atsPoint = new atsAwyPointModel();

                    if (line.Substring(118, 1) == "X")
                    {
                        atsPoint.GapAfter = true;
                    }

                    if (line.Substring(156, 1) == " ")
                    {
                        atsPoint.EndOfAirway = true;
                    }

                    if (line.IndexOf("BORDER", 156, 40) != -1)
                    {
                        atsPoint.BorderAfter = true;
                    }
                }
                else if (line.Substring(0, 4) == "ATS2")
                {
                    atsPoint.AirwayId = line.Substring(6, 12).Trim();

                    if (atsPoint.AirwayId.Length > 6)
                    {
                        if (atsPoint.AirwayId.Substring(0, 6) == "ROUTE ")
                        {
                            List<string> newRTEString = atsPoint.AirwayId.Split(' ').ToList();
                            newRTEString[1] = newRTEString[1].TrimStart(new char[] { '0' });
                            atsPoint.AirwayId = $"RTE{newRTEString[1]}";
                        }
                    }
                    

                    atsPoint.Name = line.Substring(25, 40).Trim();
                    atsPoint.Lat = new GlobalConfig().CorrectLatLon(line.Substring(109, 14).Trim(), true, GlobalConfig.Convert);
                    atsPoint.Lon = new GlobalConfig().CorrectLatLon(line.Substring(123, 14).Trim(), false, GlobalConfig.Convert);
                    atsPoint.Dec_Lat = new GlobalConfig().createDecFormat(atsPoint.Lat, true);
                    atsPoint.Dec_Lon = new GlobalConfig().createDecFormat(atsPoint.Lon, true);

                    if (line.Substring(65, 25).Trim() == "NDB")
                    {
                        atsPoint.Type = "NDB";
                    }
                    else
                    {
                        atsPoint.Type = "VOR";
                    }

                    if (line.Substring(90, 15).Trim() == "FIX")
                    {
                        atsPoint.Type = "FIX";
                    }

                    if (line.Substring(142, 4).Trim() != string.Empty)
                    {
                        atsPoint.PointId = line.Substring(142, 4).Trim();
                    }
                    else
                    {
                        atsPoint.PointId = atsPoint.Name;
                    }

                    allAtsAwyPoints.Add(atsPoint);
                }
            }

            AtsAirwayModel atsAwy = new AtsAirwayModel();
            int totalPoints = allAtsAwyPoints.Count();
            int currentPointCount = 0;

            foreach (atsAwyPointModel point in allAtsAwyPoints)
            {
                currentPointCount += 1;
                if (point.AirwayId == atsAwy.Id)
                {
                    atsAwy.atsAwyPoints.Add(point);
                }
                else
                {
                    if (atsAwy.Id != null)
                    {
                        allAtsAwy.Add(atsAwy);
                    }

                    atsAwy = new AtsAirwayModel();
                    atsAwy.Id = point.AirwayId;
                    atsAwy.atsAwyPoints = new List<atsAwyPointModel>();
                    atsAwy.atsAwyPoints.Add(point);
                }

                if (totalPoints == currentPointCount)
                {
                    allAtsAwy.Add(atsAwy);
                }
            }
        }

        private void WriteAwyAlias()
        {
            foreach (atsAwyPointModel pointModel in allAtsAwyPoints)
            {

                if (!awyFixes.ContainsKey(pointModel.AirwayId))
                {
                    awyFixes.Add(pointModel.AirwayId, new List<string>());
                }

                awyFixes[pointModel.AirwayId].Add(pointModel.PointId);
            }

            string awyAliasFilePath = $"{GlobalConfig.outputDirectory}\\ALIAS\\AWY_ALIAS.txt";
            StringBuilder sb = new StringBuilder();

            foreach (string awyId in awyFixes.Keys)
            {
                string saveString = $".{awyId}F .FF ";
                string allFixesToSave = string.Join(" ", awyFixes[awyId]);
                saveString += allFixesToSave;
                sb.AppendLine(saveString);
            }

            File.AppendAllText(awyAliasFilePath, sb.ToString());
        }

        /// <summary>
        /// Write the AWY Sector File data to a file. this will be under [HIGH AIRWAYS]
        /// </summary>
        private void WriteAwySctData()
        {
            // Set our File Path
            string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\[LOW AIRWAY].sct2";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[LOW AIRWAY]");

            foreach (AtsAirwayModel airway in allAtsAwy)
            {
                atsAwyPointModel prevPoint = new atsAwyPointModel();

                foreach (atsAwyPointModel point in airway.atsAwyPoints)
                {
                    if (prevPoint.AirwayId == null)
                    {
                        // Set Previous Point = Current Point
                        prevPoint = point;
                    }
                    else
                    {
                        sb.AppendLine($"{airway.Id.PadRight(27)}{prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId.PadRight(5)} {point.PointId.PadRight(5)}");

                        if (point.GapAfter)
                        {
                            prevPoint = new atsAwyPointModel();
                            sb.AppendLine("\n;GAP\n");
                        }
                        else
                        {
                            prevPoint = point;
                        }
                    }
                }
                    sb.AppendLine();

            }
                File.WriteAllText(filePath, sb.ToString());
                File.AppendAllText(filePath, $"\n\n\n\n\n\n");
                File.AppendAllText($"{GlobalConfig.outputDirectory}\\{GlobalConfig.testSectorFileName}", File.ReadAllText(filePath));
        }
    }
}
