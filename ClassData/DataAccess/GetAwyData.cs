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
    public class GetAwyData
    {
        List<AwyPointModel> allAWYPoints = new List<AwyPointModel>();
        List<AirwayModel> allAwy = new List<AirwayModel>();

        private string zipFolder;
        private string unzipedFolder;

        public void AWYQuarterbackFunc(string effectiveDate) 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "AWYQuarterbackFunc()")}: {DateTime.Now}");
            DownloadAwyData(effectiveDate);
            ParseAwyData();
            WriteAwySctData();
            deleteUnneededDir();
        }

        private void DownloadAwyData(string effectiveDate)
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadAwyData()")}: {DateTime.Now}");

            var client = new WebClient();

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadAwyData().DOWNLOADING")}: {DateTime.Now}");

            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/AWY.zip", $"{GlobalConfig.tempPath}\\awy.zip");
            zipFolder = $"{GlobalConfig.tempPath}\\awy.zip";

            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "DownloadAwyData().UNZIPING")}: {DateTime.Now}");

            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\awy.zip", $"{GlobalConfig.tempPath}\\awy");
            unzipedFolder = $"{GlobalConfig.tempPath}\\awy";
        }

        private void ParseAwyData() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "ParseAwyData()")}: {DateTime.Now}");

            AwyPointModel awyPoint = new AwyPointModel();

            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\awy\\AWY.txt")) 
            {
                if (line.Substring(0, 4) == "AWY1")
                {
                    awyPoint = new AwyPointModel();

                    if (line.Substring(106, 1) == "X")
                    {
                        awyPoint.GapAfter = true;
                    }

                    if (line.Substring(144, 1) == " ")
                    {
                        awyPoint.EndOfAirway = true;
                    }

                    //if (line.Substring(144, 6) == "BORDER")
                    if (line.IndexOf("BORDER", 144, 33) != -1)
                    {
                        awyPoint.BorderAfter = true;
                    }
                }
                else if (line.Substring(0, 4) == "AWY2")
                {
                    awyPoint.AirwayId = line.Substring(4, 5).Trim();
                    awyPoint.Name = line.Substring(15, 30).Trim();

                    if (line.Substring(116, 4).Trim() == string.Empty)
                    {
                        awyPoint.PointId = awyPoint.Name;
                    }
                    else
                    {
                        awyPoint.PointId = line.Substring(116, 4).Trim();
                    }
                    awyPoint.Type = line.Substring(45, 19).Trim();
                    

                    if (awyPoint.Name.IndexOf("BORDER",0 , awyPoint.Name.Length) == -1)
                    {
                        awyPoint.Lat = new GlobalConfig().CorrectLatLon(line.Substring(83, 14).Trim(), true, GlobalConfig.Convert);
                        awyPoint.Lon = new GlobalConfig().CorrectLatLon(line.Substring(97, 14).Trim(), false, GlobalConfig.Convert);

                        awyPoint.Dec_Lat = new GlobalConfig().createDecFormat(awyPoint.Lat);
                        awyPoint.Dec_Lon = new GlobalConfig().createDecFormat(awyPoint.Lon);


                        allAWYPoints.Add(awyPoint);
                    }
                }
            }

            AirwayModel awy = new AirwayModel();

            int totalPoints = allAWYPoints.Count;
            int currentPointCount = 0;
            foreach (AwyPointModel point in allAWYPoints)
            {
                currentPointCount += 1;

                if (point.AirwayId == awy.Id)
                {
                    awy.AwyPoints.Add(point);

                }
                else
                {
                    if (awy.Id != null)
                    {
                        allAwy.Add(awy);
                    }

                    awy = new AirwayModel();
                    awy.Id = point.AirwayId;
                    awy.AwyPoints = new List<AwyPointModel>();
                    awy.AwyPoints.Add(point);
                }

                if (totalPoints == currentPointCount)
                {
                    allAwy.Add(awy);
                }
            }
        }

        private void WriteAwySctData() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "WriteAwySctData()")}: {DateTime.Now}");

            string filePath = $"{GlobalConfig.outputDirectory}\\VRC\\AWY.sct2";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[HIGH AIRWAY]");

            foreach (AirwayModel airway in allAwy)
            {
                AwyPointModel prevPoint = new AwyPointModel();

                foreach (AwyPointModel point in airway.AwyPoints)
                {
                    // airway.AwyPoints[count].BorderAfter || airway.AwyPoints[count].GapAfter

                    if (prevPoint.AirwayId == null)
                    {
                        prevPoint = point;
                    }
                    else
                    {
                        sb.AppendLine($"{airway.Id.PadRight(27)}{prevPoint.Lat} {prevPoint.Lon} {point.Lat} {point.Lon}; {prevPoint.PointId.PadRight(5)} {point.PointId.PadRight(5)}");

                        if (point.GapAfter)
                        {
                            prevPoint = new AwyPointModel();
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
        }

        private void deleteUnneededDir() 
        {
            GlobalConfig.timetracker.AppendLine($"{string.Format("{0,-37}", "deleteUnneededDir()")}: {DateTime.Now}");

            File.Delete(zipFolder);

            Directory.Delete(unzipedFolder, true);
        }
    }
}
