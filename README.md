# DarkSouls3SaveGameBackupTool

A program that creates backups of save games of Dark Souls 3.

Back ups are created in the following format (hours in 24HR military time format):

`Month_Day_Year_Hour_Minute__DS30000.sl2.bak`

Like so: `5_2_2016_10_03__DS30000.sl2.bak`

To restore a backup, delete your `DS30000.sl2` file and rename the backup of your choice to `DS30000.sl2`.

Please note that you will start the backup process by pressing the "Start" button. Then start up Dark Souls 3 and play. When you are done playing, do __not__ forget to press the Stop button or close the DarkSouls3SaveGameBackupTool program. Otherwise, it will continually copy every time interval until stopped.

### [Download the latest release - v2](https://github.com/insane0hflex/DarkSouls3SaveGameBackupTool/blob/master/Releases/DarkSouls3SaveGameBackupTool.exe?raw=true)

__NOTE:__ Upon launching this program, if you are on Windows 10 - you might get a "security" warning. Press the "More info" button then press "Run anyway".

[VirusTotal report for DarkSouls3SaveGameBackupTool](https://www.virustotal.com/en/file/69fefd118f30edc858810287587a849eac9cba94c9772c03959d753540f377d7/analysis/1462063233/)

Please ignore the Qihoo360 false positive - this antivirus seems to dislike any C#/.NET 4.0+ project or something, which this program was created with.

![DarkSouls3SaveGameBackupTool](https://github.com/insane0hflex/DarkSouls3SaveGameBackupTool/blob/master/exampleImage.jpg)

### ToDos
- ~~Store TimeInterval setting for user~~
- ~~Make back up file name built with a human readable date rather than .ToFileTime()~~
- Prettify the UI
- ~~Better code comments~~
- Dark souls 2 support? 
- Keep a "MaxBackup" amount - like 10 - where older backups above 10 are automatically deleted.
- Create a check for the `.exe.config` file and create it if it doesn't exist

### Build Instructions

Want to build the source for this?

Download the source then simply open the solution file (you will need Visual Studio installed, at least .NET Framework 4.0 installed) and build. You can get [Visual Studio Community](https://www.visualstudio.com/en-us/visual-studio-homepage-vs.aspx) for free.
