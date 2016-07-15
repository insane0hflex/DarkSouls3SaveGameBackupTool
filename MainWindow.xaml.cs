using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Media;

namespace DarkSouls3SaveGameBackupTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Dark souls 3 Save game folder file path/location
        private string saveGameLocation = "";

        //Dark souls 3 Saves backup folder file path/location
        private string saveBackupLocation = "";

        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        //pale yellow to simulate DarkSouls3 like UI color scheme
        Color paleYellow = new Color();

        //the actual Colors.DarkGray isn't actually darker (more black) than regular Colors.Gray...
        //so this is a color that is inbetween gray and black for use for the UI
        Color deepGray = new Color();

        //the time of the last write to the save file
        DateTime lastSaveWriteTime;

        public MainWindow()
        {
            InitializeComponent();

            //0xFFF5EECF == pale yellow like color
            paleYellow.A = 0xFF;
            paleYellow.R = 0xF5;
            paleYellow.G = 0xEE;
            paleYellow.B = 0xCF;

            //0xFF515151 == deep gray color
            deepGray.A = 0xFF;
            deepGray.R = 0x51;
            deepGray.G = 0x51;
            deepGray.B = 0x51;


            Disable_btnEndBackUpProcess();
            btn_startBackUpProcess.FontWeight = FontWeights.Bold;


            //avoid duplication of ticks - assign eventhandler for tick on init
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);


            if (!File.Exists("DarkSouls3SaveGameBackupTool.exe.config"))
            {
                CreateAppConfigFile();
            }

            txtBox_backupInterval.Text = GetTimeIntervalAppSetting();

            SetDarkSouls3SaveGameLocation();
            SetDarkSouls3BackupLocation();
        }


        /// <summary>
        /// If user moved the .exe without copying over the .exe.config file - recreate it with default values.
        /// </summary>
        private void CreateAppConfigFile()
        {
            string errorMessage = "DarkSouls3SaveGameBackupTool.exe.config doesn't exist."
                                + Environment.NewLine + "Please do not delete it. "
                                + Environment.NewLine + "Creating a new one...";

            CustomNotificationMessageBox(errorMessage);

            try
            {
                using (var appConfigFile = File.CreateText("DarkSouls3SaveGameBackupTool.exe.config"))
                {
                    appConfigFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    appConfigFile.WriteLine("<configuration>");
                    appConfigFile.WriteLine("    <startup>");
                    appConfigFile.WriteLine("        <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.6.1\" />");
                    appConfigFile.WriteLine("    </startup>");
                    appConfigFile.WriteLine("<appSettings>");
                    appConfigFile.WriteLine("    <add key=\"TimeInterval\" value=\"15\" />");
                    appConfigFile.WriteLine("    <add key=\"BackupLocation\" value=\"default\" />");
                    appConfigFile.WriteLine("</appSettings>");
                    appConfigFile.WriteLine("</configuration>");
                }
            }
            catch (Exception ex)
            {
                CustomErrorMessageBox(ex.ToString());
            }
        }



        /// <summary>
        /// Set on program start up and will be something like:
        ///     C:\Users\USER_NAME\AppData\Roaming\DarkSoulsIII\HEX_NUMBER_GARBAGE
        /// </summary>
        private void SetDarkSouls3SaveGameLocation()
        {
            try
            {
                string darkSoulsIIIBaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DarkSoulsIII\";

                var darkSoulIIISubFolders = Directory.GetDirectories(darkSoulsIIIBaseFolder);

                //The first directory should be the saves.
                //Not sure if person has multiple users/steam profiles then the directories would be more than 1 to host the gameID
                saveGameLocation = darkSoulIIISubFolders[0] + "\\";

                txtBox_darkSouls3SaveGameLocation.Text = saveGameLocation;
            }
            catch (Exception ex)
            {
                string errorMessage = "Error!Could not find the DarkSoulsIII folder.Looked for the DarkSoulsIII folder in: "
                                    + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                    + Environment.NewLine;

                CustomErrorMessageBox(errorMessage);
                CustomErrorMessageBox(ex.ToString());
                txtBox_darkSouls3SaveGameLocation.Text = "Error!";
            }

        }

        /// <summary>
        /// Set backup location. Defaults to save game directory.
        /// User defined location is stored in config
        /// </summary>
        private void SetDarkSouls3BackupLocation()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                string backupLocation = config.AppSettings.Settings["BackupLocation"].Value;

                if (backupLocation == "default" || !Directory.Exists(backupLocation))
                {
                    backupLocation = saveGameLocation;
                }

                saveBackupLocation = backupLocation;
            }
            catch (Exception ex)
            {
                string errorMessage = "Error! Looks like you deleted the DarkSouls3SaveGameBackupTool.exe.config file."
                                    + "Please make sure this file is in the same directory as DarkSouls3SaveGameBackupTool.exe.";

                CustomErrorMessageBox(errorMessage);
                CustomErrorMessageBox(ex.ToString());

                saveBackupLocation = saveGameLocation;
            }
            txtBox_darkSouls3BackupLocation.Text = saveBackupLocation;
        }


        /// <summary>
        /// Start creating back ups of the Dark Souls 3 save
        /// timeInterval setting is used for the dispatchTimer Tick interval
        /// </summary>
        private void btn_enableBackUpProcess_Click(object sender, RoutedEventArgs e)
        {
            txtBox_log.AppendText("Starting backup process. \t\t" + DateTime.Now.ToString() + Environment.NewLine);
            txtBox_log.AppendText("Creating a backup every " + GetTimeIntervalValue() + " minutes." + Environment.NewLine);


            //backupInterval in minutes
            int backupInterval = GetTimeIntervalValue();
            dispatcherTimer.Interval = new TimeSpan(0, backupInterval, 0);

            dispatcherTimer.Start();

            Enable_btnEndBackUpProcess();
            Disable_btnStartBackUpProcess();
            txtBox_backupInterval.IsEnabled = false;
            txtBox_log.ScrollToEnd();
        }


        /// <summary>
        /// Stop the dispatcherTimer to stop creating back ups
        /// </summary>
        private void btn_endBackUpProcess_Click(object sender, RoutedEventArgs e)
        {
            txtBox_log.AppendText(Environment.NewLine + "Stopped backup process. \t\t" + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine);


            dispatcherTimer.Stop();

            Enable_btnStartBackUpProcess();
            Disable_btnEndBackUpProcess();

            txtBox_backupInterval.IsEnabled = true;
            txtBox_log.ScrollToEnd();
        }



        //The following methods color and enable/disable the "Start" and "Stop" buttons
        #region Color and Enable/Disable Start and Stop Buttons
        private void Enable_btnStartBackUpProcess()
        {
            btn_startBackUpProcess.FontWeight = FontWeights.Bold;
            btn_startBackUpProcess.IsEnabled = true;
            btn_startBackUpProcess.Background = new SolidColorBrush(deepGray);
            btn_startBackUpProcess.Foreground = new SolidColorBrush(paleYellow);
        }
        private void Disable_btnStartBackUpProcess()
        {
            btn_startBackUpProcess.FontWeight = FontWeights.Normal;
            btn_startBackUpProcess.IsEnabled = false;
            btn_startBackUpProcess.Background = new SolidColorBrush(Colors.Gray);
            btn_startBackUpProcess.Foreground = new SolidColorBrush(Colors.LightGray);
        }
        private void Enable_btnEndBackUpProcess()
        {
            btn_endBackUpProcess.FontWeight = FontWeights.Bold;
            btn_endBackUpProcess.IsEnabled = true;
            btn_endBackUpProcess.Background = new SolidColorBrush(deepGray);
            btn_endBackUpProcess.Foreground = new SolidColorBrush(paleYellow);
        }
        private void Disable_btnEndBackUpProcess()
        {
            btn_endBackUpProcess.FontWeight = FontWeights.Normal;
            btn_endBackUpProcess.IsEnabled = false;
            btn_endBackUpProcess.Background = new SolidColorBrush(Colors.Gray);
            btn_endBackUpProcess.Foreground = new SolidColorBrush(Colors.LightGray);
        }
        #endregion




        /// <summary>
        /// Tick event that is triggered every timeInterval. Actual implementation to copy the game save to create the backup copy
        /// </summary>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //check for DarkSoulsIII process so that no back ups are made if the game isnt running
            //thanks Tenchuu on the NexusMods site for this code suggestion!
            Process[] dsIIIProcesses = Process.GetProcessesByName("DarkSoulsIII");

            //dsIIIProcesses == 0 means no DarkSoulsIII process is running
            if (dsIIIProcesses.Length == 0)
            {
                txtBox_log.AppendText("Dark Souls III is not running!" + Environment.NewLine
                                        + "\tSkipping backup creation: \t" + DateTime.Now.ToString() + Environment.NewLine);

                txtBox_log.ScrollToEnd();
                return;
            }

            //check if save file has been written to since last backup
            DateTime curSaveWriteTime = File.GetLastWriteTime(saveGameLocation + "DS30000.sl2");
            if (lastSaveWriteTime != null && lastSaveWriteTime == curSaveWriteTime)
            {
                txtBox_log.AppendText("Save file unchanged!" + Environment.NewLine
                                        + "\tSkipping backup creation: \t" + DateTime.Now.ToString() + Environment.NewLine);

                txtBox_log.ScrollToEnd();
                return;
            }

            lastSaveWriteTime = curSaveWriteTime; //update write time

            try
            {
                //human readable date for file backup - M/DD/YYYY 24H:MM
                string dateOfBackupForFileName = DateTime.Now.ToString("M/d/yyyy HH:mm");

                //Remove spaces, : and / from dateOfBackupForFileName and replace with underscore
                dateOfBackupForFileName = System.Text.RegularExpressions.Regex.Replace(dateOfBackupForFileName, @"[:|/|\s]", "_");

                File.Copy(saveGameLocation + "DS30000.sl2", saveBackupLocation + dateOfBackupForFileName + "__DS30000.sl2.bak");

                txtBox_log.AppendText("Created a new backup:\t\t" + DateTime.Now.ToString() + Environment.NewLine);
                txtBox_log.ScrollToEnd();
            }
            catch (Exception ex)
            {
                CustomErrorMessageBox(ex.Message);

                if (dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Stop();
                }

            }
        }


        /// <summary>
        /// Gets the time interval for creating a backup in minutes. Value has to be within 1 to 59.
        /// </summary>
        /// <returns>the interval - should be used as minutes so wont be greater than 59 and less than 1</returns>
        private int GetTimeIntervalValue()
        {
            int timeInterval = 0;

            try
            {
                timeInterval = Convert.ToInt32(txtBox_backupInterval.Text);

                if (timeInterval < 1)
                {
                    throw new Exception("Time interval cannot be less than 1 minute! Defaulting to 15 minutes...");
                }
                else if (timeInterval > 59)
                {
                    throw new Exception("Time interval cannot be more than 59 minutes! Defaulting to 15 minutes...");
                }

                return timeInterval;

            }
            catch (Exception ex)
            {
                //stop timer event
                CustomErrorMessageBox(ex.Message);

                if (dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Stop();
                }

                //default back to 15 minutes for time interval
                txtBox_backupInterval.Text = "15";

                return 15;
            }
        }



        /// <summary>
        /// A custom Error MessageBox that has an Error Icon
        /// </summary>
        /// <param name="errorMessage">error message to display</param>
        public void CustomErrorMessageBox(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }


        /// <summary>
        /// A custom Notification MessageBox with an info icon
        /// </summary>
        /// <param name="notificationMessage">notification message to display</param>
        public void CustomNotificationMessageBox(string notificationMessage)
        {
            MessageBox.Show(notificationMessage, "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        /// <summary>
        /// Open DarkSoulsIII savegame folder in Windows explorer
        /// </summary>
        private void btn_openDarkSouls3GameSavesLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(saveGameLocation + "\\");
        }


        /// <summary>
        /// Display help and about info. Like how to restore a save game.
        /// </summary>
        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            System.Text.StringBuilder message = new System.Text.StringBuilder();

            message.AppendLine("Dark Souls 3 Save Game Backup Tool v2.1");

            message.AppendLine("by Svaalbard");
            message.AppendLine();

            message.AppendLine("To restore a save game, either use the \"Restore a Save\" button, or do it manually by renaming the file from something like: ");
            message.AppendLine("\t5_2_2016_07_29__DS30000.sl2.bak");
            message.AppendLine("to:");
            message.AppendLine("\tDS30000.sl2");
            message.AppendLine();

            message.AppendLine("You might need to turn on file extensions in Windows to see the .bak file extension and remove it.");
            message.AppendLine();

            message.AppendLine("Google: \"Show or hide file name extensions\" for help on how to enable this.");

            CustomNotificationMessageBox(message.ToString());
        }


        /// <summary>
        /// Set/Save the Time Interval for saving from the UI to the app.config file.
        /// </summary>
        private void SaveTimeIntervalAppSetting()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                string timeIntervalSetting = GetTimeIntervalValue().ToString();

                config.AppSettings.Settings["TimeInterval"].Value = timeIntervalSetting;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                txtBox_log.AppendText("Saved Time Interval setting: " + timeIntervalSetting + "\t" + DateTime.Now.ToString() + Environment.NewLine);

            }
            catch (Exception ex)
            {
                string errorMessage = "Error! Looks like you deleted the DarkSouls3SaveGameBackupTool.exe.Config file.";
                errorMessage += "Please make sure this file is in the same directory as DarkSouls3SaveGameBackupTool.exe.";

                CustomErrorMessageBox(errorMessage);
                CustomErrorMessageBox(ex.ToString());
            }
        }

        /// <summary>
        /// Set/Save the backup location to the app.config file.
        /// </summary>
        private void SaveBackupLocationAppSetting()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings["BackupLocation"].Value = saveBackupLocation;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

            }
            catch (Exception ex)
            {
                string errorMessage = "Error! Looks like you deleted the DarkSouls3SaveGameBackupTool.exe.Config file.";
                errorMessage += "Please make sure this file is in the same directory as DarkSouls3SaveGameBackupTool.exe.";

                CustomErrorMessageBox(errorMessage);
                CustomErrorMessageBox(ex.ToString());
            }
        }


        /// <summary>
        /// Get the TimeInterval app.config setting
        /// Also set the txtBox_backupInterval Text with the TimeInterval app.config setting.
        /// </summary>
        private string GetTimeIntervalAppSetting()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                return config.AppSettings.Settings["TimeInterval"].Value;
            }
            catch (Exception ex)
            {
                string errorMessage = "Error! Looks like you deleted the DarkSouls3SaveGameBackupTool.exe.config file."
                                    + "Please make sure this file is in the same directory as DarkSouls3SaveGameBackupTool.exe.";

                CustomErrorMessageBox(errorMessage);
                CustomErrorMessageBox(ex.ToString());

                return "ERROR!";
            }
        }


        /// <summary>
        /// Save the TimeInterval setting to the app.config file
        /// </summary>
        private void btn_saveTimeInterval_Click(object sender, RoutedEventArgs e)
        {
            SaveTimeIntervalAppSetting();
        }


        /// <summary>
        /// Restore a .bak file to replace the DS30000.sl2 file.
        /// Maybe backup the DS30000.sl2 file into a "DeletedByRestoreSave" folder...
        /// </summary>
        private void btn_restoreSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            fileDialog.Filter = "Dark Souls 3 Save Game Backups (.bak)|*.bak";
            fileDialog.InitialDirectory = saveBackupLocation;

            bool? result = fileDialog.ShowDialog();

            //user selected a file to use as the backup
            if (result == true)
            {
                string backupChoiceFileName = fileDialog.FileName;

                string deleteConfirmationMessage = "Are you sure? This will DELETE DS30000.sl2 (your current save file) and replace it with: "
                                                 + Environment.NewLine + Path.GetFileName(backupChoiceFileName);

                MessageBoxResult messageBoxResult = MessageBox.Show(deleteConfirmationMessage, "Delete DS30000.sl2 Confirmation", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        //back up the original incase user error idea...
                        //Directory.CreateDirectory(saveGameLocation + "RestoredSaveBackups
                        //File.Copy(saveGameLocation + "DS30000.sl2", saveGameLocation + "\\RestoredSaveBackups\\" + "DeletedSave__DS30000.sl2.bak");

                        File.Delete(saveGameLocation + "DS30000.sl2");
                        txtBox_log.AppendText("Deleted DS30000.sl2...\t\t" + DateTime.Now.ToString() + Environment.NewLine);

                        File.Copy(backupChoiceFileName, saveGameLocation + "DS30000.sl2");
                        txtBox_log.AppendText("Created DS30000.sl2 from backup.\t" + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine);

                        CustomNotificationMessageBox("Back up has been successfully restored.");
                    }
                    catch (Exception ex)
                    {
                        CustomErrorMessageBox(ex.ToString());
                    }

                }

            }

        }

        /// <summary>
        /// Lets user choose a folder for backups. Saves to config file
        /// </summary>
        private void btn_chooseSaveBackupsLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();

            if (!String.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                saveBackupLocation = folderBrowser.SelectedPath + "\\";
                txtBox_darkSouls3BackupLocation.Text = saveBackupLocation;
                SaveBackupLocationAppSetting();
            }
        }

        /// <summary>
        /// opens backup location
        /// </summary>
        private void btn_openBackupLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(saveBackupLocation);
        }
        
        
        
        
    }
}
