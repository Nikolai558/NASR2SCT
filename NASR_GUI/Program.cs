﻿using ClassData.DataAccess;
using ClassData.Models;
using NASARData;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace NASR_GUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set application settings.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // API CALL TO GITHUB, WARNING ONLY 60 PER HOUR IS ALLOWED, WILL BREAK IF WE DO MORE!
            GlobalConfig.UpdateCheck();

            // Check Current Program Against Github, if different ask user if they want to update.
            CheckVersion();

            // Start the application
            Application.Run(new MainForm());
           
        }

        /// <summary>
        /// Check the Program version against Github, if different ask the user if they want to update.
        /// </summary>
        private static void CheckVersion() 
        {
            // Check to see if Version's match.
            if (GlobalConfig.ProgramVersion != GlobalConfig.GithubVersion)
            {
                Processing processForm = new Processing();
                processForm.Size = new Size(600, 600);
                processForm.ChangeTitle("Update Available");
                processForm.ChangeUpdatePanel(new Point(12, 52));
                processForm.ChangeUpdatePanel(new Size(560, 370));
                processForm.ChangeProcessingLabel(new Point(5, 5));
                processForm.DisplayMessages(true);
                processForm.ShowDialog();

                if (GlobalConfig.updateProgram)
                {
                    // Create our Temp Directory so we can download assets from Github and store them here.
                    GlobalConfig.createDirectories(true);

                    processForm = new Processing();
                    processForm.Size = new Size(359, 194);
                    processForm.ChangeTitle("Downloading and Installing Update");
                    processForm.ChangeUpdatePanel(new Point(12, 12));
                    processForm.ChangeUpdatePanel(new Size(319, 131));
                    processForm.ChangeProcessingLabel("Processing Update");
                    processForm.ChangeProcessingLabel(new Point(45, 49));

                    new Thread(() => processForm.ShowDialog()).Start();
                    //new Thread(() => new Processing().ShowDialog()).Start();

                    // User DOES want to update. 
                    GlobalConfig.DownloadAssets();
                    
                    UpdateProgram();


                    StartNewVersion();

                    Environment.Exit(1);
                }
                else
                {
                    // User does not want to Update
                }
            }
        }

        /// <summary>
        /// Use squirrel to update the program.
        /// </summary>
        private static async void UpdateProgram() 
        {
            using (var updateManager = new UpdateManager($"{GlobalConfig.tempPath}"))
            {
                var releaseEntry = await updateManager.UpdateApp();
            }
        }


        private static void StartNewVersion() 
        {
            string filePath = $"{GlobalConfig.tempPath}\\startNewVersion.bat";
            string writeMe =
                "SET /A COUNT=0\n\n" +
                ":CHK\n" +
                $"IF EXIST \"%userprofile%\\AppData\\Local\\NASR2SCT\\app-{GlobalConfig.GithubVersion}\\NASR2SCT.exe\" goto FOUND\n" +
                "SET /A COUNT=%COUNT% + 1\n" +
                "IF %COUNT% GEQ 6 GOTO FOUND\n" +
                "PING 127.0.0.1 -n 3 >nul\n" +
                "GOTO CHK\n\n" +
                ":FOUND\n" +
                $"start \"\" \"%userprofile%\\AppData\\Local\\NASR2SCT\\app-{GlobalConfig.GithubVersion}\\NASR2SCT.exe\"\n";


            File.WriteAllText(filePath, writeMe);


            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + $"\"{GlobalConfig.tempPath}\\startNewVersion.bat\"");
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;

            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();

            ExitCode = Process.ExitCode;
            Process.Close();
        }
    }
}
