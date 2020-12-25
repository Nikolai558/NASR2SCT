using ClassData.Models.MetaFileModels;
using NASARData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ClassData.DataAccess
{
    public class GetFaaMetaFileData
    {
        // TODO - Get the Airac Cycle from Database
        public static string AiracCycle { get; private set; } = AiracDateCycleModel.AllCycleDates[GlobalConfig.airacEffectiveDate];

        public static string DownloadUrl { get; private set; } = $"https://aeronav.faa.gov/d-tpp/{AiracCycle}/xml_data/d-tpp_Metafile.xml";

        private static readonly string filePath = $"{GlobalConfig.tempPath}\\{AiracCycle}_FAA_Meta.xml";
        
        private List<MetaAirportModel> AllAirports = new List<MetaAirportModel>();

        public void QuarterbackFunc() 
        {
            if (!File.Exists(filePath))
            {
                DownloadMetaFile();
            }
            ParseMetaFile();
        }

        private static void DownloadMetaFile() 
        {
            var client = new WebClient();
            client.DownloadFile(DownloadUrl, filePath);
        }

        private void ParseMetaFile() 
        {

            string debugUrl = $"https://aeronav.faa.gov/d-tpp/2014/";

            StringBuilder aliasCommandSB = new StringBuilder();
            var xmlDoc = XDocument.Parse(File.ReadAllText(filePath));
            var airports = xmlDoc.Descendants("airport_name");

            foreach (var aptData in airports)
            {
                MetaAirportModel apt = new MetaAirportModel() 
                {
                    AirportName = aptData.Attribute("ID").Value,
                    Military = aptData.Attribute("military").Value,
                    AptIdent = aptData.Attribute("apt_ident").Value,
                    Icao = aptData.Attribute("icao_ident").Value,
                    Alnum = aptData.Attribute("alnum").Value
                };

                MetaRecordModel prevRecord = new MetaRecordModel();
                int count = 1;
                foreach (var recordData in aptData.Elements())
                {
                    MetaRecordModel record = new MetaRecordModel() 
                    {
                        ChartSeq = recordData.Element("chartseq").Value,
                        ChartCode = recordData.Element("chart_code").Value,
                        ChartName = recordData.Element("chart_name").Value,
                        UserAction = recordData.Element("useraction").Value,
                        PdfName = recordData.Element("pdf_name").Value,
                        CnFlag = recordData.Element("cn_flg").Value,
                        CnPage = recordData.Element("cnpage").Value,
                        CnSection = recordData.Element("cnsection").Value,
                        BvSection = recordData.Element("bvsection").Value,
                        BvPage = recordData.Element("bvpage").Value,
                        ProcUid = recordData.Element("procuid").Value,
                        TwoColored = recordData.Element("two_colored").Value,
                        Civil = recordData.Element("civil").Value,
                        Faanfd18 = recordData.Element("faanfd18").Value,
                        Copter = recordData.Element("copter").Value,
                        AmdtNum = recordData.Element("amdtnum").Value,
                        AmdtDate = recordData.Element("amdtdate").Value
                    };
                    record.CreateAliasComand();
                    apt.Records.Add(record);

                    if (
                            record.ChartName.IndexOf("CONVERGING") == -1 &&
                            record.ChartName.IndexOf("COPTER") == -1 &&
                            record.ChartName.IndexOf("HI-") == -1 &&
                            record.ChartName.IndexOf("PRM") == -1 &&
                            record.ChartName.IndexOf("PRM") == -1 &&
                            record.ChartName.IndexOf("BC") == -1 &&
                            record.ChartName.IndexOf("CAT ") == -1 &&
                            record.ChartName.IndexOf("TACAN 056") == -1)
                    {

                        if (prevRecord.ProcUid == record.ProcUid && prevRecord.AliasCommand == record.AliasCommand)
                        {
                            count += 1;
                            if (record.AliasCommand.IndexOf('/') == -1)
                            {
                                aliasCommandSB.AppendLine($".{apt.AptIdent}{record.AliasCommand}{count}C .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                            }
                            else
                            {
                                foreach (string str in record.AliasCommand.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(str))
                                    {
                                        continue;
                                    }

                                    if (str == "*")
                                    {
                                        aliasCommandSB.AppendLine($".{apt.AptIdent}APDC .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                    else if (record.ChartCode == "TM" || record.ChartCode == "DVA")
                                    {
                                        aliasCommandSB.AppendLine($".{apt.AptIdent}{str}C .OPENURL {debugUrl}{record.PdfName}#nameddest=({apt.AptIdent})  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                    else if (record.ChartCode == "STAR" || record.ChartCode == "DP" || record.ChartCode == "ODP" && !string.IsNullOrEmpty(str))
                                    {
                                        aliasCommandSB.AppendLine($".{str}{count}C .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                    else if (!string.IsNullOrEmpty(str))
                                    {
                                        aliasCommandSB.AppendLine($".{apt.AptIdent}{str}{count}C .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            count = 1;
                            if (record.AliasCommand.IndexOf('/') == -1)
                            {
                                aliasCommandSB.AppendLine($".{apt.AptIdent}{record.AliasCommand}C .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                            }
                            else
                            {
                                foreach (string str in record.AliasCommand.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(str))
                                    {
                                        continue;
                                    }

                                    if (str == "*")
                                    {
                                        aliasCommandSB.AppendLine($".{apt.AptIdent}APDC .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                    else if (record.ChartCode == "MIN")
                                    {
                                        aliasCommandSB.AppendLine($".{apt.AptIdent}{str}C .OPENURL {debugUrl}{record.PdfName}#nameddest=({apt.AptIdent})  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                    else if (record.ChartCode == "STAR" || record.ChartCode == "DP" || record.ChartCode == "ODP" && !string.IsNullOrEmpty(str))
                                    {
                                        aliasCommandSB.AppendLine($".{str}C .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                    else if (!string.IsNullOrEmpty(str))
                                    {
                                        aliasCommandSB.AppendLine($".{apt.AptIdent}{str}C .OPENURL {debugUrl}{record.PdfName}  ; {apt.AirportName}-{record.ChartName}");
                                    }
                                }
                            }
                        }
                        prevRecord = record;
                    }
                }

                AllAirports.Add(apt);
            }

            File.WriteAllText($"{GlobalConfig.outputDirectory}\\ALIAS\\FAA_CHART_RECALL.txt", aliasCommandSB.ToString());
        }
    }
}
