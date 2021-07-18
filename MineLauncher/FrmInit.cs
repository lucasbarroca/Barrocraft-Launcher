using MineLauncher.Language;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineLauncher
{
    public partial class FrmInit : Form
    {
        private string LangDomain = "FrmInit";
        public static string LocalOptionsSavePath = "options";

        public FrmInit()
        {
            InitializeComponent();

            // Load form language domain
            Lang.LoadLanguageDomain(LangDomain);

            // Events
            this.Shown += FrmInit_Shown;

            // Loading message
            lblMessage.Text = Lang.__("LauncherLoading", LangDomain);
        }

        private void frmInit_Load(object sender, EventArgs e)
        {

        }
        
        private async void FrmInit_Shown(object sender, EventArgs ea)
        {
            Application.DoEvents();

            // Load external data from server
            try
            {
                // Load LauncherServerUrl
                using (WebClient wcLauncherUrl = new WebClient())
                {
                    string jsonData = wcLauncherUrl.DownloadString(Program.LauncherGetServerUrl);
                    Program.LauncherServerUrl = JObject.Parse(jsonData)["LauncherServerUrl"].ToString();
                    //Program.LauncherServerUrl = "http://localhost/barrocraft/update";
                }

                // Ensure correct server url format (without "/" at the end)
                if (Program.LauncherServerUrl.EndsWith("/"))
                {
                    Program.LauncherServerUrl = Program.LauncherServerUrl.Substring(0, Program.LauncherServerUrl.Length - 1);
                }

                // Load launcher data
                using (WebClient wcLauncherData = new WebClient())
                {
                    string jsonData = wcLauncherData.DownloadString("{ServerUrl}/launcher.json".Replace("{ServerUrl}", Program.LauncherServerUrl));
                    Program.ServerConfig = JsonConvert.DeserializeObject<ExternalConfig>(jsonData);
                }

                // Setup launcher data directory
                Program.ServerConfig.LauncherDataDir = Program.ServerConfig.LauncherDataDir.Replace("%appdata%",
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

                // Prepare urls and paths
                Program.ServerConfig.Files.LauncherUpdateUrl[0] = Program.ServerConfig.Files.LauncherUpdateUrl[0].Replace("{ServerUrl}", Program.LauncherServerUrl);
                Program.ServerConfig.Files.TLauncherBaseGame = Program.ServerConfig.Files.TLauncherBaseGame.Replace("{ServerUrl}", Program.LauncherServerUrl);
                Program.ServerConfig.Files.TLauncherLauncher = Program.ServerConfig.Files.TLauncherLauncher.Replace("{ServerUrl}", Program.LauncherServerUrl);
                Program.ServerConfig.Files.ConfigFiles = Program.ServerConfig.Files.ConfigFiles.Replace("{ServerUrl}", Program.LauncherServerUrl);
                
                Program.ServerConfig.Files.LowOptions = Program.ServerConfig.Files.LowOptions.Replace("{ServerUrl}", Program.LauncherServerUrl);
                Program.ServerConfig.Files.MediumOptions = Program.ServerConfig.Files.MediumOptions.Replace("{ServerUrl}", Program.LauncherServerUrl);
                Program.ServerConfig.Files.HighOptions = Program.ServerConfig.Files.HighOptions.Replace("{ServerUrl}", Program.LauncherServerUrl);

                foreach (string[] mod in Program.ServerConfig.Files.Mods)
                {
                    mod[0] = mod[0].Replace("{ServerUrl}", Program.LauncherServerUrl);
                }

                LocalOptionsSavePath = Path.Combine(Program.ServerConfig.LauncherDataDir, LocalOptionsSavePath);
            }
            catch (Exception e)
            {
                ShowStatusError(Lang.__("ExternalConfigError", LangDomain), e);
                MessageBox.Show(Lang.__("ExternalConfigError", LangDomain), Lang.__("MsgBoxErrorCaption", Program.LangDomain), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            // Maintenance mode
            if (Program.ServerConfig.ShowMessage)
            {
                // Default to english if no translation found
                var langId = 0;
                for (int i = 0; i < Program.ServerConfig.Message.Text.Length; i++)
                {
                    if (Program.ServerConfig.Message.Text[i].Culture == Lang.Culture.TwoLetterISOLanguageName)
                    {
                        langId = i;
                    }
                }

                // Show the message
                MessageBox.Show(
                    Program.ServerConfig.Message.Text[langId].Text, Program.ServerConfig.Message.Text[langId].Caption,
                    Program.ServerConfig.Message.Buttons, Program.ServerConfig.Message.Icon);

                if (Program.ServerConfig.Message.CloseLauncher)
                {
                    Environment.Exit(0);
                }
            }

            // Process local configuration stuff
            try
            {
                // Create local launcher data dir
                Directory.CreateDirectory(Program.ServerConfig.LauncherDataDir);

                // Load config if exists or create new
                Program.LoadConfig();

                // Check game options files
                Directory.CreateDirectory(LocalOptionsSavePath);

                await CheckOptionsFile(Program.ServerConfig.Files.LowOptions);
                await CheckOptionsFile(Program.ServerConfig.Files.MediumOptions);
                await CheckOptionsFile(Program.ServerConfig.Files.HighOptions);
            }
            catch (Exception e)
            {
                ShowStatusError(Lang.__("InitializationError", LangDomain), e);
                MessageBox.Show(Lang.__("InitializationError", LangDomain), Lang.__("MsgBoxErrorCaption", Program.LangDomain), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            // Check launchers availability and game installation
            foreach (var launcher in Program.Config.Launchers)
            {
                launcher.Available = IsLauncherAvailable(launcher.Launcher);
            }

            // Default TLauncher when no launcher available
            if (!Program.Config.Launchers[(int)Program.Config.LastSelectedLauncher].Available)
            {
                if (Program.Config.Launchers[(int)MinecraftLauncher.Minecraft].Available)
                {
                    Program.Config.LastSelectedLauncher = MinecraftLauncher.Minecraft;
                }
                else
                {
                    Program.Config.LastSelectedLauncher = MinecraftLauncher.TLauncher;
                }
            }

            // New launcher version found!
            if (Program.ServerConfig.LauncherVersion.CompareTo(Program.Config.CurrentLauncherVersion) > 0)
            {
                ShowStatus(Lang.__("LauncherUpdating", LangDomain));

                try
                {
                    var newLauncherUrl = Program.ServerConfig.Files.LauncherUpdateUrl[0];
                    var newLauncherFilename = Path.GetFileName(newLauncherUrl);
                    var newLauncherExeName = Program.ServerConfig.Files.LauncherUpdateUrl[1];
                    var currentLauncherFullFilename = Process.GetCurrentProcess().MainModule.FileName;
                    var currentLauncherFilename = Path.GetFileName(currentLauncherFullFilename);
                    var tempSavePath = Path.Combine(Program.ServerConfig.LauncherDataDir, "update");
                    var oldLauncherTempFolder = Path.Combine(tempSavePath, "old");
                    var newLauncherFullFilename = Path.Combine(tempSavePath, newLauncherFilename);
                    var newLauncherFullExeName = Path.Combine(tempSavePath, newLauncherExeName);

                    Directory.CreateDirectory(oldLauncherTempFolder);

                    Console.WriteLine(newLauncherFullFilename);
                    using (WebClient wcLauncherDownloader = new WebClient())
                    {
                        await wcLauncherDownloader.DownloadFileTaskAsync(newLauncherUrl, newLauncherFullFilename);
                    }

                    // Extract zip (if needed)
                    if (Path.GetExtension(newLauncherFilename) == ".zip")
                    {
                        Program.ExtractZipFileToDirectory(newLauncherFullFilename, tempSavePath, true);
                        File.Delete(newLauncherFullFilename);
                    }

                    // Move current launcher
                    if (File.Exists(Path.Combine(oldLauncherTempFolder, currentLauncherFilename)))
                    {
                        File.Delete(Path.Combine(oldLauncherTempFolder, currentLauncherFilename));
                    }

                    File.Move(currentLauncherFullFilename, Path.Combine(oldLauncherTempFolder, currentLauncherFilename));

                    // Bring the new launcher in place
                    File.Move(newLauncherFullExeName, currentLauncherFullFilename);

                    // Save new version
                    Program.Config.CurrentLauncherVersion = Program.ServerConfig.LauncherVersion;
                    Program.SaveConfig();

                    // Restart!
                    ShowStatus(Lang.__("RestartingLauncher", LangDomain));
                    await Task.Delay(2000);
                    Process.Start(currentLauncherFullFilename);
                    Environment.Exit(0);
                }
                catch (Exception e)
                {
                    ShowStatusError(Lang.__("LauncherUpdateError", LangDomain), e);
                    MessageBox.Show(Lang.__("LauncherUpdateError", LangDomain), Lang.__("MsgBoxErrorCaption", Program.LangDomain), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }

            // Everything ok! let's go next!
            await Task.Delay(1000);
            Program.InitOk = true;
            Close();
        }

        /// <summary>
        /// Download file containing game options files (if needed) and save to launcher data directory for later usage
        /// </summary>
        /// <param name="optionsFileUrl">File url to check and download</param>
        /// <returns></returns>
        private async Task CheckOptionsFile(string optionsFileUrl)
        {
            // Culture setup
            var fileUrl = optionsFileUrl.Replace("{Culture}", Program.ServerConfig.DefaultCulture);

            if (Program.ServerConfig.AvailableCultures.Contains(Lang.Culture.TwoLetterISOLanguageName))
            {
                fileUrl = optionsFileUrl.Replace("{Culture}", Lang.Culture.TwoLetterISOLanguageName);
            }

            var fileName = Path.GetFileName(fileUrl);
            var localFilename = Path.Combine(LocalOptionsSavePath, fileName);

            // Check if file exists or needs update
            var needUpdate = false;
            foreach(var launcher in Program.Config.Launchers)
            {
                if (Program.ServerConfig.GameOptionsPatchVersion.CompareTo(
                    launcher.CurrentGameOptionsPatch) > 0)
                {
                    needUpdate = true;
                }
            }

            if (!File.Exists(localFilename) || needUpdate)
            {
                // Download file to local dir
                using (WebClient wcOptionsDownloader = new WebClient())
                {
                    await wcOptionsDownloader.DownloadFileTaskAsync(fileUrl, localFilename);
                }
            }
        }

        /// <summary>
        /// Check for given launcher availability
        /// </summary>
        /// <param name="launcher">Minecraft launcher to check availability</param>
        /// <returns>True if launcher config folders are found in system</returns>
        static bool IsLauncherAvailable(MinecraftLauncher launcher)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            switch (launcher)
            {
                case MinecraftLauncher.Minecraft:
                    {
                        return false;
                    }

                case MinecraftLauncher.TLauncher:
                    {
                        var tlauncherPath = Path.Combine(appData, ".tlauncher");

                        if (Directory.Exists(tlauncherPath))
                        {
                            var tlauncherProperties = Path.Combine(tlauncherPath, "tlauncher-2.0.properties");

                            if (File.Exists(tlauncherProperties))
                            {
                                try
                                {
                                    // Load tlauncher properties file and check for minecraft directory
                                    var data = new Dictionary<string, string>();
                                    foreach (var row in File.ReadAllLines(tlauncherProperties))
                                    {
                                        data.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));
                                    }

                                    var gameDir = Regex.Unescape(data["minecraft.gamedir"]);
                                    if (Directory.Exists(gameDir))
                                    {
                                        return true;
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show(Lang.__("TLauncherIsOpenError", Program.LangDomain), Lang.__("MsgBoxErrorCaption", Program.LangDomain), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    Environment.Exit(0);
                                }
                            }
                        }

                        return false;
                    }

                default:
                    return false;
            }
        }

        #region "Form Controls Functions"
        /// <summary>
        /// Show a status message in form
        /// </summary>
        /// <param name="message">Message to show</param>
        private void ShowStatus(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = SystemColors.ControlText;

            Console.WriteLine(message);
        }

        /// <summary>
        /// Show a red status message in form
        /// </summary>
        /// <param name="errorMessage">Message to show</param>
        /// <param name="e">Exception to save for support</param>
        private void ShowStatusError(string errorMessage, Exception e)
        {
            lblMessage.Text = errorMessage;
            lblMessage.ForeColor = Color.Red;

            Console.WriteLine(errorMessage);
        }
        #endregion
    }
}
