using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Scripting;

public class DebugVisualizerNodeData
{
    public DebugVisualizerNodeData(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
    [JsonPropertyName("id")] public string Id { get; }
    [JsonPropertyName("label")] public string Label { get; set; }
    [JsonPropertyName("color")] public string Color { get; set; }
    [JsonPropertyName("shape")] public string Shape { get; set; }
    public override string ToString() => JsonSerializer.Serialize(this);
}