﻿using ClassData.Models;
using NASARData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.DataAccess
{
    public class GetStarDpData
    {
        List<StarAndDpModel> allProcedures = new List<StarAndDpModel>();
        Dictionary<string, List<string>> procedureLines = new Dictionary<string, List<string>>();
        Dictionary<string, List<StarAndDpModel>> procedures = new Dictionary<string, List<StarAndDpModel>>();

        

        public void StarDpQuaterBackFunc(string EffectiveDate) 
        {
            ParseStarDp(EffectiveDate);
            //WriteSctData();
            WriteGeoMap();
            WriteAlias();
            WriteSctDiagrams();
        }

        private void ParseStarDp(string effectiveDate)
        {
            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\{effectiveDate}_STARDP\\STARDP.txt"))
            {

                string procId = line.Substring(0, 5);

                StarAndDpModel Star_Dp = new StarAndDpModel
                {
                    Type = line.Substring(0, 1).Trim(),
                    SeqNumber = line.Substring(1, 4).Trim(),
                    PointCode = line.Substring(10, 2).Trim(),
                    PointId = line.Substring(30, 6).Trim(),
                    ComputerCode = line.Substring(38, 13).Trim(),
                    Lat_N_S = line.Substring(13, 1).Trim(),
                    Lat_Deg = "0" + line.Substring(14, 2).Trim(),
                    Lat_Min = line.Substring(16, 2).Trim(),
                    Lat_Sec = line.Substring(18, 2).Trim(),
                    Lat_MS = line.Substring(20, 1).Trim() + "00",
                    Lon_E_W = line.Substring(21, 1).Trim(),
                    Lon_Deg = line.Substring(22, 3).Trim(),
                    Lon_Min = line.Substring(25, 2).Trim(),
                    Lon_Sec = line.Substring(27, 2).Trim(),
                    Lon_MS = line.Substring(29, 1).Trim() + "00"
                };

                if (procedureLines.ContainsKey(procId))
                {
                    procedureLines[procId].Add(line);
                    procedures[procId].Add(Star_Dp);
                }
                else
                {
                    procedureLines[procId] = new List<string>();
                    procedureLines[procId].Add(line);

                    procedures[procId] = new List<StarAndDpModel>();
                    procedures[procId].Add(Star_Dp);
                }
            }
        }

        private void WriteGeoMap() 
        {
            StringBuilder allDpGeoMaps = new StringBuilder();
            allDpGeoMaps.AppendLine("        <GeoMapObject Description=\"ALL_DPs\" TdmOnly=\"false\">");
            allDpGeoMaps.AppendLine("          <LineDefaults Bcg=\"8\" Filters=\"8\" Style=\"Solid\" Thickness=\"1\" />");
            allDpGeoMaps.AppendLine("          <Elements>");

            StringBuilder allStarGeoMaps = new StringBuilder();
            allStarGeoMaps.AppendLine("        <GeoMapObject Description=\"ALL_STARs\" TdmOnly=\"false\">");
            allStarGeoMaps.AppendLine("          <LineDefaults Bcg=\"18\" Filters=\"18\" Style=\"Solid\" Thickness=\"1\" />");
            allStarGeoMaps.AppendLine("          <Elements>");

            bool newSeq;
            foreach (string procKey in procedures.Keys)
            {
                newSeq = true;
                string currentComputerCode;

                if (procedures[procKey][0].ComputerCode.IndexOf("NOT ASSIGNED") != -1)
                {
                    continue;
                }
                else if (procedures[procKey][0].Type == "S")
                {
                    currentComputerCode = procedures[procKey][0].ComputerCode.Split('.')[1].Substring(0, procedures[procKey][0].ComputerCode.Split('.')[1].Length - 1);
                }
                else if (procedures[procKey][0].Type == "D")
                {
                    currentComputerCode = procedures[procKey][0].ComputerCode.Split('.')[0].Substring(0, procedures[procKey][0].ComputerCode.Split('.')[0].Length - 1);
                }
                else
                {
                    throw new NotImplementedException();
                }

                List<string> allFixes = new List<string>();

                StarAndDpModel prevPoint = null;
                foreach (StarAndDpModel point in procedures[procKey])
                {
                    if (point.PointCode == "AA")
                    {
                        newSeq = true;
                        prevPoint = null;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(point.ComputerCode))
                    {
                        newSeq = true;
                        prevPoint = point;
                    }

                    if (newSeq != true && prevPoint != null)
                    {
                        if (point.Type == "S")
                        {
                            allStarGeoMaps.AppendLine($"            <Element xsi:type=\"Line\" Filters=\"\" StartLat=\"{GlobalConfig.CreateDecFormat(prevPoint.Lat, true)}\" StartLon=\"{GlobalConfig.CreateDecFormat(prevPoint.Lon, true)}\" EndLat=\"{GlobalConfig.CreateDecFormat(point.Lat, true)}\" EndLon=\"{GlobalConfig.CreateDecFormat(point.Lon, true)}\" />");
                        }
                        else
                        {
                            allDpGeoMaps.AppendLine($"            <Element xsi:type=\"Line\" Filters=\"\" StartLat=\"{GlobalConfig.CreateDecFormat(prevPoint.Lat, true)}\" StartLon=\"{GlobalConfig.CreateDecFormat(prevPoint.Lon, true)}\" EndLat=\"{GlobalConfig.CreateDecFormat(point.Lat, true)}\" EndLon=\"{GlobalConfig.CreateDecFormat(point.Lon, true)}\" />");
                        }

                        prevPoint = point;
                    }

                    if (!allFixes.Contains(point.PointId) && point.PointCode != "AA" && point.PointCode != "NA")
                    {
                        allFixes.Add(point.PointId);
                    }

                    newSeq = false;
                }
            }

            allDpGeoMaps.AppendLine("          </Elements>");
            allDpGeoMaps.AppendLine("        </GeoMapObject>");

            allStarGeoMaps.AppendLine("          </Elements>");
            allStarGeoMaps.AppendLine("        </GeoMapObject>");

            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VERAM\\ALL_DP_GEOMAP.xml", allDpGeoMaps.ToString());
            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VERAM\\ALL_STAR_GEOMAP.xml", allStarGeoMaps.ToString());
        }

        private void WriteAlias() 
        {
            string saveFilePath = $"{GlobalConfig.outputDirectory}\\ALIAS\\STAR_DP_Fixes_Alias.txt";
            StringBuilder sb = new StringBuilder();

            Dictionary<string, List<StarAndDpModel>> aliasCommands = new Dictionary<string, List<StarAndDpModel>>();


            foreach (string procSeqNumber in procedures.Keys)
            {
                List<StarAndDpModel> activePoints = new List<StarAndDpModel>();
                bool isDuplicatePointId = false;

                string currentComputerCode;

                if (procedures[procSeqNumber][0].ComputerCode.IndexOf("NOT ASSIGNED") != -1)
                {
                    // Um this might skip procedures...... Might have an issue here? We will see. 
                    continue;
                }
                else if (procedures[procSeqNumber][0].Type == "S")
                {
                    currentComputerCode = procedures[procSeqNumber][0].ComputerCode.Split('.')[1].Substring(0, procedures[procSeqNumber][0].ComputerCode.Split('.')[1].Length - 1);
                }
                else if (procedures[procSeqNumber][0].Type == "D")
                {
                    currentComputerCode = procedures[procSeqNumber][0].ComputerCode.Split('.')[0].Substring(0, procedures[procSeqNumber][0].ComputerCode.Split('.')[0].Length - 1);
                }
                else
                {
                    throw new NotImplementedException();
                }

                bool clearPoints = false;
                
                int lineCount = 0;
                bool addAllEndingPoints = false;

                List<string> allAirportsInProcedure = new List<string>();
                List<int> pointIndex = new List<int>();

                foreach (StarAndDpModel indvData in procedures[procSeqNumber])
                {
                    lineCount += 1;

                    if (lineCount == procedures[procSeqNumber].Count)
                    {
                        // We are at the last line. is it anything but AA or NA If so we need to add all the points including the one we are on to ALL Airport commands.
                        if (indvData.PointCode != "NA" && indvData.PointCode != "AA")
                        {
                            addAllEndingPoints = true;
                        }
                        else
                        {
                            addAllEndingPoints = false;
                        }
                    }

                    if (indvData.PointCode == "AA")
                    {
                        if (!allAirportsInProcedure.Contains(indvData.PointId))
                        {
                            allAirportsInProcedure.Add(indvData.PointId.Trim());
                        }

                        foreach (int index in pointIndex)
                        {
                            if (!procedures[procSeqNumber][index].AirpotsThisPointServes.Contains(indvData.PointId))
                            {
                                procedures[procSeqNumber][index].AirpotsThisPointServes.Add(indvData.PointId);
                            }
                        }

                        if (!aliasCommands.ContainsKey($".{indvData.PointId.Trim()}{currentComputerCode}F"))
                        {
                            aliasCommands[$".{indvData.PointId.Trim()}{currentComputerCode}F"] = new List<StarAndDpModel>();
                            foreach (StarAndDpModel point in activePoints)
                            {
                                foreach (StarAndDpModel aliasPoint in aliasCommands[$".{indvData.PointId.Trim()}{currentComputerCode}F"])
                                {
                                    if (aliasPoint.PointId == point.PointId)
                                    {
                                        // We already have it in the data Continue.
                                        isDuplicatePointId = true;
                                        break;
                                    }
                                    else
                                    {
                                        isDuplicatePointId = false;
                                    }
                                }

                                if (aliasCommands[$".{indvData.PointId.Trim()}{currentComputerCode}F"].Count == 0)
                                {
                                    isDuplicatePointId = false;
                                }

                                if (!isDuplicatePointId)
                                {
                                    aliasCommands[$".{indvData.PointId.Trim()}{currentComputerCode}F"].Add(point);
                                }
                            }
                        }
                        else
                        {
                            foreach (StarAndDpModel point in activePoints)
                            {
                                foreach (StarAndDpModel aliasPoint in aliasCommands[$".{indvData.PointId.Trim()}{currentComputerCode}F"])
                                {
                                    if (aliasPoint.PointId == point.PointId)
                                    {
                                        // We already have it in the data Continue.
                                        isDuplicatePointId = true;
                                        break;
                                    }
                                    else
                                    {
                                        isDuplicatePointId = false;
                                    }
                                }

                                if (!isDuplicatePointId)
                                {
                                    aliasCommands[$".{indvData.PointId.Trim()}{currentComputerCode}F"].Add(point);
                                }
                            }
                        }

                        clearPoints = true;
                    }
                    else if (indvData.PointCode == "NA")
                    {
                        // Do nothing here but not Continue???
                    }
                    else
                    {
                        if (clearPoints)
                        {
                            activePoints = new List<StarAndDpModel>();
                            clearPoints = false;

                            pointIndex = new List<int>();
                        }

                        activePoints.Add(indvData);
                        pointIndex.Add(lineCount - 1);
                    }

                    if (addAllEndingPoints)
                    {

                        foreach (string apt in allAirportsInProcedure)
                        {
                            foreach (StarAndDpModel point in activePoints)
                            {
                                if (!point.AirpotsThisPointServes.Contains(apt))
                                {
                                    point.AirpotsThisPointServes.Add(apt);
                                }

                                foreach (StarAndDpModel aliasPoint in aliasCommands[$".{apt}{currentComputerCode}F"])
                                {
                                    if (aliasPoint.PointId == point.PointId)
                                    {
                                        // We already have it in the data Continue.
                                        isDuplicatePointId = true;
                                        break;
                                    }
                                    else
                                    {
                                        isDuplicatePointId = false;
                                    }
                                }

                                if (!isDuplicatePointId)
                                {
                                    aliasCommands[$".{apt}{currentComputerCode}F"].Add(point);
                                }
                            }
                        }
                    }
                }
            }

            // Get the Alias Stringbuilder with all the Diagram fixes
            foreach (string command in aliasCommands.Keys)
            {
                string output = $"{command} .FF";
                foreach (StarAndDpModel point in aliasCommands[command])
                {
                    output += $" {point.PointId}";
                }

                if (output.ToString().Split('.')[2].Length > 3)
                {
                    sb.AppendLine(output);
                }
            }

            File.WriteAllText(saveFilePath, sb.ToString());
            File.AppendAllText($"{GlobalConfig.outputDirectory}\\ALIAS\\AliasTestFile.txt", sb.ToString());

        }

        private void WriteSctDiagrams()
        {
            StringBuilder combinedDataDp = new StringBuilder();
            combinedDataDp.AppendLine("[SID]");
            combinedDataDp.AppendLine($"{"All_DPs".PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000");


            StringBuilder combinedDataStar = new StringBuilder();
            combinedDataStar.AppendLine("[STAR]");
            combinedDataStar.AppendLine($"{"All_STARs".PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000");

            Dictionary<string, Dictionary<string, List<string>>> allDiagrams = new Dictionary<string, Dictionary<string, List<string>>>
            {
                { "S", new Dictionary<string, List<string>>() },
                { "D", new Dictionary<string, List<string>>() }
            };


            bool newSeq;
            foreach (string procKey in procedures.Keys)
            {
                newSeq = true;
                string currentComputerCode;

                if (procedures[procKey][0].ComputerCode.IndexOf("NOT ASSIGNED") != -1)
                {
                    continue;
                }
                else if (procedures[procKey][0].Type == "S")
                {
                    currentComputerCode = procedures[procKey][0].ComputerCode.Split('.')[1].Substring(0, procedures[procKey][0].ComputerCode.Split('.')[1].Length - 1);
                }
                else if (procedures[procKey][0].Type == "D")
                {
                    currentComputerCode = procedures[procKey][0].ComputerCode.Split('.')[0].Substring(0, procedures[procKey][0].ComputerCode.Split('.')[0].Length - 1);
                }
                else
                {
                    throw new NotImplementedException();
                }

                StarAndDpModel prevPoint = null;
                foreach (StarAndDpModel point in procedures[procKey])
                {
                    foreach (string apt in point.AirpotsThisPointServes)
                    {
                        if (point.Type == "S")
                        {
                            if (!allDiagrams["S"].ContainsKey($"{apt}_{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000"))
                            {
                                allDiagrams["S"][$"{apt}_{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000"] = new List<string>();
                            }
                        }
                        else
                        {
                            if (!allDiagrams["D"].ContainsKey($"{apt}_{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000"))
                            {
                                allDiagrams["D"][$"{apt}_{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000"] = new List<string>();
                            }
                        }
                    }


                    if (point.PointCode == "AA")
                    {
                        newSeq = true;
                        prevPoint = null;
                        continue;
                    }

                    if (point.PointCode == "NA")
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(point.ComputerCode))
                    {
                        newSeq = true;
                        prevPoint = point;
                    }

                    if (newSeq != true && prevPoint != null)
                    {
                        foreach (string airport in point.AirpotsThisPointServes)
                        {
                            if (point.Type == "S")
                            {
                                allDiagrams["S"][$"{airport}_{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000"].Add($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");
                            }
                            else
                            {
                                allDiagrams["D"][$"{airport}_{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000"].Add($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");
                            }
                        }

                        if (point.Type == "S")
                        {
                            combinedDataStar.AppendLine($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");
                        }
                        else
                        {
                            combinedDataDp.AppendLine($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");
                        }

                        prevPoint = point;
                    }

                    newSeq = false;
                }
            }

            foreach (string star in allDiagrams["S"].Keys)
            {
                StringBuilder starDiagramSb = new StringBuilder();

                starDiagramSb.AppendLine(star);

                foreach (string starDiagram in allDiagrams["S"][star])
                {
                    starDiagramSb.AppendLine(starDiagram);
                }

                if (starDiagramSb.ToString().Length > 91)
                {
                    File.WriteAllText($"{GlobalConfig.outputDirectory}\\VRC\\[STAR]\\{star.Split(' ')[0]}.sct2", starDiagramSb.ToString());
                }
            }

            foreach (string dp in allDiagrams["D"].Keys)
            {
                StringBuilder dpDiagramSb = new StringBuilder();

                dpDiagramSb.AppendLine(dp);

                foreach (string starDiagram in allDiagrams["D"][dp])
                {
                    dpDiagramSb.AppendLine(starDiagram);
                }

                if (dpDiagramSb.ToString().Length > 91)
                {
                    File.WriteAllText($"{GlobalConfig.outputDirectory}\\VRC\\[SID]\\{dp.Split(' ')[0]}.sct2", dpDiagramSb.ToString());
                }
            }


            //foreach (StringBuilder sbDP in listAllIndividualStars)
            //{
            //    if (string.IsNullOrEmpty(sbDP.ToString()))
            //    {
            //        continue;
            //    }

            //    string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\[STAR]\\{sbDP.ToString().Substring(0, 26).Trim()}.sct2";
            //    if (sbDP.Length > 87)
            //    {
            //        File.WriteAllText(filePath, sbDP.ToString());
            //    }
            //}

            //foreach (StringBuilder sbStars in listAllIndividualDp)
            //{
            //    string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\[SID]\\{sbStars.ToString().Substring(0, 26).Trim()}.sct2";

            //    if (sbStars.Length > 87)
            //    {
            //        File.WriteAllText(filePath, sbStars.ToString());
            //    }
            //}

            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VRC\\[STAR]\\000_All_STAR_Combined.sct2", combinedDataStar.ToString());
            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VRC\\[SID]\\000_All_DP_Combined.sct2", combinedDataDp.ToString());

            File.AppendAllText($"{GlobalConfig.outputDirectory}{GlobalConfig.testSectorFileName}", combinedDataStar.ToString());
            File.AppendAllText($"{GlobalConfig.outputDirectory}{GlobalConfig.testSectorFileName}", combinedDataDp.ToString());


        }

        private void WriteSctData()
        {
            StringBuilder combinedDataDp = new StringBuilder();
            combinedDataDp.AppendLine("[SID]");
            combinedDataDp.AppendLine($"{"All_DPs".PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000");


            StringBuilder combinedDataStar = new StringBuilder();
            combinedDataStar.AppendLine("[STAR]");
            combinedDataStar.AppendLine($"{"All_STARs".PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000");

            StringBuilder allDpGeoMaps = new StringBuilder();
            allDpGeoMaps.AppendLine("        <GeoMapObject Description=\"ALL_DPs\" TdmOnly=\"false\">");
            allDpGeoMaps.AppendLine("          <LineDefaults Bcg=\"8\" Filters=\"8\" Style=\"Solid\" Thickness=\"1\" />");
            allDpGeoMaps.AppendLine("          <Elements>");

            StringBuilder allStarGeoMaps = new StringBuilder();
            allStarGeoMaps.AppendLine("        <GeoMapObject Description=\"ALL_STARs\" TdmOnly=\"false\">");
            allStarGeoMaps.AppendLine("          <LineDefaults Bcg=\"18\" Filters=\"18\" Style=\"Solid\" Thickness=\"1\" />");
            allStarGeoMaps.AppendLine("          <Elements>");

            List<StringBuilder> listAllIndividualStars = new List<StringBuilder>();
            List<StringBuilder> listAllIndividualDp = new List<StringBuilder>();
            List<StringBuilder> allAliasCommands = new List<StringBuilder>();

            //combinedData.AppendLine($"{"ALL_STARs".PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000");

            StringBuilder individualData;
            StringBuilder aliasComand;

            bool newSeq;
            foreach (string procKey in procedures.Keys)
            {

                individualData = new StringBuilder();
                aliasComand = new StringBuilder();

                newSeq = true;
                string currentComputerCode;

                if (procedures[procKey][0].ComputerCode.IndexOf("NOT ASSIGNED") != -1)
                {
                    continue;
                }
                else if (procedures[procKey][0].Type == "S")
                {
                    currentComputerCode = procedures[procKey][0].ComputerCode.Split('.')[1].Substring(0, procedures[procKey][0].ComputerCode.Split('.')[1].Length - 1);
                }
                else if (procedures[procKey][0].Type == "D")
                {
                    currentComputerCode = procedures[procKey][0].ComputerCode.Split('.')[0].Substring(0, procedures[procKey][0].ComputerCode.Split('.')[0].Length - 1);
                }
                else
                {
                    throw new NotImplementedException();
                }

                aliasComand.Append($".{currentComputerCode}F .FF ");

                List<string> allFixes = new List<string>();

                individualData.AppendLine($"{currentComputerCode.PadRight(26, ' ')}N000.00.00.000 E000.00.00.000 N000.00.00.000 E000.00.00.000");

                StarAndDpModel prevPoint = null;
                foreach (StarAndDpModel point in procedures[procKey])
                {
                    if (point.PointCode == "AA")
                    {
                        newSeq = true;
                        prevPoint = null;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(point.ComputerCode))
                    {
                        newSeq = true;
                        prevPoint = point;
                    }

                    if (newSeq != true && prevPoint != null)
                    {
                        individualData.AppendLine($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");

                        if (point.Type == "S")
                        {
                            combinedDataStar.AppendLine($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");

                            allStarGeoMaps.AppendLine($"            <Element xsi:type=\"Line\" Filters=\"\" StartLat=\"{GlobalConfig.CreateDecFormat(prevPoint.Lat, true)}\" StartLon=\"{GlobalConfig.CreateDecFormat(prevPoint.Lon, true)}\" EndLat=\"{GlobalConfig.CreateDecFormat(point.Lat, true)}\" EndLon=\"{GlobalConfig.CreateDecFormat(point.Lon, true)}\" />");
                        }
                        else
                        {
                            combinedDataDp.AppendLine($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");
                            allDpGeoMaps.AppendLine($"            <Element xsi:type=\"Line\" Filters=\"\" StartLat=\"{GlobalConfig.CreateDecFormat(prevPoint.Lat, true)}\" StartLon=\"{GlobalConfig.CreateDecFormat(prevPoint.Lon, true)}\" EndLat=\"{GlobalConfig.CreateDecFormat(point.Lat, true)}\" EndLon=\"{GlobalConfig.CreateDecFormat(point.Lon, true)}\" />");


                        }

                        prevPoint = point;
                    }

                    if (!allFixes.Contains(point.PointId) && point.PointCode != "AA" && point.PointCode != "NA")
                    {
                        allFixes.Add(point.PointId);
                        aliasComand.Append($"{point.PointId} ");
                    }

                    newSeq = false;
                }




                allAliasCommands.Add(aliasComand);

                if (procedures[procKey][0].Type == "S")
                {
                    listAllIndividualStars.Add(individualData);
                }
                else if (procedures[procKey][0].Type == "D")
                {
                    listAllIndividualDp.Add(individualData);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }


            foreach (StringBuilder sbDP in listAllIndividualStars)
            {
                string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\[STAR]\\{sbDP.ToString().Substring(0, 26).Trim()}.sct2";
                if (sbDP.Length > 87)
                {
                    File.WriteAllText(filePath, sbDP.ToString());
                }
            }

            foreach (StringBuilder sbStars in listAllIndividualDp)
            {
                string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\[SID]\\{sbStars.ToString().Substring(0, 26).Trim()}.sct2";

                if (sbStars.Length > 87)
                {
                    File.WriteAllText(filePath, sbStars.ToString());
                }
            }

            string aliasFilePath = $"{GlobalConfig.outputDirectory}\\ALIAS\\STAR_DP_Fixes_Alias.txt";
            foreach (StringBuilder sbAlias in allAliasCommands)
            {
                if (sbAlias.ToString().Split('.')[2].Length > 3)
                {
                    File.AppendAllText(aliasFilePath, sbAlias.ToString() + "\n");
                    File.AppendAllText($"{GlobalConfig.outputDirectory}\\ALIAS\\AliasTestFile.txt", sbAlias.ToString() + "\n");
                }
            }

            allDpGeoMaps.AppendLine("          </Elements>");
            allDpGeoMaps.AppendLine("        </GeoMapObject>");

            allStarGeoMaps.AppendLine("          </Elements>");
            allStarGeoMaps.AppendLine("        </GeoMapObject>");

            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VERAM\\ALL_DP_GEOMAP.xml", allDpGeoMaps.ToString());
            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VERAM\\ALL_STAR_GEOMAP.xml", allStarGeoMaps.ToString());



            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VRC\\[STAR]\\000_All_STAR_Combined.sct2", combinedDataStar.ToString());
            File.WriteAllText($"{GlobalConfig.outputDirectory}\\VRC\\[SID]\\000_All_DP_Combined.sct2", combinedDataDp.ToString());

            File.AppendAllText($"{GlobalConfig.outputDirectory}{GlobalConfig.testSectorFileName}", combinedDataStar.ToString());
            File.AppendAllText($"{GlobalConfig.outputDirectory}{GlobalConfig.testSectorFileName}", combinedDataDp.ToString());

        }
    }
}
