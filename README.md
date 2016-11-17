# Dark Souls 3 Save Game Backup Tool

A program that creates backups of your Dark Souls 3 save every 15 minutes by default. Can customize the back up time interval to be between 1 to 59 minutes.

Back ups are created in the following format (hours in 24HR military time format):

`Month_Day_Year_Hour_Minute__DS30000.sl2.bak`

Like so: `5_2_2016_10_03__DS30000.sl2.bak`

To restore a backup, either use the "Restore a Save" feature or delete your `DS30000.sl2` file and rename the backup of your choice to `DS30000.sl2`.

Please note that you will start the backup process by pressing the "Start" button. Then start up Dark Souls 3 and play.

When you are done playing, press the "Stop" button. The program will continue to try to create back ups until you close the program or press the "Stop" button. However, if Dark Souls 3 is not running, then no backup is made.

### [Download Dark Souls 3 Save Game Backup Tool](http://www.nexusmods.com/darksouls3/mods/16?)

#### [Virus Total Report for Dark Souls 3 Save Game Backup Tool 5/5/2016](https://www.virustotal.com/en/file/7cd31e90694bbe896272e5c81eca0762cb5f860ad926077c3673ba2e3a4a2253/analysis/1462494364/)

Important note: Do not delete or remove the `DarkSouls3SaveGameBackupTool.exe.config` file. This stores the time interval setting. It must be in the same folder as the `DarkSouls3SaveGameBackupTool.exe` file.

![DarkSouls3SaveGameBackupTool](https://github.com/insane0hflex/DarkSouls3SaveGameBackupTool/blob/master/Images/example.png)

### ToDos
- Key to create a back up manually (think of like an F5 to "quick save" of sorts
- ~~Keep a "MaxBackup" amount - like 10 - where older backups above 10 are automatically deleted.~~
- By Lakon: ~~Add a specific folder for backup locations to save the back ups too~~
- ~~Store TimeInterval setting for user~~
- ~~Make back up file name built with a human readable date rather than .ToFileTime()~~
- ~~Prettify the UI~~
- ~~Better code comments~~
- ~~Create a check for the `.exe.config` file and create it if it doesn't exist~~
- ~~Restore save from backup feature (instead of user manually deleting the old save and renaming a backup to `DS30000.sl2`~~


### Build Instructions

Want to build from source?

Download this repo then open the solution file (you will need Visual Studio installed with at least .NET Framework 4.0 installed) and build. You can get [Visual Studio Community](https://www.visualstudio.com/en-us/visual-studio-homepage-vs.aspx) for free. Once you build, the binary (.exe file) is located in either the /bin/Debug/ folder or the /bin/Release/ folder.


