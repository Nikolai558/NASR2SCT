using NASARData;
using System;
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
            // Get Github latest Version of the Application
            GlobalConfig.githubVersion();

            // Check to see if Version's match.
            if (GlobalConfig.ProgramVersion != GlobalConfig.GithubVersion)
            {
                // If they don't match, Tell the user about the new version on Github.
                DialogResult dialogResult = MessageBox.Show($"There is a new version released on Github\nProgram Version: {GlobalConfig.ProgramVersion}\nGithub Version: {GlobalConfig.GithubVersion}\n\nhttps://github.com/Nikolai558/NASR2SCT/releases", "Update Available", MessageBoxButtons.OK);
            }

            // Set application settings.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start the application
            Application.Run(new MainForm());
        }
    }
}
