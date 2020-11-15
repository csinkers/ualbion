using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class GeneralConfig : IGeneralConfig
    {
        [JsonIgnore] public string BasePath { get; set; }
        public string XldPath { get; set; }
        public string ExePath { get; set; }
        public string SavePath { get; set; }
        public string SettingsPath { get; set; }
        public string CoreConfigPath { get; set; }
        public string GameConfigPath { get; set; }
        public string BaseAssetsPath { get; set; }
        public string ModPath { get; set; }
        public string ExportedXldPath { get; set; }

        public static GeneralConfig Load(string configPath, string baseDir)
        {
            var config = File.Exists(configPath) 
                ? JsonConvert.DeserializeObject<GeneralConfig>(File.ReadAllText(configPath)) 
                : new GeneralConfig();

            config.BasePath = baseDir;
            config.XldPath ??= @"albion/CD/XLDLIBS";
            config.ExePath ??= "ALBION";
            config.SavePath ??= "ALBION/SAVES";
            config.BaseAssetsPath ??= "data/Base";
            config.ModPath ??= "data/Mods";
            config.SettingsPath ??= "data/settings.json";
            config.CoreConfigPath ??= "data/core.json";
            config.GameConfigPath ??= "data/game.json";
            config.ExportedXldPath ??= "data/Exported/raw";
            return config;
        }
    }
}
