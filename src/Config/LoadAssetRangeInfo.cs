using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class LoadAssetRangeInfo
{
    public Dictionary<string, LoadAssetFileInfo> Files { get; set; } = new();
    [JsonInclude] [JsonExtensionData] public Dictionary<string, JsonElement> Properties { get; private set; }
}