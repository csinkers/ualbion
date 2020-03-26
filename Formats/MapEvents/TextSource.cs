using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class TextSource
    {
        public static TextSource Map(MapDataId mapId) => new TextSource(AssetType.MapText, (int)mapId);
        public static TextSource EventSet(EventSetId eventSetId) => new TextSource(AssetType.EventText, (int)eventSetId);

        TextSource(AssetType type, int id) { Type = type; Id = id; }

        public AssetType Type { get; }
        public int Id { get; }
    }
}