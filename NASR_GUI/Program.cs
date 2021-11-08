using NASARData;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
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

            GlobalConfig.CheckTempDir(true);
            //GlobalConfig.CheckTempDir();

            //GetFaaMetaFileData ParseMeta = new GetFaaMetaFileData();
            //ParseMeta.QuarterbackFunc();

            // API CALL TO GITHUB, WARNING ONLY 60 PER HOUR IS ALLOWED, WILL BREAK IF WE DO MORE!
            try
            {
                GlobalConfig.UpdateCheck();
            }
            catch (Exception)
            {
                MessageBox.Show($"NASR2SCT could not perform update check, please check internet connection.\n\nThis program will exit.\nPlease try again.");
                Environment.Exit(-1);
            }

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
                processForm.Size = new Size(600, 700);
                processForm.ChangeTitle("Update Available");
                processForm.ChangeUpdatePanel(new Point(12, 52));
                processForm.ChangeUpdatePanel(new Size(560, 370));
                processForm.ChangeProcessingLabel(new Point(5, 5));
                processForm.DisplayMessages(true);
                processForm.ShowDialog();
                
				   if (GlobalConfig.updateProgram)
				   {
				   string updateInformationMessage = "Once you click 'OK', all screens related to NASR2SCT will close.\n\n" +
				   "Once the program has fully updated, it will restart.";

				   MessageBox.Show(updateInformationMessage);

				// Create our Temp Directory so we can download assets from Github and store them here.

				/////////////////////////// TESTING - Checking to see if this is our problem code with the auto updater ///////////////////////////////////

				//GlobalConfig.createDirectories(true);

				//processForm = new Processing();
				//processForm.Size = new Size(359, 194);
				//processForm.ChangeTitle("Downloading and Installing Update");
				//processForm.ChangeUpdatePanel(new Point(12, 12));
				//processForm.ChangeUpdatePanel(new Size(319, 131));
				//processForm.ChangeProcessingLabel("Processing Update");
				//processForm.ChangeProcessingLabel(new Point(45, 49));

				//new Thread(() => processForm.ShowDialog()).Start();
				//new Thread(() => new Processing().ShowDialog()).Start();

				/////////////////////////// END TESTING - Checking to see if this is our problem code with the auto updater ///////////////////////////////////


				// This is incharge of calling squirrel to patch update the program. 
			        UpdateProgram();

                    Environment.Exit(1);
				}
            }
        }


        /// <summary>
        /// Use squirrel to update the program.
        /// </summary>

        private static void UpdateProgram()
        {
            // run OS specific update methods
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                String updateScript = "@echo off" +
                    @"CD %USERPROFILE%\AppData\Local\NASR2SCT" +
                    "for /f \"tokens = *\" %%a in ('CURL https://github.com/Nikolai558/NASR2SCT/releases/latest') do set latest_ver_URL=%%a" +
                    "for /f delims^=^\" ^ tokens ^= 2 %% x in (\"%latest_ver_URL%\") do set latest_ver_URL =%% x" +
                    "for /f \"tokens = 7 delims =/ \" %%a in (\" % latest_ver_URL % \") do set NASR2SCT_LATEST_VERSION_NUM=%%a" +
                    "powershell -Command \"Invoke - WebRequest https://github.com/Nikolai558/NASR2SCT/releases/download/%NASR2SCT_LATEST_VERSION_NUM%/NASR2SCT-%NASR2SCT_LATEST_VERSION_NUM%-full.nupkg -OutFile 'NASR2SCT-%NASR2SCT_LATEST_VERSION_NUM%-full.zip'\"" +
                    "powershell -Command \"Expand - Archive - Path NASR2SCT -% NASR2SCT_LATEST_VERSION_NUM % -full.zip - DestinationPath NASR2SCT -% NASR2SCT_LATEST_VERSION_NUM % -full\"" +
                    "DEL /Q /S NASR2SCT-%NASR2SCT_LATEST_VERSION_NUM%-full.zip" +
                    "MOVE \"%USERPROFILE%\\AppData\\Local\\NASR2SCT\\NASR2SCT-0.8.3-full\\lib\\net45\" \"%USERPROFILE%\\AppData\\Local\\NASR2SCT\\app-%NASR2SCT_LATEST_VERSION_NUM%\\" +
                    "RD /Q /S NASR2SCT-%NASR2SCT_LATEST_VERSION_NUM%-full" +
                    "CD \" % USERPROFILE %\\AppData\\Local\\NASR2SCT\app -% NASR2SCT_LATEST_VERSION_NUM % \"" +
                    "START \"\" \"NASR2SCT.exe\"" +
                    "CD \"%USERPROFILE%\\AppData\\Local\\NASR2SCT\"" +
                    "MKLINK /H NASR2SCT.exe \" % USERPROFILE %\\AppData\\Local\\NASR2SCT\\app -% NASR2SCT_LATEST_VERSION_NUM %\\NASR2SCT\\\"" +
                    "EXIT";

                File.WriteAllText($"{GlobalConfig.tempPath}\\update.bat", updateScript);
                Process.Start($"{GlobalConfig.tempPath}\\update.bat");
                Process.Start(@$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AppData\Local\NASR2SCT\app-{GlobalConfig.GithubVersion}\NASR2SCT.exe");

            } else
            {
                MessageBox.Show("This platform is not yet supported.");
            }

        }


        //from https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // run OS specific browser process
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }

}
