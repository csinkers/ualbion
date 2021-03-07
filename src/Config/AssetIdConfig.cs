using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class AssetIdConfig
    {
        public Dictionary<string, List<AssetType>> Mappings { get; } = new Dictionary<string, List<AssetType>>();
        public Dictionary<string, List<string>> Extras { get; } = new Dictionary<string, List<string>>();
        public static AssetIdConfig Load(string filename, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            return JsonConvert.DeserializeObject<AssetIdConfig>(disk.ReadAllText(filename));
        }
    }
}
