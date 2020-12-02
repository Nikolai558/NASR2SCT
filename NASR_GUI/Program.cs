using ClassData.DataAccess;
using NASARData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //GlobalConfig.outputDirectory = "C:\\Users\\Pocono Coast West\\Downloads";
            //GlobalConfig.createDirectories();
            //new GetAptData().WriteEramAirportsXML();
            //new GetAptData().WriteEramXML();

            GlobalConfig.githubVersion();

            if (GlobalConfig.ProgramVersion != GlobalConfig.GithubVersion)
            {
                DialogResult dialogResult = MessageBox.Show($"There is a new version released on Github\nProgram Version: {GlobalConfig.ProgramVersion}\nGithub Version: {GlobalConfig.GithubVersion}\n\nhttps://github.com/Nikolai558/NASR2SCT/releases", "Update Available", MessageBoxButtons.OK);
            }


            Application.Run(new MainForm());
        }
    }
}
