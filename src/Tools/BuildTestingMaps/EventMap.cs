using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public static class EventMap
{
    const byte MapWidth = 64;
    const byte MapHeight = 64;
    static int Pos(int x, int y) => y * MapWidth + x;
    public static Dictionary<AssetId, object> Build(MapId mapId, TestTilemap tileset1)
    {
        if (tileset1 == null) throw new ArgumentNullException(nameof(tileset1));

        var builder = new MapBuilder2D(mapId, Constants.Palette1Id, tileset1, MapWidth, MapHeight);
        builder.DrawBorder();
        builder.Marker(1, 3, 3, "Give 10 gold to everyone", s => "change everyone gold AddAmount 100");

        var (map, mapText) = builder.Build();
        return new Dictionary<AssetId, object>
        {
            { map.Id, map },
            { map.Id.ToMapText(), mapText }
        };
    }
}