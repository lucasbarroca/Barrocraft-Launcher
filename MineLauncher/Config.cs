using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineLauncher
{
    public enum MinecraftLauncher
    {
        Minecraft,
        TLauncher
    }

    public enum GameOptions
    {
        Low,
        Medium,
        High
    }

    class Config
    {
        [JsonConverter(typeof(VersionConverter))]
        public Version CurrentLauncherVersion = new Version("1.2.0.0");

        [JsonConverter(typeof(StringEnumConverter))]
        public MinecraftLauncher LastSelectedLauncher = MinecraftLauncher.TLauncher;

        public MinecraftLauncherConfig[] Launchers = {
            new MinecraftLauncherConfig(MinecraftLauncher.Minecraft),
            new MinecraftLauncherConfig(MinecraftLauncher.TLauncher)
        };
    }

    class MinecraftLauncherConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MinecraftLauncher Launcher;

        public bool Available;
        public bool GameInstalled;

        [JsonConverter(typeof(VersionConverter))]
        public Version CurrentBaseGamePatch = new Version("1.0.0.0");
        [JsonConverter(typeof(VersionConverter))]
        public Version CurrentModsPatch = new Version("1.0.0.0");
        [JsonConverter(typeof(VersionConverter))]
        public Version CurrentConfigsPatch = new Version("1.0.0.0");
        [JsonConverter(typeof(VersionConverter))]
        public Version CurrentGameOptionsPatch = new Version("1.0.0.0");

        public GameOptions SelectedGameOptions = GameOptions.Medium;
        public string ExecutablePath = "";

        public MinecraftLauncherConfig()
        {

        }

        public MinecraftLauncherConfig(MinecraftLauncher minecraftLauncher)
        {
            Launcher = minecraftLauncher;
        }
    }
}
