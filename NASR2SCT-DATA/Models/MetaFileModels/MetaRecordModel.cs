﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassData.Models.MetaFileModels
{
    public class MetaRecordModel
    {
        public string FAAChartName { get; set; }

        public string ChartSeq { get; set; }

        public string ChartCode { get; set; }

        public string ChartName { get; set; }

        public string UserAction { get; set; }

        public string PdfName { get; set; }

        public string CnFlag { get; set; }

        public string CnSection { get; set; }

        public string CnPage { get; set; }

        public string BvSection { get; set; }

        public string BvPage { get; set; }

        public string ProcUid { get; set; }

        public string TwoColored { get; set; }

        public string Civil { get; set; }

        public string Faanfd18 { get; set; }

        public string Copter { get; set; }

        public string AmdtNum { get; set; }

        public string AmdtDate { get; set; }

        public bool HasMultiplePages { get; set; } = false;

        public int PageCount { get; set; } = 1;

        public string Variant { get; set; }

        public string AliasCommand { get; private set; }

        public void CreateAliasComand(string AptIata) 
        {
            if (ChartCode == "MIN")
            {
                if (ChartName == "TAKEOFF MINIMUMS")
                {
                    AliasCommand +=  "/TM";
                }
                else if (ChartName == "ALTERNATE MINIMUMS")
                {
                    AliasCommand +=  "/";
                    //AliasCommand +=  "/AM";
                }
                else if (ChartName == "DIVERSE VECTOR AREA")
                {
                    AliasCommand +=  "/DVA";
                }
                else if (ChartName == "RADAR MINIMUMS")
                {
                    AliasCommand +=  "/RM";
                }
                else
                {
                    // Only get here if the FAA added another "Type" to the MIN category.
                    AliasCommand +=  "/NEWMINTYPEERROR";
                }
            }
            else if (ChartCode == "IAP")
            {
                if (ChartName.IndexOf(@" OR ") != -1)
                {
                    string runwayTempVar;
                    List<MetaRecordModel> tempRecordList = new List<MetaRecordModel>();

                    if (ChartName.IndexOf("RWY") == -1)
                    {
                        runwayTempVar = "";
                    }
                    else
                    {
                        runwayTempVar = ChartName.Substring(ChartName.IndexOf("RWY"));
                    }

                    foreach (string individualChartName in ChartName.Split(new string[] { @" OR " }, StringSplitOptions.None))
                    {
                        MetaRecordModel tempRecordModel = new MetaRecordModel();
                        tempRecordModel.ChartCode = ChartCode;
                        tempRecordModel.PdfName = PdfName;
                        tempRecordModel.FAAChartName = FAAChartName;


                        tempRecordModel.ChartName = individualChartName;

                        if (tempRecordModel.ChartName.IndexOf("RWY") == -1)
                        {
                            tempRecordModel.ChartName += " " + runwayTempVar;
                        }
                        
                        tempRecordModel.CreateAliasComand(AptIata);

                        tempRecordList.Add(tempRecordModel);
                        //AliasCommand += tempRecordModel.AliasCommand;
                    }

                    List<int> indexesMissingVariant = new List<int>();
                    int count = 0;
                    string tempVariant = "";
                    foreach (MetaRecordModel tempRcord in tempRecordList)
                    {
                        if (string.IsNullOrEmpty(tempRcord.Variant))
                        {
                            indexesMissingVariant.Add(count);
                        }
                        else
                        {
                            tempVariant = tempRcord.Variant;
                        }
                        count += 1;
                    }

                    if (indexesMissingVariant.Count >= 1 && indexesMissingVariant.Count != tempRecordList.Count)
                    {
                        foreach (int missingIndex in indexesMissingVariant)
                        {
                            if (char.IsDigit(tempRecordList[missingIndex].AliasCommand[tempRecordList[missingIndex].AliasCommand.Length - 1]) &&
                                char.IsDigit(tempRecordList[missingIndex].AliasCommand[tempRecordList[missingIndex].AliasCommand.Length - 2]))
                            {
                                string firstCommandPart = tempRecordList[missingIndex].AliasCommand.Substring(0, 2);
                                string middleCommandPart = tempVariant;
                                string endCommandPart = tempRecordList[missingIndex].AliasCommand.Substring(tempRecordList[missingIndex].AliasCommand.Length - 1);

                                tempRecordList[missingIndex].AliasCommand = firstCommandPart + middleCommandPart + endCommandPart;
                            }
                            else
                            {
                                tempRecordList[missingIndex].AliasCommand = tempRecordList[missingIndex].AliasCommand.Insert(2, tempVariant);
                            }

                            // Might want to remove this - This is to see MisMatchingVaarents inside the Temp File.
                            // File.AppendAllText($"{NASARData.GlobalConfig.tempPath}\\MisMatchingVariants.txt", $"APT IATA: {AptIata} - {tempRecordList[missingIndex].FAAChartName}\n");
                        }
                    }
                    
                    foreach (MetaRecordModel tempRecord in tempRecordList)
                    {
                        AliasCommand += tempRecord.AliasCommand;
                    }
                }
                else if (PdfName.IndexOf("_VIS") != -1)
                {
                    string output = "/V";

                    foreach (string str in ChartName.Substring(0, ChartName.IndexOf("VISUAL")).Split(' '))
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            output += str[0];
                        }
                    }

                    AliasCommand += output;
                }
                else if (ChartName.IndexOf("ILS ") != -1 || ChartName.IndexOf("ILS-") != -1)
                {
                    string output = CreateAliasCommandHelper("/I");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("LOC ") != -1 || ChartName.IndexOf("LOC-") != -1)
                {
                    string output = CreateAliasCommandHelper("/L");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("LDA ") != -1 || ChartName.IndexOf("LDA-") != -1)
                {
                    string output = CreateAliasCommandHelper("/D");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("LDA/DME") != -1)
                {
                    string output = CreateAliasCommandHelper("/A");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("GPS ") != -1 || ChartName.IndexOf("GPS-") != -1)
                {
                    string output = CreateAliasCommandHelper("/G");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("LOC/DME ") != -1 || ChartName.IndexOf("LOC/DME-") != -1)
                {
                    string output = CreateAliasCommandHelper("/K");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("LOC/NDB ") != -1)
                {
                    // Do not account for LOC/NDB (Only 2 in entire USA)
                    AliasCommand += "/";
                }
                else if (ChartName.IndexOf("NDB ") != -1 || ChartName.IndexOf("NDB-") != -1)
                {
                    string output = CreateAliasCommandHelper("/N");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("RNAV (GPS) ") != -1 || ChartName.IndexOf("RNAV (GPS)-") != -1 || ChartName.IndexOf("RNAV (RNP) ") != -1)
                {
                    string oldChartName = ChartName;

                    if (ChartName.IndexOf("(GPS)") != -1)
                    {
                        ChartName = ChartName.Replace(" (GPS)", string.Empty);
                    }
                    
                    if (ChartName.IndexOf("(RNP)") != -1)
                    {
                        ChartName = ChartName.Replace(" (RNP)", string.Empty);
                    }

                    string output = CreateAliasCommandHelper("/R");

                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }

                    //ChartName = oldChartName;
                }
                else if (ChartName.IndexOf("SDF ") != -1)
                {
                    string output = CreateAliasCommandHelper("/S");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("TACAN ") != -1 || ChartName.IndexOf("TACAN-") != -1)
                {
                    string output = CreateAliasCommandHelper("/T");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("VOR ") != -1 || ChartName.IndexOf("VOR-") != -1)
                {
                    string output = CreateAliasCommandHelper("/O");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("VOR/DME ") != -1 || ChartName.IndexOf("VOR/DME-") != -1)
                {
                    string output = CreateAliasCommandHelper("/F");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("NDB/DME ") != -1 || ChartName.IndexOf("NDB/DME-") != -1)
                {
                    string output = CreateAliasCommandHelper("/B");
                    if (output.IndexOf("!DONT-INCLUDE!") == -1)
                    {
                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("GLS ") != -1)
                {
                    // Do not account for GLS
                    AliasCommand += "/";
                }
                else
                {
                    AliasCommand += "/ERROR";
                }
            }
            else if (ChartCode == "DP")
            {
                if (!string.IsNullOrEmpty(Faanfd18))
                {
                    AliasCommand += $"/{AptIata}{Faanfd18.Split('.')[0].Substring(0, Faanfd18.Split('.')[0].Length - 1)}";
                }
                else
                {
                    // The DP Does not have a Computer Code.
                    AliasCommand += "/";
                }
            }
            else if (ChartCode == "ODP")
            {
                if (!string.IsNullOrEmpty(Faanfd18))
                {
                    AliasCommand += $"/{AptIata}{Faanfd18.Split('.')[0].Substring(0, Faanfd18.Split('.')[0].Length - 1)}";
                }
                else
                {
                    // The ODP does not have a Computer Code.
                    AliasCommand += "/";
                }
            }
            else if (ChartCode == "HOT")
            {
                AliasCommand += "/HS";
            }
            else if (ChartCode == "STAR")
            {
                if (!string.IsNullOrEmpty(Faanfd18))
                {
                    AliasCommand += $"/{AptIata}{Faanfd18.Split('.')[1].Substring(0, Faanfd18.Split('.')[1].Length - 1)}";
                }
                else
                {
                    // The Star does not have a Computer Code.
                    AliasCommand += "/";
                }
            }
            else if (ChartCode == "APD")
            {
                AliasCommand += "/*";
            }
            else if (ChartCode == "LAH")
            {
                AliasCommand += "/LAHSO";
            }
            else if (ChartCode == "DAU")
            {
                //AliasCommand += "/DAU";
                AliasCommand += "/";
            }
            else
            {
                AliasCommand += "/GENERALERROR";
            }
        }

        private string CreateAliasCommandHelper(string aproachTypeCode) 
        {
            string output;
            bool getTwoDigitRwy;

            if (ChartName.IndexOf("COPTER") != -1 || ChartName.IndexOf("HI-") != -1)
            {
                output = aproachTypeCode;

                // Chart name has COPTER or HI- included in it. WE DO NOT WANT THIS CHART.
                output += "!DONT-INCLUDE!";
                return output;
            }

            if (ChartName.Contains("CONT."))
            {
                HasMultiplePages = true;
                PageCount += 1;

                ChartName = ChartName.Replace($"{ChartName.Substring(ChartName.IndexOf(", C"))}", string.Empty);
            }

            if (ChartName.IndexOf("RWY") == -1)
            {
                output = aproachTypeCode;
                // Chartname does NOT have any runways. So Just return the VARIANT (if it has one)
                if (ChartName.IndexOf("-") != -1)
                {
                    // Chartname has a -VARIANT
                    Variant = ChartName.Split('-')[1];
                    output += ChartName.Split('-')[1];

                    if (HasMultiplePages)
                    {
                        output += $"{PageCount}";
                    }

                    return output;
                }
                else if (ChartName.Split(' ').Count() >= 2)
                {
                    // Chartname has a ' VARIANT'
                    Variant = ChartName.Split(' ')[1];
                    output += ChartName.Split(' ')[1];

                    if (HasMultiplePages)
                    {
                        output += $"{PageCount}";
                    }

                    return output;
                }
                else
                {
                    // Chartname has no VARIANT

                    if (HasMultiplePages)
                    {
                        output += $"{PageCount}";
                    }

                    return output;
                }
            }

            output = aproachTypeCode;

            if (ChartName.IndexOf("-") != -1)
            {
                getTwoDigitRwy = false;
                // Chartname has '-' so it HAS a variant, ALWAYS.
                // add the varrient to output.
                Variant = ChartName.Split('-')[1][0].ToString();
                output += ChartName.Split('-')[1][0];
            }
            else if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() > 2)
            {
                getTwoDigitRwy = false;
                // Chartname HAS variant
                Variant = ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];
                output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];
            }
            else
            {
                // Chartname does NOT have a variant
                getTwoDigitRwy = true;
            }

            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
            {
                // Chart Runway does not have a designator.
                if (getTwoDigitRwy)
                {
                    output += ChartName.Substring(ChartName.Length - 2);
                }
                else
                {
                    output += ChartName.Substring(ChartName.Length - 1);
                }

                if (HasMultiplePages)
                {
                    output += $"{PageCount}";
                }

                return output;
            }
            else
            {
                // Chart Runway DOES have designator, need to check to see if it has multiple.
                if (ChartName.Substring(ChartName.IndexOf("RWY")).IndexOf("/") == -1)
                {
                    // Chart RWY does NOT have multiple designators.
                    output += ChartName.Substring(ChartName.Length - 2);

                    if (HasMultiplePages)
                    {
                        output += $"{PageCount}";
                    }

                    return output;
                }
                else
                {
                    // Chart Runway has MULTIPLE designators

                    // RWY 30L/R/C

                    string tempRwyNumber = ChartName.Substring(ChartName.IndexOf("RWY")).Trim().Split('/')[0].Substring(4, 2);
                    int tempCount = 0;
                    string tempOutput = output;

                    foreach (string designator in ChartName.Substring(ChartName.IndexOf("RWY")).Split('/'))
                    {
                        if (tempCount > 0)
                        {
                            // this is 2nd and up index add entire alias command, last digit of the runway, and the designator
                            output += tempOutput;
                            output += tempRwyNumber[1];
                            output += designator;
                        }
                        else
                        {
                            // this is the first index of the rwy, grab last two characters (i.e. 6R)
                            output += designator.Substring(designator.Length - 2);
                        }

                        if (HasMultiplePages)
                        {
                            output += $"{PageCount}";
                        }

                        tempCount += 1;
                    }

                    return output;
                }
            }
        }

        public void ORIGINAL___CreateAliasComand(string AptIata)
        {

            if (ChartCode == "MIN")
            {
                if (ChartName == "TAKEOFF MINIMUMS")
                {
                    AliasCommand += "/TM";
                }
                else if (ChartName == "ALTERNATE MINIMUMS")
                {
                    AliasCommand += "/";
                    //AliasCommand +=  "/AM";
                }
                else if (ChartName == "DIVERSE VECTOR AREA")
                {
                    AliasCommand += "/DVA";
                }
                else if (ChartName == "RADAR MINIMUMS")
                {
                    AliasCommand += "/RM";
                }
                else
                {
                    // Only get here if the FAA added another "Type" to the MIN category.
                    AliasCommand += "/NEWMINTYPEERROR";
                }
            }
            else if (ChartCode == "IAP")
            {
                if (ChartName.IndexOf(@" OR ") != -1)
                {

                    string runwayTempVar;

                    if (ChartName.IndexOf("RWY") == -1)
                    {
                        runwayTempVar = "";
                    }
                    else
                    {
                        runwayTempVar = ChartName.Substring(ChartName.IndexOf("RWY"));
                    }

                    foreach (string individualChartName in ChartName.Split(new string[] { @" OR " }, StringSplitOptions.None))
                    {
                        MetaRecordModel tempRecordModel = new MetaRecordModel();
                        tempRecordModel.ChartCode = ChartCode;
                        tempRecordModel.PdfName = PdfName;


                        tempRecordModel.ChartName = individualChartName;

                        if (tempRecordModel.ChartName.IndexOf("RWY") == -1)
                        {
                            tempRecordModel.ChartName += " " + runwayTempVar;
                        }

                        tempRecordModel.CreateAliasComand(AptIata);
                        AliasCommand += tempRecordModel.AliasCommand;
                    }
                }
                else if (PdfName.IndexOf("_VIS") != -1)
                {
                    string output = "/V";

                    foreach (string str in ChartName.Substring(0, ChartName.IndexOf("VISUAL")).Split(' '))
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            output += str[0];
                        }
                    }

                    AliasCommand += output;
                }
                else if (ChartName.IndexOf("ILS ") != -1 || ChartName.IndexOf("ILS-") != -1)
                {
                    string output = "/I";

                    if (ChartName.IndexOf("ILS ") != -1)
                    {
                        // ILS VARIANT

                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            // ILS HAS VARIANT IN FORM OF SPACE (ILS Y RWY 03)

                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                // RUNWAY JUST A DIGIT, NO PARALLEL DESIGNATER
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                // RUNWAY HAS PARALLEL DESIGNATOR
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    // RUNWAY HAS MULTIPLE DESIGNATORS
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            // Just in case we need this. The designator should never be unassigned. 
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            // First Designator, Has runway last digit included
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            // Next Designator, Need to Copy runway Last Digit.
                                            output += $"/I{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            // ILS DOES NOT HAVE VARIANT
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                // RUNWAY JUST A DIGIT, NO PARALLEL DESIGNATER
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                // RUNWAY HAS PARALLEL DESIGNATOR
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    // RUNWAY HAS MULTIPLE DESIGNATORS
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            // Just in case we need this. The designator should never be unassigned. 
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            // First Designator, Has runway last digit included
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            // Next Designator, Need to Copy runway Last Digit.
                                            output += $"/I{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    // RUNWAY ONLY HAS ONE DESIGNATOR
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        // ILS-VARIANT
                        // ILS-Y RWY 10
                        output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split('-')[1];

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            // ILS-Y RWY 10R
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                // RUNWAY HAS MULTIPLE DESIGNATORS
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        // Just in case we need this. The designator should never be unassigned. 
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        // First Designator, Has runway last digit included
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        // Next Designator, Need to Copy runway Last Digit.
                                        output += $"/I{rwyLastDigit}{designator}";
                                    }

                                }

                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("LOC ") != -1 || ChartName.IndexOf("LOC-") != -1)
                {
                    string output = "/L";

                    if (ChartName.IndexOf("LOC ") != -1)
                    {
                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/L{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2, 2);
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/L{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/L{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("LDA ") != -1 || ChartName.IndexOf("LDA-") != -1)
                {
                    string output = "/D";

                    if (ChartName.IndexOf("LDA ") != -1)
                    {
                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/D{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/D{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/D{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("LDA/DME") != -1)
                {
                    string output = "/A";

                    if (ChartName.IndexOf("RWY") == -1)
                    {
                        output += ChartName.Split('-')[1];
                        AliasCommand += output;
                        return;
                    }

                    if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                    {
                        output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/A{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                        }

                        AliasCommand += output;
                    }
                    else
                    {
                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/A{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                        }

                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("GPS ") != -1 || ChartName.IndexOf("GPS-") != -1)
                {
                    string output = "/G";

                    if (ChartName.IndexOf("GPS ") != -1)
                    {
                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/G{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/G{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/G{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("LOC/DME ") != -1 || ChartName.IndexOf("LOC/DME-") != -1)
                {
                    string output = "/K";

                    if (ChartName.IndexOf("LOC/DME ") != -1)
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/K{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/K{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/K{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }

                }
                else if (ChartName.IndexOf("LOC/NDB ") != -1)
                {
                    AliasCommand += "/";

                }
                else if (ChartName.IndexOf("NDB ") != -1 || ChartName.IndexOf("NDB-") != -1)
                {
                    string output = "/N";

                    if (ChartName.IndexOf("COPTER") != -1)
                    {
                        return;
                    }

                    if (ChartName.IndexOf("NDB ") != -1)
                    {
                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/N{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/N{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/N{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("RNAV (GPS) ") != -1 || ChartName.IndexOf("RNAV (GPS)-") != -1 || ChartName.IndexOf("RNAV (RNP) ") != -1)
                {
                    string oldChartName = ChartName;

                    if (ChartName.IndexOf("(GPS)") != -1)
                    {
                        ChartName = ChartName.Replace(" (GPS)", string.Empty);
                    }

                    if (ChartName.IndexOf("(RNP)") != -1)
                    {
                        ChartName = ChartName.Replace(" (RNP)", string.Empty);
                    }


                    string output = "/R";
                    if (ChartName.IndexOf("COPTER") != -1)
                    {
                        output = "/";
                        AliasCommand += output;
                        return;
                    }


                    if (ChartName.IndexOf("RWY") == -1)
                    {
                        output += ChartName.Split('-')[1];
                        AliasCommand += output;
                        return;
                    }

                    if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                    {
                        output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/L{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2, 2);
                            }
                        }

                        AliasCommand += output;
                    }
                    else
                    {
                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/L{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                        }

                        AliasCommand += output;
                    }

                    ChartName = oldChartName;
                }
                else if (ChartName.IndexOf("SDF ") != -1)
                {
                    string output = "/S";

                    if (ChartName.IndexOf("RWY") == -1)
                    {
                        output += ChartName.Split('-')[1];
                        AliasCommand += output;
                        return;
                    }

                    if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                    {
                        output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/S{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                        }

                        AliasCommand += output;
                    }
                    else
                    {
                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/S{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                        }

                        AliasCommand += output;
                    }
                }
                else if (ChartName.IndexOf("TACAN ") != -1 || ChartName.IndexOf("TACAN-") != -1)
                {
                    string output = "/T";

                    if (ChartName.IndexOf("TACAN ") != -1)
                    {
                        if (ChartName.IndexOf("HI-") != -1 || ChartName.IndexOf("COPTER") != -1)
                        {
                            return;
                        }

                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/T{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/T{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/T{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("VOR ") != -1 || ChartName.IndexOf("VOR-") != -1)
                {
                    string output = "/O";

                    if (ChartName.IndexOf("VOR ") != -1)
                    {

                        if (ChartName.IndexOf("COPTER") != -1)
                        {
                            return;
                        }


                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/O{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/O{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/O{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("VOR/DME ") != -1 || ChartName.IndexOf("VOR/DME-") != -1)
                {
                    string output = "/F";

                    if (ChartName.IndexOf("VOR/DME ") != -1)
                    {
                        if (ChartName.IndexOf("COPTER") != -1)
                        {
                            return;
                        }

                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/F{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/F{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/F{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }

                }
                else if (ChartName.IndexOf("NDB/DME ") != -1 || ChartName.IndexOf("NDB/DME-") != -1)
                {
                    string output = "/B";

                    if (ChartName.IndexOf("NDB/DME ") != -1)
                    {
                        if (ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ').Count() == 3)
                        {
                            output += ChartName.Substring(0, ChartName.IndexOf("RWY")).Split(' ')[1];

                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1][1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/B{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                            }

                            AliasCommand += output;
                        }
                        else
                        {
                            if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                            {
                                output += ChartName.Substring(ChartName.IndexOf("RWY")).Split(' ')[1];
                            }
                            else
                            {
                                string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                                int tempCount = 0;
                                if (tempVar.Split('/').Count() > 1)
                                {
                                    string rwyLastDigit = "";
                                    foreach (string designator in tempVar.Split('/'))
                                    {
                                        if (string.IsNullOrEmpty(designator))
                                        {
                                            continue;
                                        }

                                        if (tempCount == 0)
                                        {
                                            output += designator.Substring(designator.Length - 2);
                                            rwyLastDigit = designator[designator.Length - 2].ToString();
                                            tempCount += 1;
                                        }
                                        else
                                        {
                                            output += $"/B{rwyLastDigit}{designator}";
                                        }
                                    }
                                }
                                else
                                {
                                    output += tempVar.Substring(tempVar.Length - 2);
                                }
                            }

                            AliasCommand += output;
                        }
                    }
                    else
                    {
                        if (ChartName.IndexOf("RWY") == -1)
                        {
                            output += ChartName.Split('-')[1];
                            AliasCommand += output;
                            return;
                        }

                        if (char.IsDigit(ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1]))
                        {
                            output += ChartName.Substring(ChartName.IndexOf("RWY"))[ChartName.Substring(ChartName.IndexOf("RWY")).Length - 1];
                        }
                        else
                        {
                            string tempVar = ChartName.Substring(ChartName.IndexOf("RWY"));

                            int tempCount = 0;
                            if (tempVar.Split('/').Count() > 1)
                            {
                                string rwyLastDigit = "";
                                foreach (string designator in tempVar.Split('/'))
                                {
                                    if (string.IsNullOrEmpty(designator))
                                    {
                                        continue;
                                    }

                                    if (tempCount == 0)
                                    {
                                        output += designator.Substring(designator.Length - 2);
                                        rwyLastDigit = designator[designator.Length - 2].ToString();
                                        tempCount += 1;
                                    }
                                    else
                                    {
                                        output += $"/B{rwyLastDigit}{designator}";
                                    }
                                }
                            }
                            else
                            {
                                output += tempVar.Substring(tempVar.Length - 2);
                            }
                            AliasCommand += output;
                        }
                    }
                }
                else if (ChartName.IndexOf("GLS ") != -1)
                {
                    AliasCommand += "/";
                }
                else
                {
                    AliasCommand += "/ERROR";
                }
            }
            else if (ChartCode == "DP")
            {
                if (!string.IsNullOrEmpty(Faanfd18))
                {
                    AliasCommand += $"/{AptIata}{Faanfd18.Split('.')[0].Substring(0, Faanfd18.Split('.')[0].Length - 1)}";
                }
                else
                {
                    // The DP Does not have a Computer Code.
                    AliasCommand += "/";
                }
            }
            else if (ChartCode == "ODP")
            {
                if (!string.IsNullOrEmpty(Faanfd18))
                {
                    AliasCommand += $"/{AptIata}{Faanfd18.Split('.')[0].Substring(0, Faanfd18.Split('.')[0].Length - 1)}";
                }
                else
                {
                    // The ODP does not have a Computer Code.
                    AliasCommand += "/";
                }
            }
            else if (ChartCode == "HOT")
            {
                AliasCommand += "/HS";
            }
            else if (ChartCode == "STAR")
            {
                if (!string.IsNullOrEmpty(Faanfd18))
                {
                    AliasCommand += $"/{AptIata}{Faanfd18.Split('.')[1].Substring(0, Faanfd18.Split('.')[1].Length - 1)}";
                }
                else
                {
                    // The Star does not have a Computer Code.
                    AliasCommand += "/";
                }
            }
            else if (ChartCode == "APD")
            {
                AliasCommand += "/*";
            }
            else if (ChartCode == "LAH")
            {
                AliasCommand += "/LAHSO";
            }
            else if (ChartCode == "DAU")
            {
                //AliasCommand += "/DAU";
                AliasCommand += "/";
            }
            else
            {
                AliasCommand += "/GENERALERROR";
            }
        }

    }
}
