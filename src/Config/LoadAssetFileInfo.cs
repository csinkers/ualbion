using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class LoadAssetFileInfo
{
    public string MapFile { get; set; }
    public Dictionary<string, LoadAssetInfo> Map { get; set; }

    [JsonInclude, JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; private set; }
}