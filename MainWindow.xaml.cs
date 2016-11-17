using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Linq;
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
            txtBox_maxBackups.Text = GetMaxBackupsAppSetting();

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
                    appConfigFile.WriteLine("    <add key=\"MaxBackups\" value=\"10\" />");
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
                string errorMessage = "Error!Could not find the DarkSoulsIII folder. Looked for the DarkSoulsIII folder in: "
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
                string dateOfBackupForFileName = DateTime.Now.ToString("M_d_yyyy_HH_mm");

                File.Copy(saveGameLocation + "DS30000.sl2", saveBackupLocation + dateOfBackupForFileName + "__DS30000.sl2.bak");

                txtBox_log.AppendText("Created a new backup:\t\t" + DateTime.Now.ToString() + Environment.NewLine);
                DeleteOldBackup();
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
        /// Checks if there are more backups than the maximum allowed amount, and deletes the most
        /// recent if there is.
        /// </summary>
        /// <remarks>
        /// This does not retroactively delete backups exceeding the maximum if the maximum is lowered.
        /// E.g. if you have 10 backups and set the max to 5, it will not delete 5 backups the next time
        /// the timer fires, it will only delete one (the oldest).
        /// </remarks>
        private void DeleteOldBackup()
        {
            var maxBackups = GetMaxBackupsValue();

            if (maxBackups == 0)
                return; // 0 indicates no maximum

            var backupFiles = new DirectoryInfo(saveBackupLocation).EnumerateFiles("*.sl2.bak").ToList();

            if (backupFiles.Count > maxBackups)
            {
                var oldestBackup = backupFiles.OrderBy(fi => fi.CreationTime).First();
                oldestBackup.Delete();

                txtBox_log.AppendText($"Backups reached maximum; deleted oldest backup ({oldestBackup.Name}){Environment.NewLine}");
            }
        }


        /// <summary>
        /// Gets the time interval for creating a backup in minutes. Value has to be within 1 to 59.
        /// </summary>
        /// <returns>the interval - should be used as minutes so wont be greater than 59 and less than 1</returns>
        private int GetTimeIntervalValue()
        {
            int timeInterval = 0;

            if (Int32.TryParse(txtBox_backupInterval.Text, out timeInterval))
            {
                if (timeInterval < 1)
                {
                    txtBox_log.AppendText("Time interval cannot be less than 1 minute! Defaulting to 15 minutes...");
                    return ResetTimerInterval();
                }
                else if (timeInterval > 59)
                {
                    txtBox_log.AppendText("Time interval cannot be more than 59 minutes! Defaulting to 15 minutes...");
                    return ResetTimerInterval();
                }

                return timeInterval;
            }

            return ResetTimerInterval();
        }

        private int GetMaxBackupsValue()
        {
            int maxBackups = 0;

            if (Int32.TryParse(txtBox_maxBackups.Text, out maxBackups))
            {
                if (maxBackups < 0)
                {
                    txtBox_maxBackups.Text = "0";
                    return 0;
                }

                return maxBackups;
            }

            txtBox_maxBackups.Text = "10";
            return 10;
        }

        /// <summary>
        /// Resets the timer interval and stops the timer, for error correction purposes.
        /// </summary>
        /// <returns></returns>
        private int ResetTimerInterval()
        {
            //stop timer event
            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }

            //default back to 15 minutes for time interval
            txtBox_backupInterval.Text = "15";
            return 15;
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

        private void SaveMaxBackupsAppSetting()
        {
            TrySaveConfigValue("MaxBackups", GetMaxBackupsValue().ToString());
        }

        /// <summary>
        /// Set/Save the Time Interval for saving from the UI to the app.config file.
        /// </summary>
        private void SaveTimeIntervalAppSetting()
        {
            TrySaveConfigValue("TimeInterval", GetTimeIntervalValue().ToString());
        }

        /// <summary>
        /// Set/Save the backup location to the app.config file.
        /// </summary>
        private void SaveBackupLocationAppSetting()
        {
            TrySaveConfigValue("BackupLocation", saveBackupLocation);
        }


        /// <summary>
        /// Get the TimeInterval app.config setting
        /// Also set the txtBox_backupInterval Text with the TimeInterval app.config setting.
        /// </summary>
        private string GetTimeIntervalAppSetting()
        {
            return TryGetConfigValue("TimeInterval");
        }

        private string GetMaxBackupsAppSetting()
        {
            return TryGetConfigValue("MaxBackups");
        }

        /// <summary>
        /// Attempts to save a value to the configuration file using the provided key.
        /// Does not throw if the value cannot be saved.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void TrySaveConfigValue(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings[key].Value = value;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                txtBox_log.AppendText($"Saved {key} setting: {value}\t{DateTime.Now}{Environment.NewLine}");
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
        /// Attempts to read the configuration value for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value if it exists and can be read, otherwise "ERROR!"</returns>
        private string TryGetConfigValue(string key)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                return config.AppSettings.Settings[key].Value;
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
        /// Save the TimeInterval and MaxBackups settings to the app.config file
        /// </summary>
        private void btn_saveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveTimeIntervalAppSetting();
            SaveMaxBackupsAppSetting();
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
