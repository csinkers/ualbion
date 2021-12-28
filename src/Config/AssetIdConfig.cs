using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

public class AssetIdConfig
{
    [JsonInclude] public Dictionary<string, List<AssetType>> Mappings { get; private set; } = new();
    [JsonInclude] public Dictionary<string, List<string>> Extras { get; private set; } = new();
    public static AssetIdConfig Load(string filename, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        return jsonUtil.Deserialize<AssetIdConfig>(disk.ReadAllBytes(filename));
    }
}