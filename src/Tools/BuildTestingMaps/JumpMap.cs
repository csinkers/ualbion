using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public static class JumpMap
{
    const byte MapWidth = 255;
    const byte MapHeight = 255;
    public static Dictionary<AssetId, object> Build(MapId mapId, MapId targetId, byte x, byte y)
    {
        var builder = MapBuilder.Create2D(mapId, Constants.Palette1Id, Constants.Tileset1.Tileset.Id, MapWidth, MapHeight);
        builder.SetChain(0, _ => @$"
	teleport {targetId.Id} {x} {y}
	");

        builder.Draw2D(map =>
        {
            map.Flags |= MapFlags.Unk8000; 
            map.AddGlobalZone(TriggerTypes.MapInit, 0);
        });

        var (map, mapText) = builder.Build();
        return new Dictionary<AssetId, object>
        {
            { map.Id, map },
            { map.Id.ToMapText(), mapText }
        };
    }
}