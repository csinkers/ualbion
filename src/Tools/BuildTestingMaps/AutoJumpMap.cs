using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace BuildTestingMaps;

static class AutoJumpMap
{
    const byte MapWidth = 36;
    const byte MapHeight = 80;
    public static Dictionary<AssetId, object> Build(Map mapId, Map targetId, int x, int y, TestTilemap tileset1)
    {
        ArgumentNullException.ThrowIfNull(tileset1);

        var builder = new MapBuilder2D(mapId, Constants.Palette1Id, tileset1, MapWidth, MapHeight);
        builder.DrawBorder();
        builder.SetChain(0, _ => @$"teleport {targetId} {x} {y}");
        builder.AddGlobalZone(TriggerTypes.EveryStep, 0);

        var (map, mapText) = builder.Build();
        return new Dictionary<AssetId, object>
        {
            { map.Id, map },
            { map.Id.ToMapText(), mapText }
        };
    }
}