using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Configuration;

namespace DarkSouls3SaveGameBackupTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Save game folder file path/location
        private string saveGameLocation = ""; 

        DispatcherTimer dispatcherTimer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
            btn_endBackUpProcess.IsEnabled = false;

            //avoid duplication of ticks
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            GetTimeIntervalAppSetting();

            SetDarkSouls3SaveGameLocation();
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
                CustomErrorMessageBox("Error! Could not find the DarkSoulsIII folder. Looked for the DarkSoulsIII folder in: " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                CustomErrorMessageBox(ex.ToString());
                txtBox_darkSouls3SaveGameLocation.Text = "Error!";
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private void btn_enableBackUpProcess_Click(object sender, RoutedEventArgs e)
        {
            txtBox_log.AppendText("Starting backup process." + Environment.NewLine);
            txtBox_log.AppendText("Creating a backup every " + GetTimeIntervalValue() + " minutes." + Environment.NewLine);


            //backupInterval in minutes
            int backupInterval = GetTimeIntervalValue();
            dispatcherTimer.Interval = new TimeSpan(0, backupInterval, 0);

            dispatcherTimer.Start();

            btn_startBackUpProcess.IsEnabled = false;
            btn_endBackUpProcess.IsEnabled = true;
        }


        /// <summary>
        /// Stop the dispatcherTimer to stop creating back ups
        /// </summary>
        private void btn_endBackUpProcess_Click(object sender, RoutedEventArgs e)
        {
            txtBox_log.AppendText(Environment.NewLine + "Stopped backup process..." + Environment.NewLine);
            txtBox_log.AppendText("-----------------------------------------------" + Environment.NewLine);


            dispatcherTimer.Stop();

            //???? need to find a way to stop it from duplicating
            btn_startBackUpProcess.IsEnabled = true;
            btn_endBackUpProcess.IsEnabled = false;

        }


        /// <summary>
        /// 
        /// </summary>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            txtBox_log.AppendText("Created a new backup: " + DateTime.Now.ToString() + Environment.NewLine);


            string dateOfBackupForFileName = DateTime.Now.ToString("M/d/yyyy HH:mm");

            //Remove spaces, : and / from dateOfBackupForFileName and replace with underscore
            dateOfBackupForFileName = System.Text.RegularExpressions.Regex.Replace(dateOfBackupForFileName, @"[:|/|\s]", "_");

            try
            {
                File.Copy(saveGameLocation + "DS30000.sl2", saveGameLocation + dateOfBackupForFileName + "__DS30000.sl2.bak");

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
                    throw new Exception("Time interval cannot be less than 1 minute!");
                }
                else if (timeInterval > 59)
                {
                    throw new Exception("Time interval cannot be more than 59 minutes!");
                }

                return timeInterval;

            }
            catch (Exception ex)
            {
                //stop timer event
                CustomErrorMessageBox(ex.Message);

                if(dispatcherTimer.IsEnabled)
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
            string message = "";

            message += "Dark Souls 3 Save Game Backup Tool v2.0";
            message += Environment.NewLine;
            message += "by Svaalbard";
            message += Environment.NewLine;
            message += Environment.NewLine;
            message += "To restore a save game, rename the file from something like: ";
            message += Environment.NewLine;
            message += "5_2_2016_07_29__DS30000.sl2.bak";
            message += Environment.NewLine;
            message += "to";
            message += Environment.NewLine;
            message += "DS30000.sl2";
            message += Environment.NewLine;
            message += Environment.NewLine;
            message += "You might need to turn on file extensions in Windows to see the .bak file extension and remove it.";
            message += Environment.NewLine;
            message += "Google: \"Show or hide file name extensions\" for help on how to enable this.";

            CustomNotificationMessageBox(message);
        }



        private void SaveTimeIntervalSetting()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings["TimeInterval"].Value = GetTimeIntervalValue().ToString();

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



        private void GetTimeIntervalAppSetting()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                txtBox_backupInterval.Text = config.AppSettings.Settings["TimeInterval"].Value;
            }
            catch (Exception ex)
            {
                string errorMessage = "Error! Looks like you deleted the DarkSouls3SaveGameBackupTool.exe.Config file.";
                errorMessage += "Please make sure this file is in the same directory as DarkSouls3SaveGameBackupTool.exe.";

                CustomErrorMessageBox(errorMessage);
                CustomErrorMessageBox(ex.ToString());
            }
        }



        private void btn_saveTimeInterval_Click(object sender, RoutedEventArgs e)
        {
            SaveTimeIntervalSetting();
        }
    }
}
