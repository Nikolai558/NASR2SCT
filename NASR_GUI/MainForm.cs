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

namespace NASR_GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            GlobalConfig.CheckTempDir();

            // TODO - Make it so I dont have to change this each version.
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

            GlobalConfig.createDirectories();

            GlobalConfig.WriteTestSctFile();

            menuStrip1.Visible = false;
            chooseDirButton.Enabled = false;
            startButton.Enabled = false;


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

            GlobalConfig.CheckTempDir();
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
            var worker = new BackgroundWorker();
            worker.RunWorkerCompleted += Worker_StartParsingCompleted;
            worker.DoWork += Worker_StartParsingDoWork;

            worker.RunWorkerAsync();
        }

        private void Worker_StartParsingDoWork(object sender, DoWorkEventArgs e)
        {
            string facilityID;
            string airacEffectiveDate = "";

            if (currentAiracSelection.Checked)
            {
                airacEffectiveDate = currentAiracSelection.Text;
            }
            else if (nextAiracSelection.Checked)
            {
                airacEffectiveDate = nextAiracSelection.Text;
            }

            facilityID = facilityIdTextbox.Text.Replace(" ", string.Empty);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing DPs and STARs");
            //processingDataLabel.Text = "Processing DPs and STARs";
            //processingDataLabel.Refresh();
            GetStarDpData ParseStarDp = new GetStarDpData();
            ParseStarDp.StarDpQuaterBackFunc(airacEffectiveDate);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Fixes");
            //processingDataLabel.Text = "Processing Fixes";
            //processingDataLabel.Refresh();
            GetFixData ParseFixes = new GetFixData();
            ParseFixes.FixQuarterbackFunc(airacEffectiveDate);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Boundaries");
            //processingDataLabel.Text = "Processing Boundaries";
            //processingDataLabel.Refresh();
            GetArbData ParseArb = new GetArbData();
            ParseArb.ArbQuarterbacFunc(airacEffectiveDate);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airways");
            //processingDataLabel.Text = "Processing Airways";
            GlobalConfig.CreateAwyGeomapHeadersAndEnding(true);

            //processingDataLabel.Refresh();
            GetAwyData ParseAWY = new GetAwyData();
            ParseAWY.AWYQuarterbackFunc(airacEffectiveDate);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing ATS Airways");
            //processingDataLabel.Text = "Processing ATS Airways";
            //processingDataLabel.Refresh();
            GetAtsAwyData ParseAts = new GetAtsAwyData();
            ParseAts.AWYQuarterbackFunc(airacEffectiveDate);
            GlobalConfig.CreateAwyGeomapHeadersAndEnding(false);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing NDBs");
            //processingDataLabel.Text = "Processing NDBs";
            //processingDataLabel.Refresh();
            GetNavData ParseNDBs = new GetNavData();
            ParseNDBs.NAVQuarterbackFunc(airacEffectiveDate, facilityID);

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Airports");
            //processingDataLabel.Text = "Processing Airports";
            //processingDataLabel.Refresh();
            GetAptData ParseAPT = new GetAptData();
            ParseAPT.APTQuarterbackFunc(airacEffectiveDate, facilityID, "11579568");

            SetControlPropertyThreadSafe(processingDataLabel, "Text", "Processing Waypoints XML");
            //processingDataLabel.Text = "Processing Waypoints XML";
            //processingDataLabel.Refresh();
            GlobalConfig.WriteWaypointsXML();
            GlobalConfig.AppendCommentToXML(airacEffectiveDate);
            GlobalConfig.WriteNavXmlOutput();
            GlobalConfig.WriteAptXmlOutput();
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
            GlobalConfig.CheckTempDir();

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
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            currentAiracSelection.Text = GlobalConfig.currentAiracDate;
            nextAiracSelection.Text = GlobalConfig.nextAiracDate;

            currentAiracSelection.Checked = false;
            nextAiracSelection.Checked = true;

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
        }

        private void changeLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Nikolai558/NASR2SCT/blob/development/ChangeLog.md");
        }
    }
}
