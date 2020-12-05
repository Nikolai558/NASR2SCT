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
using NASRData.DataAccess;
using ClassData.DataAccess;
using System.Threading;

namespace NASR_GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();


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

            GlobalConfig.outputDirectory = outputDir.SelectedPath;

            filePathLabel.Text = GlobalConfig.outputDirectory;
            filePathLabel.Visible = true;
            filePathLabel.MaximumSize = new Size(257, 82);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (GlobalConfig.outputDirectory == null || GlobalConfig.outputDirectory == "")
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

            GlobalConfig.createDirectories();

            GlobalConfig.WriteTestSctFile();

            chooseDirButton.Enabled = false;
            startButton.Enabled = false;

            string airacEffectiveDate = "";
            string facilityID;

            if (convertYes.Checked)
            {
                GlobalConfig.Convert = true;
            }
            else if (convertNo.Checked)
            {
                GlobalConfig.Convert = false;
            }

            if (currentAiracSelection.Checked)
            {
                airacEffectiveDate = currentAiracSelection.Text;
            }
            else if (nextAiracSelection.Checked)
            {
                airacEffectiveDate = nextAiracSelection.Text;
            }

            facilityID = facilityIdTextbox.Text;

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

            //GetFixData ParseFixes = new GetFixData();
            //ParseFixes.FixQuarterbackFunc(airacEffectiveDate);

            GetArbData ParseArb = new GetArbData();
            ParseArb.ArbQuarterbacFunc(airacEffectiveDate);

            GetAwyData ParseAWY = new GetAwyData();
            ParseAWY.AWYQuarterbackFunc(airacEffectiveDate);

            GetAtsAwyData ParseAts = new GetAtsAwyData();
            ParseAts.AWYQuarterbackFunc(airacEffectiveDate);

            GetNavData ParseNDBs = new GetNavData();
            ParseNDBs.NAVQuarterbackFunc(airacEffectiveDate, facilityID);

            GetAptData ParseAPT = new GetAptData();
            ParseAPT.APTQuarterbackFunc(airacEffectiveDate, facilityID, "11579568");

            GlobalConfig.WriteWaypointsXML();
            GlobalConfig.AppendCommentToXML(airacEffectiveDate);

            processingDataLabel.Text = "Complete";

            processingGroupBox.Visible = true;
            processingGroupBox.Enabled = true;

            runAgainButton.Visible = true;
            runAgainButton.Enabled = true;

            exitButton.Visible = true;
            exitButton.Enabled = true;

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

        private void getAiracDate() 
        {

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
    }
}
