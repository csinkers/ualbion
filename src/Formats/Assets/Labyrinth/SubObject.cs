using Newtonsoft.Json;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class SubObject
    {
        [JsonProperty("x")] public short X { get; set; }
        [JsonProperty("y")] public short Y { get; set; }
        [JsonProperty("z")] public short Z { get; set; }
        [JsonProperty("obj")] public ushort ObjectInfoNumber { get; set; }
        public override string ToString() => $"{ObjectInfoNumber}({SpriteId}) @ ({X}, {Y}, {Z})";
        [JsonIgnore] internal SpriteId SpriteId { get; set; }
    }
}
