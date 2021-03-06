﻿using ClassData.Models;
using NASARData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassData.DataAccess
{
    public class GetTelephony
    {
        private List<TelephonyModel> allTelephony = new List<TelephonyModel>();

        public void readFAAData(string websiteFilePath) 
        {
            string[] allLines = File.ReadAllLines(websiteFilePath);

            bool inTableRow = false;
            bool inTableData = false;

            TelephonyModel currentTelephony = new TelephonyModel();

            int count = 0;

            foreach (string line in allLines)
            {
                if (line.Contains("<tr>"))
                {
                    inTableRow = true;
                    currentTelephony = new TelephonyModel();
                    continue;
                }
                if (line.Contains("<td>"))
                {
                    inTableData = true;
                    count += 1;
                    continue;
                }

                if (line.Contains("</td>"))
                {
                    inTableData = false;
                    continue;
                }
                if (line.Contains("</tr>"))
                {
                    inTableRow = false;
                    count = 0;
                    continue;
                }

                if (inTableRow && inTableData)
                {
                    string[] badCharacters = new string[] { " ", ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "\'", ";", "_", "(", ")", ":", "|", "[", "]", "-", "~", "`", "+", "\"" };

                    if (count == 1)
                    {
                        string telephonyData = line.Split('>')[1];

                        if (telephonyData.Contains('<'))
                        {
                            telephonyData = telephonyData.Split('<')[0];
                        }
                        telephonyData = telephonyData.Trim();

                        string telephonyDataAltered = telephonyData;
                        foreach (string badCharacter in badCharacters)
                        {
                            telephonyDataAltered = telephonyDataAltered.Replace(badCharacter, string.Empty);
                        }

                        currentTelephony.Telephony = telephonyData;
                        currentTelephony.TelephonyAltered = telephonyDataAltered;
                        continue;
                    }
                    else if (count == 4)
                    {
                        string threeLDData = line.Split('>')[1];

                        if (threeLDData.Contains('<'))
                        {
                            threeLDData = threeLDData.Split('<')[0];
                        }

                        threeLDData = threeLDData.Trim();
                        foreach (string badCharacter in badCharacters)
                        {
                            threeLDData = threeLDData.Replace(badCharacter, string.Empty);
                        }

                        currentTelephony.ThreeLD = threeLDData;

                        if (currentTelephony.ThreeLD.Length < 2)
                        {
                            continue;
                        }

                        allTelephony.Add(currentTelephony);
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            WriteTelephony();
        }

        public void WriteTelephony() 
        {
            string filePath = $"{GlobalConfig.outputDirectory}ALIAS\\TELEPHONY.txt";
            StringBuilder sb = new StringBuilder();

            foreach (TelephonyModel telephony in allTelephony)
            {
                sb.AppendLine($".id{telephony.ThreeLD} .MSG FAA_ISR *** 3LD: {telephony.ThreeLD} ___ TELEPHONY: {telephony.Telephony}");
                sb.AppendLine($".id{telephony.TelephonyAltered} .MSG FAA_ISR *** 3LD: {telephony.ThreeLD} ___ TELEPHONY: {telephony.Telephony}");
            }
            
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
