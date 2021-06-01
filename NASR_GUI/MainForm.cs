using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NASARData;
using ClassData;
using System.Threading;
using System.IO;
using System.Drawing.Text;
using System.Reflection;
using ClassData.DataAccess;
using NASRData.DataAccess;
using System.Net;
using ClassData.Models.MetaFileModels;
using ClassData.Models;
using System.Diagnostics;

namespace NASR_GUI
{
    public partial class MainForm : Form
    {
        private bool nextAiracAvailable;

        public MainForm()
        {
            InitializeComponent();

            // It should grab from the assembily info. 
            this.Text = $"NASR 2 SCT - V{GlobalConfig.ProgramVersion}";

            chooseDirButton.Enabled = false;
            startButton.Enabled = false;
            airacCycleGroupBox.Enabled = false;
            airacCycleGroupBox.Visible = false;

            convertGroupBox.Enabled = false;
            convertGroupBox.Visible = false;

            startGroupBox.Enabled = false;
            startGroupBox.Visible = false;

            processingGroupBox.Visible = true;
            processingGroupBox.Enabled = true;
            processingDataLabel.Visible = true;
            processingDataLabel.Enabled = true;

            facilityIdCombobox.DataSource = GlobalConfig.allArtcc;

            GlobalConfig.outputDirBase = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePathLabel.Text = GlobalConfig.outputDirBase;
            filePathLabel.Visible = true;
            filePathLabel.MaximumSize = new Size(257, 82);
        }

        private void currentAiracSelection_CheckedChanged(object sender, EventArgs e)
        {
            currentAiracSelection.Text = GlobalConfig.currentAiracDate;
            nextAiracSelection.Text = GlobalConfig.nextAiracDate;
        }

        private void nextAiracSelection_CheckedChanged(object sender, EventArgs e)
        {
            currentAiracSelection.Text = GlobalConfig.currentAiracDate;
            nextAiracSelection.Text = GlobalConfig.nextAiracDate;
        }

        private void chooseDirButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog outputDir = new FolderBrowserDialog();

            outputDir.ShowDialog();

            GlobalConfig.outputDirBase = outputDir.SelectedPath;

            filePathLabel.Text = GlobalConfig.outputDirBase;
            filePathLabel.Visible = true;
            filePathLabel.MaximumSize = new Size(257, 82);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (GlobalConfig.outputDirBase == null || GlobalConfig.outputDirBase == "")
            {
                DialogResult dialogResult = MessageBox.Show("Seems there may be an error.\n Please verify you have chosen an output location.", "ERROR: NO Output Location", MessageBoxButtons.OK);
                if (dialogResult == DialogResult.OK)
                {
                    return;
                }
                else
                {
                    return;
                }
            }

            if (GlobalConfig.facilityID == "" || GlobalConfig.facilityID.Trim() == null)
            {
                DialogResult dialogResult = MessageBox.Show("Seems there may be an error.\n Please verify you have selected a correct Facility ID.", "ERROR: NO Facility ID", MessageBoxButtons.OK);
                if (dialogResult == DialogResult.OK)
                {
                    return;
                }
                else
                {
                    return;
                }
            }

            //if (facilityIdTextbox.Text.Trim() == "" || facilityIdTextbox.Text.Trim() == null)
            //{
            //    DialogResult dialogResult = MessageBox.Show("Seems there may be an error.\n Please verify you have selected an Incorrect Facility ID.", "ERROR: NO Facility ID", MessageBoxButtons.OK);
            //    if (dialogResult == DialogResult.OK)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

            if (GlobalConfig.outputDirectory == null)
            {
                GlobalConfig.outputDirectory = $"{GlobalConfig.outputDirBase}\\NASR2SCT_Output";

                if (Directory.Exists(GlobalConfig.outputDirectory))
                {
                    GlobalConfig.outputDirectory += $"-{DateTime.Now.ToString("MMddHHmmss")}";
                }

                GlobalConfig.outputDirectory += "\\";
            }
            else
            {
                GlobalConfig.outputDirectory = $"{GlobalConfig.outputDirBase}\\NASR2SCT_Output";

                if (Directory.Exists(GlobalConfig.outputDirectory))
                {
                    GlobalConfig.outputDirectory += $"-{DateTime.Now.ToString("MMddHHmmss")}";
                }

                GlobalConfig.outputDirectory += "\\";
            }

            GlobalConfig.CreateDirectories();

            GlobalConfig.WriteTestSctFile();

            menuStrip1.Visible = false;
            chooseDirButton.Enabled = false;
            //startButton.Enabled = false;


            if (convertYes.Checked)
            {
                GlobalConfig.Convert = true;
            }
            else if (convertNo.Checked)
            {
                GlobalConfig.Convert = false;
            }

            airacCycleGroupBox.Enabled = false;
            airacCycleGroupBox.Visible = false;

            convertGroupBox.Enabled = false;
            convertGroupBox.Visible = false;

            startGroupBox.Enabled = false;
            startGroupBox.Visible = false;

            processingGroupBox.Visible = true;
            processingGroupBox.Enabled = true;
            processingDataLabel.Visible = true;
            processingDataLabel.Enabled = true;

            startParsing();
        }

        private void runAgainButton_Click(object sender, EventArgs e)
        {
            // Artifact Code - REMOVE THIS. 

            processingGroupBox.Visible = false;
            processingGroupBox.Enabled = false;

            //runAgainButton.Visible = false;
            //runAgainButton.Enabled = false;

            exitButton.Visible = false;
            exitButton.Enabled = false;

            processingDataLabel.Text = "Processing Data, Please Wait.";

            processingDataLabel.Visible = false;
            processingDataLabel.Enabled = false;

            chooseDirButton.Enabled = true;
            startButton.Enabled = true;

            airacCycleGroupBox.Enabled = true;
            airacCycleGroupBox.Visible = true;

            convertGroupBox.Enabled = true;
            convertGroupBox.Visible = true;

            startGroupBox.Enabled = true;
            startGroupBox.Visible = true;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);

        public static void SetControlPropertyThreadSafe(
            Control control,
            string propertyName,
            object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate
                (SetControlPropertyThreadSafe),
                new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { propertyValue });
            }
        }

        private void startParsing()
        {
            AdjustProcessingBox();

            var worker = new BackgroundWorker();
            worker.RunWorkerCompleted += Worker_StartParsingCompleted;
            worker.DoWork += Worker_StartParsingDoWork;

            worker.RunWorkerAsync();
        }

        private void AdjustProcessingBox() 
        {
            outputDirectoryLabel.Text = GlobalConfig.outputDirectory;
            outputDirectoryLabel.Visible = true;
            outputLocationLabel.Visible = true;

            processingGroupBox.Location = new Point(114, 59);
            processingGroupBox.Size = new Size(557, 213);

            outputLocationLabel.Location = new Point(9, 22);
            outputDirectoryLabel.Location = new Point(24, 47);
            processingDataLabel.Location = new Point(6, 102);
            //runAgainButton.Location = new Point(60, 173);
            exitButton.Location = new Point(187, 173);
        }

        private void Worker_StartParsingDoWork(object sender, DoWorkEventArgs e)
        {
            GlobalConfig.CheckTempDir();

            if (currentAiracSelection.Checked)
            {
                GlobalConfig.airacEffectiveDate = currentAiracSelection.Text;
            }
            else if (nextAiracSelection.Checked)
            {
                GlobalConfig.airacEffectiveDate = nextAiracSelection.Text;
            }

            //facilityIdCombobox.Invoke(new MethodInvoker(delegate { facilityID = facilityIdCombobox.SelectedItem.ToString(); }));
            //facilityID = facilityIdCombobox.SelectedItem.ToString();

            // TODO - This is messy and needs to be cleaned up. 

            if (nextAiracSelection.Checked == true && nextAiracAvailable == false)
            {
                // DialogResult dialogResult = MessageBox.Show("Next Airacc not available, No Code to handle this yet.", "ERROR: NO Facility ID", MessageBoxButtons.OK);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Downloading FAA Data");
                GlobalConfig.DownloadAllFiles(GlobalConfig.airacEffectiveDate, AiracDateCycleModel.AllCycleDates[GlobalConfig.airacEffectiveDate], false);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Unzipping Files");
                GlobalConfig.UnzipAllDownloaded();

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Telephony");
                GetTelephony Telephony = new GetTelephony();
                Telephony.readFAAData($"{GlobalConfig.tempPath}\\{AiracDateCycleModel.AllCycleDates[GlobalConfig.airacEffectiveDate]}_TELEPHONY.html");

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing DPs and STARs");
                GetStarDpData ParseStarDp = new GetStarDpData();
                ParseStarDp.StarDpQuaterBackFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airports");
                GetAptData ParseAPT = new GetAptData();
                ParseAPT.APTQuarterbackFunc(GlobalConfig.airacEffectiveDate, GlobalConfig.facilityID);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Fixes");
                GetFixData ParseFixes = new GetFixData();
                ParseFixes.FixQuarterbackFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Boundaries");
                GetArbData ParseArb = new GetArbData();
                ParseArb.ArbQuarterbacFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airways");
                GlobalConfig.CreateAwyGeomapHeadersAndEnding(true);

                GetAwyData ParseAWY = new GetAwyData();
                ParseAWY.AWYQuarterbackFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing ATS Airways");
                GetAtsAwyData ParseAts = new GetAtsAwyData();
                ParseAts.AWYQuarterbackFunc(GlobalConfig.airacEffectiveDate);
                GlobalConfig.CreateAwyGeomapHeadersAndEnding(false);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing NDBs");
                GetNavData ParseNDBs = new GetNavData();
                ParseNDBs.NAVQuarterbackFunc(GlobalConfig.airacEffectiveDate, GlobalConfig.facilityID);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Waypoints XML");
                GlobalConfig.WriteWaypointsXML();
                GlobalConfig.AppendCommentToXML(GlobalConfig.airacEffectiveDate);
                GlobalConfig.WriteNavXmlOutput();

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Checking Alias Commands");
                AliasCheck aliasCheck = new AliasCheck();
                aliasCheck.CheckForDuplicates($"{GlobalConfig.outputDirectory}\\ALIAS\\AliasTestFile.txt");
            }
            else 
            {
                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Downloading FAA Data");
                GlobalConfig.DownloadAllFiles(GlobalConfig.airacEffectiveDate, AiracDateCycleModel.AllCycleDates[GlobalConfig.airacEffectiveDate]);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Unzipping Files");
                GlobalConfig.UnzipAllDownloaded();

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Telephony");
                GetTelephony Telephony = new GetTelephony();
                Telephony.readFAAData($"{GlobalConfig.tempPath}\\{AiracDateCycleModel.AllCycleDates[GlobalConfig.airacEffectiveDate]}_TELEPHONY.html");

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing DPs and STARs");
                GetStarDpData ParseStarDp = new GetStarDpData();
                ParseStarDp.StarDpQuaterBackFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airports");
                GetAptData ParseAPT = new GetAptData();
                ParseAPT.APTQuarterbackFunc(GlobalConfig.airacEffectiveDate, GlobalConfig.facilityID);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Chart Recalls");
                GetFaaMetaFileData ParseMeta = new GetFaaMetaFileData();
                ParseMeta.QuarterbackFunc();

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Getting Publications");
                PublicationParser publications = new PublicationParser();
                publications.WriteAirportInfoTxt(GlobalConfig.facilityID);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Fixes");
                GetFixData ParseFixes = new GetFixData();
                ParseFixes.FixQuarterbackFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Boundaries");
                GetArbData ParseArb = new GetArbData();
                ParseArb.ArbQuarterbacFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airways");
                GlobalConfig.CreateAwyGeomapHeadersAndEnding(true);

                GetAwyData ParseAWY = new GetAwyData();
                ParseAWY.AWYQuarterbackFunc(GlobalConfig.airacEffectiveDate);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing ATS Airways");
                GetAtsAwyData ParseAts = new GetAtsAwyData();
                ParseAts.AWYQuarterbackFunc(GlobalConfig.airacEffectiveDate);
                GlobalConfig.CreateAwyGeomapHeadersAndEnding(false);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing NDBs");
                GetNavData ParseNDBs = new GetNavData();
                ParseNDBs.NAVQuarterbackFunc(GlobalConfig.airacEffectiveDate, GlobalConfig.facilityID);

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Waypoints XML");
                GlobalConfig.WriteWaypointsXML();
                GlobalConfig.AppendCommentToXML(GlobalConfig.airacEffectiveDate);
                GlobalConfig.WriteNavXmlOutput();

                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Checking Alias Commands");
                AliasCheck aliasCheck = new AliasCheck();
                aliasCheck.CheckForDuplicates($"{GlobalConfig.outputDirectory}\\ALIAS\\AliasTestFile.txt");
            }
        }

        private void Worker_StartParsingCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            processingDataLabel.Text = "Complete";
            processingDataLabel.Refresh();

            processingGroupBox.Visible = true;
            processingGroupBox.Enabled = true;
            
            menuStrip1.Visible = true;

            //runAgainButton.Visible = true;
            //runAgainButton.Enabled = true;

            exitButton.Visible = true;
            exitButton.Enabled = true;
        }

        private void getAiracDate() 
        {
            //GlobalConfig.GetAiracDateFromFAA();
            var Worker = new BackgroundWorker();
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            Worker.DoWork += Worker_DoWork;

            Worker.RunWorkerAsync();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            getAiracDate();
            currentAiracSelection.Text = GlobalConfig.currentAiracDate;
            nextAiracSelection.Text = GlobalConfig.nextAiracDate;

            // Disable the Facility ID for Now, Might want it later, If so comment out the below line.
            // facilityIdTextbox.Enabled = false;


        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (GlobalConfig.nextAiracDate == null)
            {
                GlobalConfig.GetAiracDateFromFAA();
            }
            // Check to see if Meta file for Next Airac is available.
            nextAiracAvailable = GlobalConfig.GetMetaUrlResponse();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            currentAiracSelection.Text = GlobalConfig.currentAiracDate;
            nextAiracSelection.Text = GlobalConfig.nextAiracDate;

            processingGroupBox.Visible = false;
            processingGroupBox.Enabled = false;

            //runAgainButton.Visible = false;
            //runAgainButton.Enabled = false;

            exitButton.Visible = false;
            exitButton.Enabled = false;

            processingDataLabel.Text = "Processing Data, Please Wait.";

            processingDataLabel.Visible = false;
            processingDataLabel.Enabled = false;

            chooseDirButton.Enabled = true;
            startButton.Enabled = true;

            airacCycleGroupBox.Enabled = true;
            airacCycleGroupBox.Visible = true;

            convertGroupBox.Enabled = true;
            convertGroupBox.Visible = true;

            startGroupBox.Enabled = true;
            startGroupBox.Visible = true;
        }

        private void instructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            System.Diagnostics.Process.Start("https://docs.google.com/presentation/d/e/2PACX-1vR79DqYD9FxQhA-mUK1FQLO4Xx4mg5xO05NOIJMeB4mbIbs3CY5pIOYtrFtqo8BfmlCFaJSFMSxI_ut/embed?");

        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreditsForm frm = new CreditsForm();
            frm.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            var pfc = new PrivateFontCollection();
            pfc.AddFontFile("Properties\\romantic.ttf");
            instructionsToolStripMenuItem.Font = new Font(pfc.Families[0], 12, FontStyle.Regular);
            creditsToolStripMenuItem.Font = new Font(pfc.Families[0], 12, FontStyle.Regular);
            changeLogToolStripMenuItem.Font = new Font(pfc.Families[0], 12, FontStyle.Regular);
            uninstallToolStripMenuItem.Font = new Font(pfc.Families[0], 12, FontStyle.Regular);



        }

        private void changeLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Nikolai558/NASR2SCT/blob/development/ChangeLog.md");
        }

        private void nextAiracSelection_Click(object sender, EventArgs e)
        {

            if (!nextAiracAvailable)
            {
                //nextAiracSelection.Checked = false;
                //currentAiracSelection.Checked = true;

                MetaNotFoundForm frm = new MetaNotFoundForm();
                frm.ShowDialog();
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Would you like to UNINSTALL NASR2SCT?", "Uninstall NASR2SCT", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string uninstall_start_string = $"start \"\" \"{Path.GetTempPath()}UNINSTALL_NASR2SCT.bat\"";

                // dialogResult = MessageBox.Show("Sory Not Implemented Yet", "Work In Progress", MessageBoxButtons.OK);
                string uninstallBatchFileString = "@echo off\n"
                        + "PING 127.0.0.1 - n 5 > nul\n"
                        + "tasklist /FI \"IMAGENAME eq NASR2SCT.exe\" 2>NUL | find /I /N \"NASR2SCT.exe\">NUL\n"
                        + "if \"%ERRORLEVEL%\"==\"0\" taskkill /F /im NASR2SCT.exe\n"
                        + "\n"
                        + "TITLE NASR_2_SCT UNINSTALL\n"
                        + "\n"
                        + "SET /A NOT_FOUND_COUNT=0\n"
                        + "\n"
                        + "CD \"%temp%\"\n"
                        + "	if NOT exist NASR2SCT (\n"
                        + "		SET /A NOT_FOUND_COUNT=%NOT_FOUND_COUNT% + 1\n"
                        + "		SET NASR_TEMP_FOLDER=NOT_FOUND\n"
                        + "	)\n"
                        + "	\n"
                        + "	if exist NASR2SCT (\n"
                        + "		SET NASR_TEMP_FOLDER=FOUND\n"
                        + "		RD /Q /S \"NASR2SCT\"\n"
                        + "	)\n"
                        + "\n"
                        + "CD \"%userprofile%\\AppData\\Local\"\n"
                        + "	if NOT exist NASR2SCT (\n"
                        + "		SET /A NOT_FOUND_COUNT=%NOT_FOUND_COUNT% + 1\n"
                        + "		SET NASR_APPDATA_FOLDER=NOT_FOUND\n"
                        + "	)\n"
                        + "	\n"
                        + "	if exist NASR2SCT (\n"
                        + "		SET NASR_APPDATA_FOLDER=FOUND\n"
                        + "		RD /Q /S \"NASR2SCT\"\n"
                        + "	)\n"
                        + "\n"
                        + "CD \"%userprofile%\\Desktop\"\n"
                        + "	if NOT exist NASR2SCT.lnk (\n"
                        + "		SET /A NOT_FOUND_COUNT=%NOT_FOUND_COUNT% + 1\n"
                        + "		SET NASR_SHORTCUT=NOT_FOUND\n"
                        + "	)\n"
                        + "\n"
                        + "	if exist NASR2SCT.lnk (\n"
                        + "		SET NASR_SHORTCUT=FOUND\n"
                        + "		DEL /Q \"NASR2SCT.lnk\"\n"
                        + "	)\n"
                        + "\n"
                        + "IF %NOT_FOUND_COUNT%==0 SET UNINSTALL_STATUS=COMPLETE\n"
                        + "IF %NOT_FOUND_COUNT%==1 SET UNINSTALL_STATUS=PARTIAL\n"
                        + "IF %NOT_FOUND_COUNT%==2 SET UNINSTALL_STATUS=PARTIAL\n"
                        + "IF %NOT_FOUND_COUNT%==3 SET UNINSTALL_STATUS=FAIL\n"
                        + "\n"
                        + "IF %UNINSTALL_STATUS%==COMPLETE GOTO UNINSTALLED\n"
                        + "IF %UNINSTALL_STATUS%==PARTIAL GOTO UNINSTALLED\n"
                        + "IF %UNINSTALL_STATUS%==FAIL GOTO FAILED\n"
                        + "\n"
                        + "CLS\n"
                        + "\n"
                        + ":UNINSTALLED\n"
                        + "\n"
                        + "ECHO.\n"
                        + "ECHO.\n"
                        + "ECHO SUCCESSFULLY UNINSTALLED THE FOLLOWING:\n"
                        + "ECHO.\n"
                        + "IF %NASR_TEMP_FOLDER%==FOUND ECHO        -temp\\NASR2SCT\n"
                        + "IF %NASR_APPDATA_FOLDER%==FOUND ECHO        -AppData\\Local\\NASR2SCT\n"
                        + "IF %NASR_SHORTCUT%==FOUND ECHO        -Desktop\\NASR2SCT Shortcut\n"
                        + "\n"
                        + ":FAILED\n"
                        + "\n"
                        + "IF NOT %NOT_FOUND_COUNT%==0 (\n"
                        + "	ECHO.\n"
                        + "	ECHO.\n"
                        + "	ECHO.\n"
                        + "	ECHO.\n"
                        + "	IF %UNINSTALL_STATUS%==PARTIAL ECHO NOT ABLE TO COMPLETELY UNINSTALL BECAUSE THE FOLLOWING COULD NOT BE FOUND:\n"
                        + "	IF %UNINSTALL_STATUS%==FAIL ECHO UNINSTALL FAILED COMPLETELY BECAUSE THE FOLLOWING COULD NOT BE FOUND:\n"
                        + "	ECHO.\n"
                        + "	IF %NASR_TEMP_FOLDER%==NOT_FOUND ECHO        -temp\\NASR2SCT\n"
                        + "	IF %NASR_APPDATA_FOLDER%==NOT_FOUND ECHO        -AppData\\Local\\NASR2SCT\n"
                        + "	IF %NASR_SHORTCUT%==NOT_FOUND (\n"
                        + "		ECHO        -Desktop\\NASR2SCT Shortcut\n"
                        + "		ECHO             --If the shortcut was renamed, delete the shortcut manually.\n"
                        + "	)\n"
                        + ")\n"
                        + "\n"
                        + "ECHO.\n"
                        + "ECHO.\n"
                        + "ECHO.\n"
                        + "ECHO.\n"
                        + "ECHO.\n"
                        + "ECHO ...PRESS ANY KEY TO EXIT\n"
                        + "\n"
                        + "PAUSE>NUL\n";

                File.WriteAllText($"{Path.GetTempPath()}UNINSTALL_NASR2SCT.bat", uninstallBatchFileString);
                File.WriteAllText($"{Path.GetTempPath()}UNINSTALL_START_NASR2SCT.bat", uninstall_start_string);

                ProcessStartInfo ProcessInfo;
                Process Process;

                ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + $"\"{Path.GetTempPath()}UNINSTALL_START_NASR2SCT.bat\"");
                ProcessInfo.CreateNoWindow = false;
                ProcessInfo.UseShellExecute = false;

                Process = Process.Start(ProcessInfo);

                Process.Close();


                // TODO - COME BACK TO THIS 
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //List<string> directories = new List<string>() { $"{GlobalConfig.tempPath}", ""};


                //foreach (string path in directories)
                //{

                //    if (Directory.Exists(path))
                //    {
                //        // This variable holds all information for the temp path ie. Directories and files.
                //        DirectoryInfo di = new DirectoryInfo(path);

                //        // Loop through the Files in our TempPath
                //        foreach (FileInfo file in di.EnumerateFiles())
                //        {
                //            // Delete each file it finds inside of this directory. IE Temp Path
                //            file.Delete();
                //        }

                //        // Loop through the Directories in our TempPath
                //        foreach (DirectoryInfo dir in di.EnumerateDirectories())
                //        {
                //            // Delete the folder it finds.
                //            dir.Delete(true);
                //        }
                //    }
                //}
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //File.Delete($"{Path.GetTempPath()}UNINSTALL_NASR2SCT.bat");

                Environment.Exit(1);
            }
        }

        private void facilityIdCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalConfig.facilityID = facilityIdCombobox.SelectedItem.ToString();
        }
    }
}
