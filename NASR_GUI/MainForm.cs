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

            if (facilityIdTextbox.Text.Trim() == "" || facilityIdTextbox.Text.Trim() == null)
            {
                DialogResult dialogResult = MessageBox.Show("Seems there may be an error.\n Please verify you have typed a Facility ID.", "ERROR: NO Facility ID", MessageBoxButtons.OK);
                if (dialogResult == DialogResult.OK)
                {
                    return;
                }
                else
                {
                    return;
                }
            }

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
            processingGroupBox.Visible = false;
            processingGroupBox.Enabled = false;

            runAgainButton.Visible = false;
            runAgainButton.Enabled = false;

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
            runAgainButton.Location = new Point(60, 173);
            exitButton.Location = new Point(315, 173);
        }

        private void Worker_StartParsingDoWork(object sender, DoWorkEventArgs e)
        {
            GlobalConfig.CheckTempDir();

            string facilityID;

            if (currentAiracSelection.Checked)
            {
                GlobalConfig.airacEffectiveDate = currentAiracSelection.Text;
            }
            else if (nextAiracSelection.Checked)
            {
                GlobalConfig.airacEffectiveDate = nextAiracSelection.Text;
            }

            facilityID = facilityIdTextbox.Text.Replace(" ", string.Empty);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Downloading Req. Files");
            GlobalConfig.DownloadAllFiles(GlobalConfig.airacEffectiveDate, AiracDateCycleModel.AllCycleDates[GlobalConfig.airacEffectiveDate]);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Unzipping Files");
            GlobalConfig.UnzipAllDownloaded();

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing DPs and STARs");
            GetStarDpData ParseStarDp = new GetStarDpData();
            ParseStarDp.StarDpQuaterBackFunc(GlobalConfig.airacEffectiveDate);

            if (nextAiracSelection.Checked == true && nextAiracAvailable == true)
            {
                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Chart Recalls");
                GetFaaMetaFileData ParseMeta = new GetFaaMetaFileData();
                ParseMeta.QuarterbackFunc();
            }
            else if (currentAiracSelection.Checked == true)
            {
                SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Chart Recalls");
                GetFaaMetaFileData ParseMeta = new GetFaaMetaFileData();
                ParseMeta.QuarterbackFunc();
            }
            else
            {
                // Don't Parse Meta File
            }

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
            ParseNDBs.NAVQuarterbackFunc(GlobalConfig.airacEffectiveDate, facilityID);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airports");
            GetAptData ParseAPT = new GetAptData();
            ParseAPT.APTQuarterbackFunc(GlobalConfig.airacEffectiveDate, facilityID, "11579568");

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Waypoints XML");
            GlobalConfig.WriteWaypointsXML();
            GlobalConfig.AppendCommentToXML(GlobalConfig.airacEffectiveDate);
            GlobalConfig.WriteNavXmlOutput();
        }

        private void Worker_StartParsingCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            processingDataLabel.Text = "Complete";
            processingDataLabel.Refresh();

            processingGroupBox.Visible = true;
            processingGroupBox.Enabled = true;
            
            menuStrip1.Visible = true;

            runAgainButton.Visible = true;
            runAgainButton.Enabled = true;

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
            facilityIdTextbox.Enabled = false;


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

            runAgainButton.Visible = false;
            runAgainButton.Enabled = false;

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
                dialogResult = MessageBox.Show("Sory Not Implemented Yet", "Work In Progress", MessageBoxButtons.OK);
                //throw new NotImplementedException();
            }
            else
            {
                //do something else
            }
        }
    }
}
