using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Api;

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
        [JsonProperty("shader_path")] public string ShaderPath { get; set; }
        public List<string> Dependencies { get; } = new List<string>();

        public static ModConfig Load(string configPath, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(configPath))
                throw new FileNotFoundException($"mod.config not found for mod {configPath}");

            var configText = disk.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ModConfig>(configText);
        }
    }
}
