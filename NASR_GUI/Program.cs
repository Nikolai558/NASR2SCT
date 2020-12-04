using ClassData.Models;
using NASARData;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // API CALL TO GITHUB, WARNING ONLY 60 PER HOUR IS ALLOWED, WILL BREAK IF WE DO MORE!
            GlobalConfig.UpdateCheck();

            // Check to see if Version's match.
            if (GlobalConfig.ProgramVersion == GlobalConfig.GithubVersion)
            {
                List<string> msg = GlobalConfig.ReleaseBody.Split( new string[] { "##" }, StringSplitOptions.None).ToList();

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
                    // User DOES want to update. 
                    GlobalConfig.DownloadAssets();
                    UpdateProgram();

                    DialogResult dialogResult1 = MessageBox.Show
                    ($"Restar the program for update to kick in.\n", "Update Complete", MessageBoxButtons.OK);
                }
                else
                {
                    // User does not want to Update
                }

            }

            // Set application settings.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            // Start the application
            Application.Run(new MainForm());
        }

        private static async void UpdateProgram() 
        {
            using (var updateManager = new UpdateManager($"{GlobalConfig.tempPath}"))
            {
                var releaseEntry = await updateManager.UpdateApp();
            }
        }
    }
}
