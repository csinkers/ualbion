using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class LoadAssetInfo
{
    [JsonInclude, JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; private set; }
}