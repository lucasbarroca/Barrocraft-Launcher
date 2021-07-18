using MineLauncher.Language;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineLauncher
{
    public partial class FrmMain : Form
    {
        private string LangDomain = "FrmMain";
        public static bool NeedOptionsUpdate = false;
        //private bool newsLoaded = false;

        public FrmMain()
        {
            InitializeComponent();

            // Setup events
            cmbLaunchers.SelectedIndexChanged += CmbLaunchers_SelectedIndexChanged;
            chkForceUpdate.CheckedChanged += ChkForceUpdate_CheckedChanged;
            wbNews.DocumentCompleted += WbNews_DocumentCompleted;
            Shown += FrmMain_Shown;

            // Load form language domain
            Lang.LoadLanguageDomain(LangDomain);

            // Translate things
            Text = Lang.__("Text", LangDomain);
            chkForceUpdate.Text = Lang.__("ForceUpdateText", LangDomain);
            lblStatus.Text = "";
            pbStatus.Value = 0;

            // Populate graphics list
            lblOptions.Text = Lang.__("GameOptionsText", LangDomain);
            cmbOptions.Items.Add(Lang.__("GameOptionsLowText", LangDomain));
            cmbOptions.Items.Add(Lang.__("GameOptionsMediumText", LangDomain));
            cmbOptions.Items.Add(Lang.__("GameOptionsHighText", LangDomain));

            // Populate launcher list and check game installs
            foreach (var launcher in Program.Config.Launchers)
            {
                if (launcher.GameInstalled && !GameInstallationFound(launcher.Launcher))
                {
                    launcher.GameInstalled = false;
                }

                cmbLaunchers.Items.Add(Lang.__($"LauncherName{launcher.Launcher.ToString()}", LangDomain));
            }
            cmbLaunchers.SelectedIndex = (int)Program.Config.LastSelectedLauncher;

            // Other setups
            oFileLauncherExe.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void WbNews_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //newsLoaded = true;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            
        }

        private async void FrmMain_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            await Task.Delay(100);

            try
            {
                var newsUrl = "";
                var culture = Program.ServerConfig.DefaultCulture;

                if (Program.ServerConfig.AvailableCultures.Contains(Lang.Culture.TwoLetterISOLanguageName))
                {
                    culture = Lang.Culture.TwoLetterISOLanguageName;
                }

                using (WebClient wcLauncherUrl = new WebClient())
                {
                    string jsonData = wcLauncherUrl.DownloadString("{ServerUrl}/news.json".Replace("{ServerUrl}", Program.LauncherServerUrl));
                    newsUrl = JObject.Parse(jsonData)[$"NewsUrl.{culture}"].ToString();
                }

                wbNews.Navigate(newsUrl);
            }
            catch { }
        }

        private async void PlayGame()
        {
            var selectedLauncher = (MinecraftLauncher)cmbLaunchers.SelectedIndex;

            btPlay.Text = Lang.__("GameStartingText", LangDomain);
            ShowStatus(Lang.__("StatusPreparingPlay", LangDomain));

            // Try to restore password before start
            RestorePasswordFile(selectedLauncher);

            await Task.Delay(1);

            // Setup launcher
            switch (selectedLauncher)
            {
                case MinecraftLauncher.Minecraft:
                    break;

                case MinecraftLauncher.TLauncher:
                    {
                        // Make installation as selected
                        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        var tlauncherPath = Path.Combine(appData, ".tlauncher");

                        if (Directory.Exists(tlauncherPath))
                        {
                            var tlauncherProperties = Path.Combine(tlauncherPath, "tlauncher-2.0.properties");

                            if (File.Exists(tlauncherProperties))
                            {
                                try
                                {
                                    // Load tlauncher properties file and set selected game install
                                    var lines = File.ReadAllLines(tlauncherProperties);
                                    for (int i = 0; i<lines.Length; i++)
                                    {
                                        var property = lines[i].Split('=')[0];

                                        if (property == "login.version.game")
                                        {
                                            lines[i] = $"{property}={Program.ServerConfig.GameInstallDir}";
                                            break;
                                        }
                                    }

                                    File.WriteAllLines(tlauncherProperties, lines);

                                    // Find tlauncher executable if needed
                                    var tlauncherExe = Program.Config.Launchers[(int)selectedLauncher].ExecutablePath;
                                    if (string.IsNullOrEmpty(tlauncherExe))
                                    {
                                        // Prompt user select launcher dialog
                                        oFileLauncherExe.Title = Lang.__("SelectLauncherExeText", LangDomain).Replace("%s", Lang.__($"LauncherName{selectedLauncher}", LangDomain));
                                        oFileLauncherExe.FileName = "TLauncher.jar";
                                        oFileLauncherExe.Filter = "TLauncher|*.jar";

                                        if (oFileLauncherExe.ShowDialog() == DialogResult.OK && File.Exists(oFileLauncherExe.FileName))
                                        {
                                            tlauncherExe = oFileLauncherExe.FileName;
                                            Program.Config.Launchers[(int)selectedLauncher].ExecutablePath = tlauncherExe;
                                        }
                                        else
                                        {
                                            ShowStatusError(Lang.__("CancelLauncherExeText", LangDomain), new FileNotFoundException());
                                            EnableControls();
                                            RefreshControls(false);
                                            return;
                                        }
                                    }

                                    // Change game options only if needed
                                    var selectedGameOptions = (GameOptions)cmbOptions.SelectedIndex;

                                    if (selectedGameOptions != Program.Config.Launchers[(int)selectedLauncher].SelectedGameOptions)
                                    {
                                        SetGameOptions(selectedGameOptions, selectedLauncher);
                                    }

                                    // Save options for later usage
                                    Program.Config.LastSelectedLauncher = selectedLauncher;
                                    Program.SaveConfig();

                                    // Start launcher
                                    Process.Start(tlauncherExe);

                                    // All up! bye!
                                    Close();
                                }
                                catch (Exception e)
                                {
                                    ShowStatusError(Lang.__("TLauncherIsOpenError", Program.LangDomain), e);
                                    EnableControls();
                                    RefreshControls(false);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        #region "Update Functions"
        /// <summary>
        /// Install full base game with mods, configs and etc
        /// </summary>
        private async Task InstallGame(bool createBackup, bool forceUpdate)
        {
            var selectedLauncher = (MinecraftLauncher)cmbLaunchers.SelectedIndex;

            // Setup directories
            var minecraftDir = GetDotMinecraftDir(selectedLauncher);
            if (Directory.Exists(minecraftDir))
            {
                var gameDir = minecraftDir;
                if (selectedLauncher == MinecraftLauncher.TLauncher)
                {
                    gameDir = Path.Combine(minecraftDir, $"versions/{Program.ServerConfig.GameInstallDir}");
                }

                var modsDir = Path.Combine(gameDir, "mods");

                // backup folder if exists
                if (createBackup && Directory.Exists(gameDir))
                {
                    try
                    {
                        ShowStatus(Lang.__("StatusBackupCreation", LangDomain));
                        BackupFolder(gameDir);
                    }
                    catch (Exception e)
                    {
                        ShowStatusError(Lang.__("StatusBackupError", LangDomain), e);

                        // Refresh form
                        EnableControls();
                        RefreshControls(false);

                        return;

                    }
                }

                try
                {
                    // Create folder if needed
                    Directory.CreateDirectory(gameDir);

                    // Install base game (forge, etc)
                    if (forceUpdate || Program.ServerConfig.BaseGamePatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentBaseGamePatch) > 0)
                    {
                        switch (selectedLauncher)
                        {
                            case MinecraftLauncher.Minecraft:
                                break;

                            case MinecraftLauncher.TLauncher:
                                {
                                    var baseGameUrl = Program.ServerConfig.Files.TLauncherBaseGame;
                                    var baseGameFilename = Path.GetFileName(baseGameUrl);
                                    var baseGameFullFilename = Path.Combine(gameDir, baseGameFilename);

                                    try
                                    {
                                        using (WebClient wcBaseGameDownloader = new WebClient())
                                        {
                                            ShowStatus(Lang.__("StatusDownloadingFile", LangDomain).Replace("%s", baseGameFilename));
                                            await wcBaseGameDownloader.DownloadFileTaskAsync(baseGameUrl, Path.Combine(gameDir, baseGameFilename));
                                        }

                                        ExtractZipIfNeeded(baseGameFullFilename, gameDir);

                                        // Base game updated!
                                        Program.Config.Launchers[(int)selectedLauncher].CurrentBaseGamePatch = Program.ServerConfig.BaseGamePatchVersion;
                                    }
                                    catch (Exception e)
                                    {
                                        ShowStatusError(Lang.__("StatusDownloadError", LangDomain).Replace("%s", baseGameFilename), e);

                                        // Refresh form
                                        EnableControls();
                                        RefreshControls(false);

                                        return;
                                    }
                                }
                                break;
                        }
                    }

                    // Install mods
                    if (forceUpdate || Program.ServerConfig.ModsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentModsPatch) > 0)
                    {
                        Directory.CreateDirectory(modsDir);

                        // Verify mods
                        List<string[]> modsDownloadList = new List<string[]>();
                        foreach (string[] mod in Program.ServerConfig.Files.Mods)
                        {
                            // Check
                            var modUrl = mod[0];
                            var modFilename = mod[1];
                            var modHash = mod[2];

                            if (forceUpdate || FileNeedUpdate(Path.Combine(modsDir, modFilename), modHash))
                            {
                                modsDownloadList.Add(new string[] { modUrl, modFilename });
                            }
                        }

                        // Download mods
                        using (WebClient wcModsDownloader = new WebClient())
                        {
                            foreach (string[] mod in modsDownloadList)
                            {
                                var modUrl = mod[0];
                                var modFilename = mod[1]; // Name to show
                                var modFullFilename = Path.Combine(modsDir, Path.GetFileName(modUrl)); // Full file path

                                ShowStatus(Lang.__("StatusDownloadingFile", LangDomain).Replace("%s", modFilename));

                                try
                                {
                                    await wcModsDownloader.DownloadFileTaskAsync(modUrl, modFullFilename);
                                    ExtractZipIfNeeded(modFullFilename, modsDir);
                                }
                                catch (Exception e)
                                {
                                    ShowStatusError(Lang.__("StatusDownloadError", LangDomain).Replace("%s", modFilename), e);

                                    // Refresh form
                                    EnableControls();
                                    RefreshControls(false);

                                    return;
                                }
                            }
                        }

                        // Mods updated!
                        Program.Config.Launchers[(int)selectedLauncher].CurrentModsPatch = Program.ServerConfig.ModsPatchVersion;
                    }


                    // Install extra/config files
                    if (forceUpdate || Program.ServerConfig.ConfigsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentConfigsPatch) > 0)
                    {
                        using (WebClient wcExtraDownloader = new WebClient())
                        {
                            var configFileUrl = Program.ServerConfig.Files.ConfigFiles;
                            var configFilename = Path.GetFileName(configFileUrl);
                            var configFullFilename = Path.Combine(gameDir, configFilename);

                            ShowStatus(Lang.__("StatusDownloadingFile", LangDomain).Replace("%s", configFilename));

                            try
                            {
                                await wcExtraDownloader.DownloadFileTaskAsync(configFileUrl, configFullFilename);
                                ExtractZipIfNeeded(configFullFilename, gameDir);
                            }
                            catch (Exception e)
                            {
                                ShowStatusError(Lang.__("StatusDownloadError", LangDomain).Replace("%s", configFilename), e);

                                // Refresh form
                                EnableControls();
                                RefreshControls(false);

                                return;
                            }
                        }

                        // Configs updated!
                        Program.Config.Launchers[(int)selectedLauncher].CurrentConfigsPatch = Program.ServerConfig.ConfigsPatchVersion;
                    }

                    // Delete old/obsolete files
                    ShowStatus(Lang.__("StatusFinalizingInstall", LangDomain));

                    foreach (string pathToDelete in Program.ServerConfig.Files.PathsToDelete)
                    {
                        var fullPathToDelete = pathToDelete.Replace("{gameDir}", gameDir).Replace("{modsDir}", modsDir);

                        // Check for file deletion
                        if (File.Exists(fullPathToDelete))
                        {
                            File.Delete(fullPathToDelete);
                        }

                        // Check for folder deletion
                        if (Directory.Exists(fullPathToDelete))
                        {
                            Directory.Delete(fullPathToDelete, true);
                        }
                    }

                    // Set game options
                    if (forceUpdate || Program.ServerConfig.GameOptionsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentGameOptionsPatch) > 0)
                    {
                        var gameOptions = (GameOptions)cmbOptions.SelectedIndex;
                        SetGameOptions(gameOptions, selectedLauncher);
                    }

                    // Installation complete! Save it!
                    Program.Config.Launchers[(int)selectedLauncher].GameInstalled = true;              
                    Program.SaveConfig();

                    // Refresh form
                    EnableControls();
                    RefreshControls(true);
                }
                catch (Exception e)
                {
                    ShowStatusError(Lang.__("StatusInstallError", LangDomain), e);

                    // Refresh form
                    EnableControls();
                    RefreshControls(false);

                    return;
                }
            }
            else
            {
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// First install full base game with mods, configs and etc
        /// </summary>
        private async void FirstGameInstall()
        {
            btPlay.Text = Lang.__("GameInstallingText", LangDomain);
            ShowStatus(Lang.__("StatusPreparingInstall", LangDomain));

            // Deep search for password
            try
            {
                BackupPasswordFile(PasswordFileScanDepth.AllLaunchers);
            }
            catch { }

            // Process full install with backup
            await InstallGame(true, true);
        }

        /// <summary>
        /// Fully update game needed files
        /// </summary>
        private async void UpdateGame()
        {
            btPlay.Text = Lang.__("GameUpdatingText", LangDomain);
            ShowStatus(Lang.__("StatusPreparingInstall", LangDomain));

            // backup password for prevention
            try
            {
                BackupPasswordFile(PasswordFileScanDepth.CurrentInstall);
            }
            catch { }

            // Process install without backups and need update only
            await InstallGame(false, false);
        }

        /// <summary>
        /// Force reinstall all the game without backup
        /// </summary>
        private async void ReinstallGame()
        {
            btPlay.Text = Lang.__("GameInstallingText", LangDomain);
            ShowStatus(Lang.__("StatusPreparingInstall", LangDomain));

            // backup password for prevention
            try
            {
                BackupPasswordFile(PasswordFileScanDepth.CurrentInstall);
            }
            catch { }

            // Process install without backups but forcing update everything
            await InstallGame(false, true);
            chkForceUpdate.Checked = false;
        }

        /// <summary>
        /// Download TLauncher to launcher data dir folder and configure folders so don't need to run it first
        /// </summary>
        /// <param name="downloadOnly">If true, only download TLauncher, otherwise it will create required files and folders</param>
        private async void InstallTLauncher(bool downloadOnly)
        {
            btPlay.Text = Lang.__("GameInstallingText", LangDomain);
            ShowStatus(Lang.__("StatusTLauncherInstall", LangDomain));

            try
            {
                var launcherUrl = Program.ServerConfig.Files.TLauncherLauncher;
                var localSavePath = Path.Combine(Program.ServerConfig.LauncherDataDir, "launchers");
                var launcherFileName = Path.GetFileName(launcherUrl);
                var launcherFullFilename = Path.Combine(localSavePath, launcherFileName);

                Directory.CreateDirectory(localSavePath);

                // Download TLauncher
                using (WebClient wcDownloadTLauncher = new WebClient())
                {
                    await wcDownloadTLauncher.DownloadFileTaskAsync(launcherUrl, launcherFullFilename);
                }

                ExtractZipIfNeeded(launcherFullFilename, localSavePath, false);

                // Configure TLauncher directories and files
                if (!downloadOnly)
                {
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var minecraftDir = Path.Combine(appData, ".minecraft");
                    var tlauncherPath = Path.Combine(appData, ".tlauncher");

                    Directory.CreateDirectory(minecraftDir);
                    Directory.CreateDirectory(Path.Combine(minecraftDir, "versions"));
                    Directory.CreateDirectory(tlauncherPath);

                    var tlauncherProperties = Path.Combine(tlauncherPath, "tlauncher-2.0.properties");
                    var propLines = new string[]
                    {
                    $"minecraft.gamedir={Regex.Escape(minecraftDir).Replace(@"\\\", @"\\")}",
                    $"login.version.game={Program.ServerConfig.GameInstallDir}"
                    };

                    File.WriteAllLines(tlauncherProperties, propLines);
                }

                // Save exe path
                var launcherExeFilename = Path.Combine(localSavePath, "TLauncher.jar");
                Program.Config.Launchers[(int)MinecraftLauncher.TLauncher].ExecutablePath = launcherExeFilename;
                Program.SaveConfig();
            }
            catch (Exception e)
            {
                ShowStatusError(Lang.__("StatusInstallError", LangDomain), e);

                // Refresh form
                EnableControls();
                RefreshControls(false);

                return;
            }

            // Now lets install the game
            FirstGameInstall();
        }

        /// <summary>
        /// Rename the folder to folder_backup1, folder_backup2, etc
        /// </summary>
        /// <param name="path">The folder path to backup (rename)</param>
        private void BackupFolder(string path)
        {
            if (Directory.Exists(path))
            {
                // Generate backup folder name
                var backupNameValid = false;
                var count = 0;
                while (!backupNameValid)
                {
                    count++;

                    if (!Directory.Exists($"{path}_backup{count}"))
                    {
                        backupNameValid = true;
                    }
                }

                // Do Backup
                Directory.Move(path, $"{path}_backup{count}");
            }
        }

        /// <summary>
        /// Gets the .minecraft folder path according to selected launcher
        /// </summary>
        /// <param name="launcher">Selected launcher, Original, TLauncher, etc</param>
        /// <param name="showErrors">Call ShowStatusError if an error occurs</param>
        /// <returns>The .minecraft folder path of respective minecraft launcher or empty string if error</returns>
        private string GetDotMinecraftDir(MinecraftLauncher launcher, bool showErrors = true)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            switch (launcher)
            {
                case MinecraftLauncher.Minecraft:
                    {
                        return "";
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
                                        return gameDir;
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (showErrors)
                                    {
                                        ShowStatusError(Lang.__("TLauncherIsOpenError", Program.LangDomain), e);
                                        EnableControls();
                                        RefreshControls(false);
                                    }
                                }
                            }
                        }

                        return "";
                    }

                default:
                    return "";
            }
        }

        /// <summary>
        /// Search for game installation in given launcher
        /// </summary>
        /// <param name="launcher">Launcher to search the game install</param>
        /// <returns>True if the game install folder exists in given launcher directory</returns>
        private bool GameInstallationFound(MinecraftLauncher launcher)
        {
            // Setup directories
            var minecraftDir = GetDotMinecraftDir(launcher);
            if (Directory.Exists(minecraftDir))
            {
                var gameDir = minecraftDir;
                if (launcher == MinecraftLauncher.TLauncher)
                {
                    gameDir = Path.Combine(minecraftDir, $"versions/{Program.ServerConfig.GameInstallDir}");
                }

                var modsDir = Path.Combine(gameDir, "mods");
                return Directory.Exists(gameDir) && Directory.Exists(modsDir);
            }

            return false;
        }
        #endregion

        #region "File Functions"
        private enum PasswordFileScanDepth
        {
            CurrentInstall = 1, // Only current game install
            AllInstalls,        // Any game installs of this server
            CurrentLauncher,    // Any game installs using current launcher
            AllLaunchers        // Any game installs in any launcher
        }

        /// <summary>
        /// If there's no backup, scans for a password file until found one and then backup it
        /// </summary>
        /// <param name="scanDepth">Password search scan depth</param>
        private void BackupPasswordFile(PasswordFileScanDepth scanDepth)
        {
            // Skip backup if already exists
            var searchPwdFileName = Path.GetFileName(Program.ServerConfig.PasswordFileName);
            var localPwdFilePath = Path.Combine(Program.ServerConfig.LauncherDataDir, searchPwdFileName);

            if (!File.Exists(localPwdFilePath))
            {
                // Let's start the backup stuff
                var fileFound = false;

                // Check for existing password file in current selected install
                var selectedLauncher = (MinecraftLauncher)cmbLaunchers.SelectedIndex;
                var pwdFilePath = "";

                fileFound = FoundPasswordFile(selectedLauncher, out pwdFilePath);

                // Check for other existing installs
                if (!fileFound && scanDepth >= PasswordFileScanDepth.AllInstalls)
                {
                    for (int i = 0; i < Program.Config.Launchers.Length; i++)
                    {
                        var launcher = (MinecraftLauncher)i;
                        if (launcher != selectedLauncher)
                        {
                            fileFound = FoundPasswordFile(launcher, out pwdFilePath);
                        }
                    }
                }

                // Deep check every folder in current install
                if (!fileFound && scanDepth >= PasswordFileScanDepth.CurrentLauncher)
                {
                    fileFound = DeepFoundPasswordFile(selectedLauncher, out pwdFilePath);
                }

                // Deep check for other existing installs
                if (!fileFound && scanDepth >= PasswordFileScanDepth.AllLaunchers)
                {
                    for (int i = 0; i < Program.Config.Launchers.Length; i++)
                    {
                        var launcher = (MinecraftLauncher)i;
                        if (launcher != selectedLauncher)
                        {
                            fileFound = DeepFoundPasswordFile(launcher, out pwdFilePath);
                        }
                    }
                }

                // Backup if file is found
                if (fileFound)
                {
                    File.Copy(pwdFilePath, Path.Combine(Program.ServerConfig.LauncherDataDir, Path.GetFileName(pwdFilePath)), true);
                }
            }
        }

        /// <summary>
        /// Restore a stored password file to given launcher game install folder
        /// </summary>
        /// <param name="launcher">Launcher to restore game install password</param>
        /// <param name="replace">Should replace existing password file?</param>
        /// <returns>true if file is restored, false if file not found or already exists and replace = true</returns>
        private bool RestorePasswordFile(MinecraftLauncher launcher, bool replace = true)
        {
            // Password file exists in launcher data dir?
            var searchPwdFileName = Path.GetFileName(Program.ServerConfig.PasswordFileName);
            var localPwdFilePath = Path.Combine(Program.ServerConfig.LauncherDataDir, searchPwdFileName);

            if (File.Exists(localPwdFilePath))
            {
                var gameDir = GetDotMinecraftDir(launcher);
                if (launcher == MinecraftLauncher.TLauncher)
                {
                    gameDir = Path.Combine(gameDir, $"versions/{Program.ServerConfig.GameInstallDir}");
                }

                var modsDir = Path.Combine(gameDir, "mods");

                var pwdFilename = Program.ServerConfig.PasswordFileName.Replace("{gameDir}", gameDir).Replace("{modsDir}", modsDir);

                if (!File.Exists(pwdFilename))
                {
                    File.Copy(localPwdFilePath, pwdFilename);
                    return true;
                }
                else if (replace)
                {
                    File.Copy(localPwdFilePath, pwdFilename, true);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Scan every game installation in selected launcher to found password file
        /// </summary>
        /// <returns></returns>
        private bool DeepFoundPasswordFile(MinecraftLauncher launcher, out string path)
        {
            var baseGameDir = GetDotMinecraftDir(launcher);
            if (launcher == MinecraftLauncher.TLauncher)
            {
                baseGameDir = Path.Combine(baseGameDir, "versions");
            }

            // Scan all installs
            foreach(string gameDir in Directory.GetDirectories(baseGameDir))
            {
                var modsDir = Path.Combine(gameDir, "mods");

                var pwdFilename = Program.ServerConfig.PasswordFileName.Replace("{gameDir}", gameDir).Replace("{modsDir}", modsDir);

                Console.WriteLine(pwdFilename);
                if (File.Exists(pwdFilename))
                {
                    path = pwdFilename;
                    return true;
                }
            }
            
            path = "";
            return false;
        }

        /// <summary>
        /// Check if password file exists in selected launcher in current install
        /// </summary>
        /// <param name="launcher">Minecraft launcher to check</param>
        /// <param name="path">File path if found</param>
        /// <returns>True if file is found, otherwise false</returns>
        private bool FoundPasswordFile(MinecraftLauncher launcher, out string path)
        {
            var gameDir = GetDotMinecraftDir(launcher);
            if (launcher == MinecraftLauncher.TLauncher)
            {
                gameDir = Path.Combine(gameDir, $"versions/{Program.ServerConfig.GameInstallDir}");
            }

            var modsDir = Path.Combine(gameDir, "mods");

            var pwdFilename = Program.ServerConfig.PasswordFileName.Replace("{gameDir}", gameDir).Replace("{modsDir}", modsDir);

            Console.WriteLine(pwdFilename);
            if (File.Exists(pwdFilename))
            {
                path = pwdFilename;
                return true;
            }

            path = "";
            return false;
        }

        /// <summary>
        /// Check for game options file and copy to current launcher folder
        /// </summary>
        /// <param name="gameOption">Game option to apply</param>
        /// <param name="launcher">Target launcher to copy files to game install</param>
        private void SetGameOptions(GameOptions gameOption, MinecraftLauncher launcher)
        {
            // Get local file name
            var optionsFileUrl = "";

            switch (gameOption)
            {
                case GameOptions.Low:
                    optionsFileUrl = Program.ServerConfig.Files.LowOptions;
                    break;

                case GameOptions.Medium:
                    optionsFileUrl = Program.ServerConfig.Files.MediumOptions;
                    break;

                case GameOptions.High:
                    optionsFileUrl = Program.ServerConfig.Files.HighOptions;
                    break;
            }

            var fileUrl = optionsFileUrl.Replace("{Culture}", Program.ServerConfig.DefaultCulture);

            if (Program.ServerConfig.AvailableCultures.Contains(Lang.Culture.TwoLetterISOLanguageName))
            {
                fileUrl = optionsFileUrl.Replace("{Culture}", Lang.Culture.TwoLetterISOLanguageName);
            }

            var fileName = Path.GetFileName(fileUrl);
            var localFilename = Path.Combine(FrmInit.LocalOptionsSavePath, fileName);

            // Save it to destination
            if (File.Exists(localFilename))
            {
                var gameDir = GetDotMinecraftDir(launcher);
                if (launcher == MinecraftLauncher.TLauncher)
                {
                    gameDir = Path.Combine(gameDir, $"versions/{Program.ServerConfig.GameInstallDir}");
                }

                var destFilename = Path.Combine(gameDir, fileName);

                File.Copy(localFilename, destFilename, true);
                ExtractZipIfNeeded(destFilename, gameDir, false);

                Program.Config.Launchers[(int)launcher].SelectedGameOptions = gameOption;
                Program.Config.Launchers[(int)launcher].CurrentGameOptionsPatch = Program.ServerConfig.GameOptionsPatchVersion;
            }
        }

        /// <summary>
        /// Extracted the file to the destination only if it is a .zip file and then delete it
        /// </summary>
        /// <param name="filename">File to extract</param>
        /// <param name="destination">Directory to extract files to</param>
        /// <param name="showStatus">Call ShowStatus() with a message about the file extraction/param>
        private void ExtractZipIfNeeded(string filename, string destination, bool showStatus = true)
        {
            if (Path.GetExtension(filename) == ".zip")
            {
                if (showStatus)
                {
                    ShowStatus(Lang.__("StatusExtractingFile", LangDomain).Replace("%s", Path.GetFileName(filename)));
                }

                Program.ExtractZipFileToDirectory(filename, destination, true);
                File.Delete(filename);
            }
        }

        /// <summary>
        /// Check if a file needs update (not exists or md5 hash is different)
        /// </summary>
        /// <param name="filename">File to verify</param>
        /// <param name="hash">File hash string to compare</param>
        /// <param name="showStatus">Call ShowStatus() with a message of the file being verified</param>
        /// <returns>True if file needs to be update, otherwise its false</returns>
        private bool FileNeedUpdate(string filename, string hash, bool showStatus = true)
        {
            ShowStatus(Lang.__("StatusVerifyingFile", LangDomain).Replace("%s", Path.GetFileName(filename)));

            if (File.Exists(filename))
            {
                return MD5FileHash(filename) != hash;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Calculate the MD5 Hash of a file
        /// </summary>
        /// <param name="filename">Target file to hash calculation</param>
        /// <returns>String containing the file MD5 Hash</returns>
        private string MD5FileHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        #endregion

        #region "Form Controls Functions"
        /// <summary>
        /// Show a status message in form
        /// </summary>
        /// <param name="message">Message to show</param>
        private void ShowStatus(string message)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = SystemColors.ControlText;

            Console.WriteLine(message);
        }

        /// <summary>
        /// Show a red status message in form
        /// </summary>
        /// <param name="errorMessage">Message to show</param>
        /// <param name="e">Exception to save for support</param>
        private void ShowStatusError(string errorMessage, Exception e)
        {
            lblStatus.Text = errorMessage;
            lblStatus.ForeColor = Color.Red;

            Console.WriteLine(errorMessage);
        }

        /// <summary>
        /// Refresh form controls data like play button text (play, update, install) and installation status.
        /// </summary>
        private void RefreshControls(bool showStatus)
        {
            var selectedLauncher = (MinecraftLauncher)cmbLaunchers.SelectedIndex;

            // Check game options
            cmbOptions.SelectedIndex = (int)Program.Config.Launchers[(int)selectedLauncher].SelectedGameOptions;

            // Original minecraft not available
            if (selectedLauncher == MinecraftLauncher.Minecraft)
            {
                btPlay.Enabled = false;
                btPlay.Text = Lang.__("UnavailableLauncherText", LangDomain);
                ShowStatusError(Lang.__("LauncherNotAvailable", LangDomain), new NotImplementedException());
                return;
            }
            else
            {
                btPlay.Enabled = true;
                if (showStatus)
                {
                    ShowStatus("");
                }
            }

            // Check game installed
            if (Program.Config.Launchers[(int)selectedLauncher].GameInstalled)
            {
                if (chkForceUpdate.Checked)
                {
                    btPlay.Text = Lang.__("ReinstallGameText", LangDomain);
                }
                else if (Program.ServerConfig.BaseGamePatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentBaseGamePatch) > 0 ||
                    Program.ServerConfig.ModsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentModsPatch) > 0 ||
                    Program.ServerConfig.ConfigsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentConfigsPatch) > 0)
                {
                    btPlay.Text = Lang.__("UpdateGameText", LangDomain);
                    if (showStatus)
                    {
                        ShowStatus(Lang.__("StatusNeedUpdate", LangDomain));
                    }
                }
                else
                {
                    btPlay.Text = Lang.__("PlayGameText", LangDomain);
                    if (showStatus)
                    {
                        ShowStatus(Lang.__("StatusUpToDate", LangDomain));
                    }
                }
            }
            else
            {
                if (Program.Config.Launchers[(int)selectedLauncher].Available)
                {
                    btPlay.Text = Lang.__("InstallGameText", LangDomain);
                }
                else
                {
                    switch (selectedLauncher)
                    {
                        case MinecraftLauncher.TLauncher:
                            btPlay.Text = Lang.__("InstallGameText", LangDomain);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Enable form controls and let the user decide what to do
        /// </summary>
        private void EnableControls()
        {
            cmbLaunchers.Enabled = true;
            chkForceUpdate.Enabled = true;
            cmbOptions.Enabled = true;
            btPlay.Enabled = true;
        }

        /// <summary>
        /// Disable form controls to prevent users from take actions while updating/installing/etc
        /// </summary>
        private void DisableControls()
        {
            cmbLaunchers.Enabled = false;
            chkForceUpdate.Enabled = false;
            cmbOptions.Enabled = false;
            btPlay.Enabled = false;
        }
        #endregion

        #region "Form Controls"
        private void btPlay_Click(object sender, EventArgs e)
        {
            DisableControls();

            var selectedLauncher = (MinecraftLauncher)cmbLaunchers.SelectedIndex;

            if (Program.Config.Launchers[(int)selectedLauncher].GameInstalled)
            {
                if (chkForceUpdate.Checked)
                {
                    ReinstallGame();
                }
                else if (Program.ServerConfig.BaseGamePatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentBaseGamePatch) > 0 ||
                    Program.ServerConfig.ModsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentModsPatch) > 0 ||
                    Program.ServerConfig.ConfigsPatchVersion.CompareTo(
                    Program.Config.Launchers[(int)selectedLauncher].CurrentConfigsPatch) > 0)
                {
                    UpdateGame();
                }
                else
                {
                    PlayGame();
                }
            }
            else
            {
                if (Program.Config.Launchers[(int)selectedLauncher].Available)
                {
                    switch (selectedLauncher)
                    {
                        case MinecraftLauncher.TLauncher:
                            InstallTLauncher(true);
                            break;
                    }
                }
                else
                {
                    switch (selectedLauncher)
                    {
                        case MinecraftLauncher.TLauncher:
                            InstallTLauncher(false);
                            break;
                    }
                }
            }
        }

        private void CmbLaunchers_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshControls(true);
        }

        private void ChkForceUpdate_CheckedChanged(object sender, EventArgs e)
        {
            RefreshControls(true);
        }
        #endregion
    }
}
