using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class ModConfig
    {
        public string Repo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }
        [JsonProperty("asset_path")] public string AssetPath { get; set; }
        public List<string> Dependencies { get; } = new List<string>();

        public static ModConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"mod.config not found for mod {configPath}");

            var configText = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ModConfig>(configText);
        }
    }
}