<<<<<<< HEAD:NASR_GUI/Program.cs
﻿using NASARData;
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

				// Opens program after update
				StartNewVersion();

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
                // Download ZIP archive of program
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://github.com/Nikolai558/NASR2SCT/releases/download/" + GlobalConfig.GithubVersion + "/NASR2SCT-" + GlobalConfig.GithubVersion + ".zip", "update.zip");
                // Unzip and delete archive
                ZipFile.ExtractToDirectory("update.zip", "update");
                File.Delete("update.zip");
                // Move update to AppData\Roaming
                try
                {
                    Directory.Move(@"update\NASR2SCT-" + GlobalConfig.GithubVersion, Directory.GetParent(Application.UserAppDataPath) + @"\NASR2SCT-" + GlobalConfig.GithubVersion);
                } catch (IOException e)
                {

                }
                string runner = $"start /d \"{Directory.GetParent(Application.UserAppDataPath) + @"\NASR2SCT-" + GlobalConfig.GithubVersion}\" NASR2SCT.exe";
                File.WriteAllText($"{GlobalConfig.tempPath}\\test.bat", runner);

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
=======
﻿using NASR2SCTDATA;
using Squirrel;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                MessageBox.Show($"NASR2SCT could not preform update check, please check internet connection.\n\nThis program will exit.\nPlease try again.");
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
                processForm.Size = new Size(600, 600);
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


                    // User DOES want to update. 
                    GlobalConfig.DownloadAssets();

                    //ZipFile.ExtractToDirectory($"{GlobalConfig.tempPath}\\NASR2SCT-{GlobalConfig.GithubVersion}.zip", $"{GlobalConfig.tempPath}\\program");


                    //// This is incharge of calling squirrel to patch update the program. 
                    UpdateProgram();

                    //// this is needed to open the program after squirrel is done with it.
                    StartNewVersion();

                    //CallSetupExe();


                    //if (File.Exists($"{GlobalConfig.tempPath}\\Setup.exe"))
                    //{

                    //}
                    //else
                    //{
                    //    MessageBox.Show(" ", "Antivirus removed my files!", MessageBoxButtons.OK);
                    //}

                    Environment.Exit(1);
                }
                else
                {
                    // User does not want to Update
                }
            }
        }

        private static void CallSetupExe()
        {
            string batchFileOne =
                "PING 127.0.0.1 -n 3 >nul\n" +
                "cd \"%temp%\\NASR2SCT\"\n" +
                "SET /A COUNT=0\n" +
                ":CHK\n" +
                "IF EXIST \"second.bat\" goto FOUND\n" +
                "SET /A COUNT=%COUNT% + 1\n" +
                "IF %COUNT% GEQ 6 GOTO FOUND\n" +
                "PING 127.0.0.1 -n 3 >nul\n" +
                "GOTO CHK\n" +
                ":FOUND\n" +
                "START \"\" \"second.bat\"\n";

            string batchFileTwo =
                "@echo off\n" +
                "PING 127.0.0.1 -n 3 >nul\n" +
                "cd \"%temp%\\NASR2SCT\"\n" +
                "SET /A COUNT=0\n" +
                ":CHK\n" +
                "IF EXIST \"Setup.exe\" goto FOUND\n" +
                "SET /A COUNT=%COUNT% + 1\n" +
                "IF %COUNT% GEQ 6 GOTO FOUND\n" +
                "PING 127.0.0.1 -n 3 >nul\n" +
                "GOTO CHK\n" +
                ":FOUND\n" +
                "START \"\" \"Setup.exe\"\n" +
                "PING 127.0.0.1 -n 3 >nul\n" +
                "cd \"%userprofile%\\AppData\\Local\\NASR2SCT\"\n" +
                "SET /A COUNT2=0\n" +
                ":CHK2\n" +
                "IF EXIST \"NASR2SCT.exe\" goto FOUND2\n" +
                "SET /A COUNT2=%COUNT2% + 1\n" +
                "IF %COUNT2% GEQ 12 GOTO FOUND2\n" +
                "PING 127.0.0.1 -n 3 >nul\n" +
                "GOTO CHK2\n" +
                ":FOUND2\n" +
                "START \"\" \"NASR2SCT.exe\"\n";

            File.WriteAllText($"{GlobalConfig.tempPath}\\first.bat", batchFileOne);
            File.WriteAllText($"{GlobalConfig.tempPath}\\second.bat", batchFileTwo);



            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + $"\"{GlobalConfig.tempPath}\\first.bat\"");
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;

            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();

            ExitCode = Process.ExitCode;
            Process.Close();
        } 

        /// <summary>
        /// Use squirrel to update the program.
        /// </summary>
        private static async void UpdateProgram() 
        {
            //var updateManager = new UpdateManager($"{GlobalConfig.tempPath}");
            //var releaseEntry = await updateManager.UpdateApp();


            using (var updateManager = new UpdateManager($"{GlobalConfig.tempPath}"))
            //using (var updateManager = new UpdateManager($"{GlobalConfig.tempPath}\\program\\NASR2SCT-{GlobalConfig.GithubVersion}"))
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
                "IF %COUNT% GEQ 12 GOTO FOUND\n" +
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
>>>>>>> 1bfa1b3c110d59d0f4ff57964c804509db203caf:NASR2SCT-WINFORMS/Program.cs
