using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class LoadAssetRangeInfo
{
    public int? Sequence { get; set; }
    public Dictionary<string, LoadAssetFileInfo> Files { get; set; } = [];
    [JsonInclude] [JsonExtensionData] public Dictionary<string, JsonElement> Properties { get; private set; }
}