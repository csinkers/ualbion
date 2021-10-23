using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Scripting
{
    public class DebugVisualizerEdgeData
    {
        public DebugVisualizerEdgeData(string from, string to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }

        [JsonPropertyName("from")] public string From { get; }
        [JsonPropertyName("to")] public string To { get; }
        [JsonPropertyName("label")] public string Label { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("color")] public string Color { get; set; }
        [JsonPropertyName("dashes")] public bool Dashes { get; set; }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}