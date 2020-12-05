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
    public class GetArbData
    {
        private List<ArbModel> allBoundaryPoints = new List<ArbModel>();
        private List<BoundryModel> allBoundarys = new List<BoundryModel>();

        private string zipFolder;
        private string unzipedFolder;


        public void ArbQuarterbacFunc(string effectiveDate) 
        {
            DownloadAptData(effectiveDate);
            ParseArb();
            WriteArbSct();
            DeleteDownloadDir();
        }

        private void DownloadAptData(string effectiveDate)
        {
            // Create Web Client to connect to FAA
            var client = new WebClient();

            // Download the APT.ZIP file from FAA
            client.DownloadFile($"https://nfdc.faa.gov/webContent/28DaySub/{effectiveDate}/ARB.zip", $"{GlobalConfig.tempPath}\\arb.zip");

            // Store our path for the zip folder we just downloaded so we can delete later
            zipFolder = $"{GlobalConfig.tempPath}\\arb.zip";

            // Unzip FAA apt.zip
            ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\arb.zip", $"{GlobalConfig.tempPath}\\arb");

            // Store our path for the unziped folder so we can delete later.
            unzipedFolder = $"{GlobalConfig.tempPath}\\arb";
        }

        private void ParseArb() 
        {
            foreach (string line in File.ReadAllLines($"{GlobalConfig.tempPath}\\arb\\ARB.txt"))
            {
                ArbModel arb = new ArbModel
                {
                    Identifier = line.Substring(0, 4).Trim(),
                    CenterName = line.Substring(12, 40).Trim(),
                    DecodeName = line.Substring(52, 10).Trim(),
                    Lat = new GlobalConfig().CorrectLatLon(line.Substring(62, 14).Trim(), true, GlobalConfig.Convert),
                    Lon = new GlobalConfig().CorrectLatLon(line.Substring(76, 14).Trim(), false, GlobalConfig.Convert),
                    Description = line.Substring(90, 300).Trim(),
                    Sequence = line.Substring(390, 6).Trim(),
                    Legal = line.Substring(396, 1).Trim()
                };
                allBoundaryPoints.Add(arb);
            }

            BoundryModel Boundary = new BoundryModel();

            int totalPoints = allBoundaryPoints.Count;
            int currentPointCount = 0;

            foreach (ArbModel point in allBoundaryPoints)
            {
                currentPointCount += 1;

                if (point.Identifier == Boundary.Identifier && point.DecodeName == Boundary.Type)
                {
                    Boundary.AllPoints.Add(point);
                }
                else
                {
                    if (Boundary.Identifier != null)
                    {
                        allBoundarys.Add(Boundary);
                    }

                    Boundary = new BoundryModel();
                    Boundary.Identifier = point.Identifier;
                    Boundary.Type = point.DecodeName;
                    Boundary.AllPoints = new List<ArbModel>();
                    Boundary.AllPoints.Add(point);
                }

                if (totalPoints == currentPointCount)
                {
                    allBoundarys.Add(Boundary);
                }
            }
        }

        private void WriteArbSct() 
        {
            // File Path to the file we want to write to
            string highFilePath = $"{GlobalConfig.outputDirectory}\\VRC\\ArtccHigh.sct2";
            string lowFilePath = $"{GlobalConfig.outputDirectory}\\VRC\\ArtccLow.sct2";


            // String Builder to store all the lines we want to write to the file.
            StringBuilder highArb = new StringBuilder();
            highArb.AppendLine("[ARTCC HIGH]");

            StringBuilder lowArb = new StringBuilder();
            lowArb.AppendLine("[ARTCC LOW]");

            foreach (BoundryModel boundry in allBoundarys)
            {
                // HIGH ARTCC =       HIGH, FIR_ONLY, UTA
                // LOW ARTCC =        LOW, CTA, BDRY

                ArbModel firstPoint = new ArbModel();
                ArbModel prevPoint = new ArbModel();
                int currentCount = 0;
                int totalCount = boundry.AllPoints.Count;

                foreach (ArbModel arbPoint in boundry.AllPoints)
                {
                    currentCount += 1;

                    if (prevPoint.Identifier == null)
                    {
                        prevPoint = arbPoint;
                        firstPoint = arbPoint;
                    }
                    else
                    {
                        if (boundry.Type == "HIGH" || boundry.Type == "FIR ONLY" || boundry.Type == "UTA")
                        {
                            highArb.AppendLine($"{boundry.Identifier} {prevPoint.Lat} {prevPoint.Lon} {arbPoint.Lat} {arbPoint.Lon}; {prevPoint.Sequence} {arbPoint.Sequence} / {prevPoint.DecodeName} {arbPoint.DecodeName}");
                        }
                        else if (boundry.Type == "LOW" || boundry.Type == "CTA" || boundry.Type == "BDRY")
                        {
                            lowArb.AppendLine($"{boundry.Identifier} {prevPoint.Lat} {prevPoint.Lon} {arbPoint.Lat} {arbPoint.Lon}; {prevPoint.Sequence} {arbPoint.Sequence} / {prevPoint.DecodeName} {arbPoint.DecodeName}");
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        prevPoint = arbPoint;

                    }

                    if (currentCount == totalCount)
                    {
                        if (boundry.Type == "HIGH" || boundry.Type == "FIR ONLY" || boundry.Type == "UTA")
                        {
                            highArb.AppendLine($"{boundry.Identifier} {prevPoint.Lat} {prevPoint.Lon} {firstPoint.Lat} {firstPoint.Lon}; {prevPoint.Sequence} {firstPoint.Sequence} / {prevPoint.DecodeName} {firstPoint.DecodeName}");
                        }
                        else if (boundry.Type == "LOW" || boundry.Type == "CTA" || boundry.Type == "BDRY")
                        {
                            lowArb.AppendLine($"{boundry.Identifier} {prevPoint.Lat} {prevPoint.Lon} {firstPoint.Lat} {firstPoint.Lon}; {prevPoint.Sequence} {firstPoint.Sequence} / {prevPoint.DecodeName} {firstPoint.DecodeName}");
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                }
            }

            // Write to the file.
            File.WriteAllText(highFilePath, highArb.ToString());
            File.WriteAllText(lowFilePath, lowArb.ToString());

            // Add some new lines to the end of the file.
            File.AppendAllText(highFilePath, $"\n\n\n\n\n\n");
            File.AppendAllText(lowFilePath, $"\n\n\n\n\n\n");

            // Add this file to our TEST SECTOR file.
            File.AppendAllText($"{GlobalConfig.outputDirectory}\\Test_Sct_File.sct2", File.ReadAllText(highFilePath));
            File.AppendAllText($"{GlobalConfig.outputDirectory}\\Test_Sct_File.sct2", File.ReadAllText(lowFilePath));
        }

        private void DeleteDownloadDir() 
        {
            // Delete our Zip folder for Airports we downloaded from FAA
            File.Delete(zipFolder);

            // Delete our unziped folder and Apt document.
            Directory.Delete(unzipedFolder, true);
        }
    }
}
