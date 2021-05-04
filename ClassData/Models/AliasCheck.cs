using NASARData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassData.Models
{
    public class AliasCheck
    {
        public void CheckForDuplicates(string CompleteAliasFilePath) 
        {
            List<string> allAliasLines = File.ReadAllLines(CompleteAliasFilePath).ToList();
            List<string> allCommands = new List<string>();
            List<string> duplicateCommands = new List<string>();
            List<string> duplicateCommandLines = new List<string>();

            // loop through to know what commands are duplicate
            foreach (string line in allAliasLines)
            {
                if (line[0] != '.')
                {
                    continue;
                }

                string command = line.Split(' ')[0];

                if (command.Contains(".NAV"))
                {
                    continue;
                }

                if (command.Contains(".APT"))
                {
                    continue;
                }

                if (allCommands.Contains(command))
                {
                    allCommands.Add(command);
                    duplicateCommands.Add(command);
                }
                else
                {
                    allCommands.Add(command);
                }
            }

            // sorit it so its easy to see the duplicate command lines.
            duplicateCommandLines.Sort();

            // Loop back again to grab the entire line for the "duplicate Lines" 
            foreach (string line in allAliasLines)
            {
                if (line[0] != '.')
                {
                    continue;
                }

                string command = line.Split(' ')[0];
                
                if (duplicateCommands.Contains(command))
                {
                    duplicateCommandLines.Add(line);
                }
            }

            WriteDupFile(duplicateCommandLines, GlobalConfig.allAptModelsForCheck);
        }


        private void WriteDupFile(List<string> DuplicateCommandsList, List<AptModel> AirportModels) 
        {
            // TODO - Make more efficient

            string outFilePath = $"{GlobalConfig.outputDirectory}\\ALIAS\\DUPLICATE_COMMANDS.txt";
            string currentAirportIatta = "";
            string aptIatta;

            File.WriteAllText(outFilePath, "Duplicate Chart Recall commands per ARTCC. Solutions are required at ARTCC level.\n" +
                "\tConsult developers if unable to resolve at a local level.\n\n");

            foreach (string line in DuplicateCommandsList)
            {
                aptIatta = line.Substring(1, 3);

                if (currentAirportIatta == aptIatta)
                {
                    File.AppendAllText(outFilePath, "\t" + line + "\n");
                }
                else
                {
                    foreach (AptModel apt in AirportModels)
                    {
                        if (aptIatta == apt.Id)
                        {
                            File.AppendAllText(outFilePath, $"{apt.ResArtcc}\n");
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    }
                    File.AppendAllText(outFilePath, "\t" + line + "\n");

                    currentAirportIatta = aptIatta;
                }
            }
        }
    }
}
