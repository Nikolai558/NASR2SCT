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
    public class GetStarDpData
    {
        List<StarAndDpModel> allProcedures = new List<StarAndDpModel>();
        Dictionary<string, List<string>> procedureLines = new Dictionary<string, List<string>>();
        Dictionary<string, List<StarAndDpModel>> procedures = new Dictionary<string, List<StarAndDpModel>>();



        public void StarDpQuaterBackFunc(string EffectiveDate) 
        {
            DownloadStarDp(EffectiveDate);
            ParseStarDp();
            WriteSctData();
        }

        private void DownloadStarDp(string effectiveDate) 
        {
            // Web Client used to connect to the FAA website.
            var client = new WebClient();

            // Download the Fix Data
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/STARDP.zip", $"{GlobalConfig.tempPath}\\stardp.zip");

            // Extract the ZIP file that we just downloaded.
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\stardp.zip", $"{GlobalConfig.tempPath}\\stardp");

        }

        private void ParseStarDp()
        {
            

            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\stardp\\STARDP.txt"))
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

            // TODO - This is wrong fix it in a second
            //sb.AppendLine("[SID]");
            //sb.AppendLine("[STAR]");

            StringBuilder individualData;
            StringBuilder aliasComand;

            GlobalConfig Gb = new GlobalConfig();

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

                if ($".{currentComputerCode}F .FF " == ".YELMF .FF ")
                {
                    string pauseme = aliasComand.ToString();
                }

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

                            allStarGeoMaps.AppendLine($"            <Element xsi:type=\"Line\" Filters=\"\" StartLat=\"{Gb.createDecFormat(prevPoint.Lat, true)}\" StartLon=\"{Gb.createDecFormat(prevPoint.Lon, true)}\" EndLat=\"{Gb.createDecFormat(point.Lat, true)}\" EndLon=\"{Gb.createDecFormat(point.Lon, true)}\" />");
                        }
                        else
                        {
                            combinedDataDp.AppendLine($"                          {prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId} {point.PointId}");
                            allDpGeoMaps.AppendLine($"            <Element xsi:type=\"Line\" Filters=\"\" StartLat=\"{Gb.createDecFormat(prevPoint.Lat, true)}\" StartLon=\"{Gb.createDecFormat(prevPoint.Lon, true)}\" EndLat=\"{Gb.createDecFormat(point.Lat, true)}\" EndLon=\"{Gb.createDecFormat(point.Lon, true)}\" />");


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
