﻿using ClassData.Models;
using NASARData;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // !!! DEBUGING CONVERTER  !!!
            //string test = new GlobalConfig().CorrectLatLon("E170.00.00.000", false, true);
            //string test2 = new GlobalConfig().CorrectLatLon("E151.00.00.000", false, true);
            //string test3 = new GlobalConfig().CorrectLatLon("E179.59.59.999", false, true);
            //string test4 = new GlobalConfig().CorrectLatLon("E179.00.00.001", false, true);
            //string test5 = new GlobalConfig().CorrectLatLon("E180.00.00.000", false, true);

            // API CALL TO GITHUB, WARNING ONLY 60 PER HOUR IS ALLOWED, WILL BREAK IF WE DO MORE!
            GlobalConfig.UpdateCheck();

            // Check Current Program Against Github, if different ask user if they want to update.
            CheckVersion();

            // Set application settings.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
                // Complete Latest release message body.
                List<string> msg = GlobalConfig.ReleaseBody.Split(new string[] { "##" }, StringSplitOptions.None).ToList();

                // Don't grab the install instructions.
                msg = msg.GetRange(2, msg.Count - 2);

                // If they don't match, Tell the user about the new version on Github.
                DialogResult dialogResult = MessageBox.Show
                    (
                    $"There is a new version released on Github\n" +
                    $"Your Program Version: {GlobalConfig.ProgramVersion}\n" +
                    $"Latest Release Version: {GlobalConfig.GithubVersion}\n\n" +
                    $"{string.Join(" ", msg)}\n\n" +
                    $"https://github.com/Nikolai558/NASR2SCT/releases", "Update Available", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    // Create our Temp Directory so we can download assets from Github and store them here.
                    GlobalConfig.createDirectories(true);

                    new Thread(() => new Processing().ShowDialog()).Start();

                    // User DOES want to update. 
                    GlobalConfig.DownloadAssets();
                    UpdateProgram();

                    // Restart the application to apply the update.
                    Application.Restart();
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
    }
}
