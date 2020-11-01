using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class ModConfig
    {
        public string Repo { get; }
        public string Name { get; }
        public string Description { get; }
        public string Author { get; }
        public Version Version { get; }
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