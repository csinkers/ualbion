using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class AssetIdConfig
    {
        public Dictionary<string, List<AssetType>> Mappings { get; } = new Dictionary<string, List<AssetType>>();
        public Dictionary<string, List<string>> Extras { get; } = new Dictionary<string, List<string>>();
        public static AssetIdConfig Load(string filename) => JsonConvert.DeserializeObject<AssetIdConfig>(File.ReadAllText(filename));
    }
}