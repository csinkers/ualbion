using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

// Collections are set by JSON.NET
#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Config
{
    public class CoreSpriteConfig
    {
        public IDictionary<int, string> CoreSpriteIds { get; set; } = new Dictionary<int, string>();
        public IDictionary<string, IDictionary<int, CoreSpriteInfo>> Hashes { get; set; } = new Dictionary<string, IDictionary<int, CoreSpriteInfo>>();

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
#pragma warning restore CA2227 // Collection properties should be read only
