using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

public class AssetIdConfig
{
    [JsonInclude] public Dictionary<string, List<AssetType>> Mappings { get; private set; } = [];
    [JsonInclude] public Dictionary<string, List<string>> Extras { get; private set; } = [];
    public static AssetIdConfig Load(string filename, IFileSystem disk, IJsonUtil jsonUtil)
    {
        ArgumentNullException.ThrowIfNull(disk);
        ArgumentNullException.ThrowIfNull(jsonUtil);
        return jsonUtil.Deserialize<AssetIdConfig>(disk.ReadAllBytes(filename));
    }
}