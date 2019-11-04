using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class CoreSpriteConfig
    {
        public class BinaryResource
        {
            public long Offset { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public Position2D Hotspot { get; set; }
        }

        public readonly IDictionary<int, string> CoreSpriteIds = new Dictionary<int, string>();
        public readonly IDictionary<string, IDictionary<int, BinaryResource>> Hashes = new Dictionary<string, IDictionary<int, BinaryResource>>();
        public string ExePath { get; set; }

        public static CoreSpriteConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "core_sprites.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Could not find core_sprites.json, was expected to be at {configPath}");

            var configText = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<CoreSpriteConfig>(configText);
        }
    }
}