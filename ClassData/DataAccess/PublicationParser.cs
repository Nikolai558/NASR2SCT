using ClassData.Models;
using NASARData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.DataAccess
{
    public class PublicationParser
    {
        public static List<string> allArtcc = new List<string>() { 
            "ZAP", "ZAN", "ZJX", "ZME", "ZTL", "ZHU", "ZID", "ZFW", "ZKC", "ZHN", "ZAB",
            "ZLA", "ZDV", "ZSE", "ZOA", "ZUA", "ZBW", "ZNY", "ZDC", "ZMA", "ZAU", "ZMP", "ZLC", "ZOB",
            "ZYZ", "ZSU", "ZVR", "ZEG", "FIM", "SBA", "ZAK", "ZUL", "ZWG"
        };

        private string outputDirectory = GlobalConfig.outputDirectory + "Publications";

        public void WriteAirportInfoTxt(string responsibleArtcc) 
        {
            StringBuilder airportInArtccInfo = new StringBuilder();
            StringBuilder airportProcedureChanges = new StringBuilder();
            StringBuilder airportProcedures = new StringBuilder();

            if (responsibleArtcc == "FAA")
            {
                List<AptModel> dupAirportModel = GlobalConfig.allAptModelsForCheck;
                foreach (string artcc in allArtcc)
                {
                    airportInArtccInfo = new StringBuilder();
                    airportProcedureChanges = new StringBuilder();
                    airportProcedures = new StringBuilder();
                    foreach (AptModel airport in dupAirportModel)
                    {
                        if (airport.ResArtcc == artcc)
                        {
                            string airportInfo = $"{airport.Name}{' '*(25-airport.Name.Length)}: {airport.Id} - {airport.Icao}";
                            airportInArtccInfo.AppendLine(airportInfo);


                        }
                    }
                    string airportsInArtccFilePath = $"{outputDirectory}\\{GlobalConfig.airacEffectiveDate}_{artcc}\\Res_Artcc_Airports.txt";
                    CreateDirAndFile(airportsInArtccFilePath);
                    File.WriteAllText(airportsInArtccFilePath, airportInArtccInfo.ToString());
                }
            }
            else if (allArtcc.Contains(responsibleArtcc))
            {
                foreach (AptModel airport in GlobalConfig.allAptModelsForCheck)
                {
                    if (airport.ResArtcc == responsibleArtcc)
                    {
                        string output = $"{airport.Id} - {airport.Icao}";
                        airportInArtccInfo.AppendLine(output);
                    }
                }
                string airportsInArtccFilePath = $"{outputDirectory}\\{GlobalConfig.airacEffectiveDate}_{responsibleArtcc}\\Res_Artcc_Airports.txt";
                CreateDirAndFile(airportsInArtccFilePath);
                File.WriteAllText(airportsInArtccFilePath, airportInArtccInfo.ToString());

            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Create the Directories and File for the full file path put in.
        /// </summary>
        /// <param name="fullFilePath">Full File Path</param>
        private void CreateDirAndFile(string fullFilePath) 
        {
            if (!Directory.Exists(fullFilePath.Substring(0, fullFilePath.LastIndexOf('\\'))))
            {
                Directory.CreateDirectory(fullFilePath.Substring(0, fullFilePath.LastIndexOf('\\')));
            }

            if (!File.Exists(fullFilePath))
            {
                //File.Create(fullFilePath);
            }
        }
    }
}
