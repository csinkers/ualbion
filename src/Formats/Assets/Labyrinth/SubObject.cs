using System.Text.Json.Serialization;

namespace UAlbion.Formats.Assets.Labyrinth;

public class SubObject
{
    [JsonPropertyName("x")] public short X { get; set; }
    [JsonPropertyName("y")] public short Y { get; set; }
    [JsonPropertyName("z")] public short Z { get; set; }
    [JsonPropertyName("obj")] public ushort ObjectInfoNumber { get; set; }
    public override string ToString() => $"{ObjectInfoNumber}({SpriteId}) @ ({X}, {Y}, {Z})";
    [JsonIgnore] internal SpriteId SpriteId { get; set; }
}