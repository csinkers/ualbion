using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public static class JumpMap
{
    const byte MapWidth = 64;
    const byte MapHeight = 64;

    public static Dictionary<AssetId, object> Build(MapId mapId, (MapId id, string name)[] portals, TestTilemap tileset1)
    {
        if (tileset1 == null) throw new ArgumentNullException(nameof(tileset1));

        var builder = new MapBuilder2D(mapId, Palette1Id, tileset1, MapWidth, MapHeight);
        builder.DrawBorder();
        builder.SetChain(portals.Length, _ => @$"teleport Map.300 8 8, chain_off Set {portals.Length}");
        builder.AddGlobalZone(TriggerTypes.EveryStep, portals.Length);

        var columns = (int)Math.Ceiling(Math.Sqrt(portals.Length));
        for (int i = 0, j = 0, index = 0; index < portals.Length; index++, i++)
        {
            if (i == columns) { i = 0; j++; }
            var portal = portals[index];
            builder.Marker(index, 3 + i * 2, 3 + j * 2, portal.name, s => @$"
teleport {portal.id} 8 8
");
        }

        var (map, mapText) = builder.Build();
        return new Dictionary<AssetId, object>
        {
            { map.Id, map },
            { map.Id.ToMapText(), mapText }
        };
    }
}