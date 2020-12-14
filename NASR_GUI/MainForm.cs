﻿using System;
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
using System.IO;

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
            this.Text = "NASR 2 SCT - V0.5.5";

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

            processingDataLabel.Text = "Processing Fixes.";
            
            GetFixData ParseFixes = new GetFixData();
            ParseFixes.FixQuarterbackFunc(airacEffectiveDate);

            processingDataLabel.Text = "Processing Boundaries.";

            GetArbData ParseArb = new GetArbData();
            ParseArb.ArbQuarterbacFunc(airacEffectiveDate);

            processingDataLabel.Text = "Processing Airways.";

            GetAwyData ParseAWY = new GetAwyData();
            ParseAWY.AWYQuarterbackFunc(airacEffectiveDate);

            processingDataLabel.Text = "Processing Alt Airways.";

            GetAtsAwyData ParseAts = new GetAtsAwyData();
            ParseAts.AWYQuarterbackFunc(airacEffectiveDate);

            processingDataLabel.Text = "Processing NDB's.";

            GetNavData ParseNDBs = new GetNavData();
            ParseNDBs.NAVQuarterbackFunc(airacEffectiveDate, facilityID);

            processingDataLabel.Text = "Processing Airports.";

            GetAptData ParseAPT = new GetAptData();
            ParseAPT.APTQuarterbackFunc(airacEffectiveDate, facilityID, "11579568");

            processingDataLabel.Text = "Processing Waypoints XML.";

            GlobalConfig.WriteWaypointsXML();
            GlobalConfig.AppendCommentToXML(airacEffectiveDate);

            GlobalConfig.WriteNavXmlOutput();
            GlobalConfig.WriteAptXmlOutput();

            processingDataLabel.Text = "Complete";

            processingGroupBox.Visible = true;
            processingGroupBox.Enabled = true;

            runAgainButton.Visible = true;
            runAgainButton.Enabled = true;

            exitButton.Visible = true;
            exitButton.Enabled = true;

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
