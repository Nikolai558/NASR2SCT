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

            Application.Run(new MainForm());
        }
    }
}
