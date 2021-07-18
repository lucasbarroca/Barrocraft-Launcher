using MineLauncher.Language;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineLauncher
{
    static class Program
    {
        static string LauncherConfigFilename = "launcher.json";
        public static string LauncherGetServerUrl = "https://mine.helparte.com/updateurl.json"; // this file must return a JSON with the LauncherServerUrl
        public static string LauncherServerUrl; // This will be returned by the above url

        public static string LangDomain = "Program";
        public static Config Config;
        public static ExternalConfig ServerConfig;

        public static bool InitOk = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Setup translation according to current language
            Lang.SetLanguage(Thread.CurrentThread.CurrentUICulture);
            //Lang.SetLanguage(new CultureInfo("pt"));

            // Load "Program" strings
            Lang.LoadLanguageDomain(LangDomain);

            // Show splash screen
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new FrmInit());

            // Show main form (only if the initialization are not cancelled by user)
            if (InitOk)
            {
                Application.Run(new FrmMain());
            }
        }

        /// <summary>
        /// Load local configuration file
        /// </summary>
        public static void LoadConfig()
        {
            var configFile = Path.Combine(ServerConfig.LauncherDataDir, LauncherConfigFilename);

            if (File.Exists(configFile))
            {
                var jsonData = File.ReadAllText(configFile);
                Config = JsonConvert.DeserializeObject<Config>(jsonData);
            }
            else
            {
                Config = new Config();
                SaveConfig();
            }
        }

        /// <summary>
        /// Save local configuration file
        /// </summary>
        public static void SaveConfig()
        {
            var configFile = Path.Combine(ServerConfig.LauncherDataDir, LauncherConfigFilename);

            try
            {
                // Remove launcher "available" from config
                var jsonObj = JObject.FromObject(Config);
                for (int i = 0; i < Config.Launchers.Length; i++)
                {
                    jsonObj["Launchers"][i]["Available"].Parent.Remove();
                }

                // Save config
                var jsonData = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(configFile, jsonData);
            }
            catch { }
        }

        /// <summary>
        /// Zip file extraction with overwrite option
        /// </summary>
        /// <param name="sourceZipFilePath">Zip file path</param>
        /// <param name="destinationDirectoryName">Destination folder to extract files</param>
        /// <param name="overwrite">Overwrite existing files when extracting</param>
        public static void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryName, bool overwrite)
        {
            using (var archive = ZipFile.Open(sourceZipFilePath, ZipArchiveMode.Read))
            {
                if (!overwrite)
                {
                    archive.ExtractToDirectory(destinationDirectoryName);
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
                string destinationDirectoryFullPath = di.FullName;

                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                    if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                    }

                    if (file.Name == "")
                    {// Assuming Empty for Directory
                        Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        continue;
                    }
                    file.ExtractToFile(completeFileName, true);
                }
            }
        }
    }
}
