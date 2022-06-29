using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public static class EventMap
{
    const byte MapWidth = 64;
    const byte MapHeight = 64;
    static int Pos(int x, int y) => y * MapWidth + x;
    public static Dictionary<AssetId, object> Build(MapId mapId)
    {
        var builder = MapBuilder.Create2D(mapId, Constants.Palette1Id, Constants.Tileset1.Tileset.Id, MapWidth, MapHeight);
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