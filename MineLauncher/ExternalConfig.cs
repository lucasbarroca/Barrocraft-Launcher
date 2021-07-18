using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MineLauncher
{
    class ExternalConfig
    {
        [JsonConverter(typeof(VersionConverter))]
        public Version LauncherVersion = new Version("1.0.0.0");
        [JsonConverter(typeof(VersionConverter))]
        public Version BaseGamePatchVersion = new Version("1.0.0.0");
        [JsonConverter(typeof(VersionConverter))]
        public Version ModsPatchVersion = new Version("1.0.0.0");      
        [JsonConverter(typeof(VersionConverter))]
        public Version ConfigsPatchVersion = new Version("1.0.0.0");
        [JsonConverter(typeof(VersionConverter))]
        public Version GameOptionsPatchVersion = new Version("1.0.0.0");

        public string LauncherDataDir = "%appdata%/.barrocraft";
        public string GameInstallDir = "Barrocraft";
        public string PasswordFileName = "{gameDir}/.sl_password";
        public string[] AvailableCultures = { "en", "pt" };
        public string DefaultCulture = "en";

        public GameFiles Files = new GameFiles();

        public bool ShowMessage = false;
        public MaintenanceMessage Message = new MaintenanceMessage();
    }

    class GameFiles
    {
        public string[] LauncherUpdateUrl = { "{ServerUrl}/newLauncherUrl.zip", "LauncherFileName.exe" };
        public string TLauncherBaseGame = "{ServerUrl}/BaseGameTLauncher.zip";
        public string TLauncherLauncher = "{ServerUrl}/LauncherTLauncher.zip";
        public string ConfigFiles = "{ServerUrl}/config.zip";
        public string LowOptions = "{ServerUrl}/options/optionsLow.{Culture}.zip";
        public string MediumOptions = "{ServerUrl}/options/optionsMedium.{Culture}.zip";
        public string HighOptions= "{ServerUrl}/options/optionsHigh.{Culture}.zip";
        public string[][] Mods = new string[][]
        {
            new string[] {"{ServerUrl}/modFile.zip", "localModFileToVerify.jar", "md5FileHashHere" },
            new string[] { "{ServerUrl}/anotherModFile.jar", "anotherLocalModFileToVerify.jar", "md5FileHashHere" }
        };

        public string[] PathsToDelete = new string[]
        {
            "{gameDir}/exampleFolderToDelete",
            "{modsDir}/obsoleteModToDelete.jar",
            "{gameDir}/obsoleteFileToDelete.example"
        };
    }

    class MaintenanceMessage
    {
        public bool CloseLauncher = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public MessageBoxButtons Buttons = MessageBoxButtons.OK;
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageBoxIcon Icon = MessageBoxIcon.Information;

        public MaintenanceMessageText[] Text = { new MaintenanceMessageText() };
    }

    class MaintenanceMessageText
    {
        public string Culture = "default";
        public string Text = "Server is under maintenance.";
        public string Caption = "Maintenance";
    }
}
