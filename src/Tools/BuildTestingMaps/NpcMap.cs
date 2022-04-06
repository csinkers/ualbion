using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public static class NpcMap
{
    const byte MapWidth = 255;
    const byte MapHeight = 255;
    static int Pos(int x, int y) => y * MapWidth + x;
    public static Dictionary<AssetId, object> Build(MapId mapId)
    {
        var builder = MapBuilder.Create2D(mapId, Palette1Id, Tileset1.Tileset.Id, MapWidth, MapHeight);
                
        for (int index = 0; index < 250; index++)
        {
            int i = index;
            builder.SetChain(i, s => @$"
	text {s($"Test string {i} ({i:X})")}
	change_language Tom Celtic SetToMaximum
	");
        }

        builder.Draw2D(map =>
        {
            map.Flags |= MapFlags.Unk8000; 

            Array.Fill(map.Overlay, 0);
            for (int i = 0; i < map.Underlay.Length; i++)
            {
                var y = i / map.Width;
                var x = i % map.Width;
                map.Underlay[i] = 
                    x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1
                    ? Tileset1.SolidOffset
                    : Tileset1.BlankOffset;
            }

            const int cellW = 6;
            const int cellH = 7;
            const int cellSpacingX = 2;
            const int cellSpacingY = 2;
            const int cellArrayW = 16;
            const int cellArrayH = 16;
            MajMin(cellArrayW, cellArrayH, (majI, majJ) =>
            {
                int cellIndex = majJ * cellArrayW + majI;
                int x0 = majI * (cellW + cellSpacingX);
                int y0 = majJ * (cellH + cellSpacingY);

                MajMin(cellW, cellH, (i, j) =>
                {
                    if (i == 0 || j == 0 || i == (cellW - 1) || j == (cellH - 1))
                        map.Underlay[Pos(x0 + i, y0 + j)] = Tileset1.SolidOffset; // Draw cell walls
                });

                map.Underlay[Pos(x0 + 1, y0 + 1)] = Tileset1.UnderlayOffset + cellIndex; // Add marker
                if (cellIndex < 250)
                    map.AddZone((byte)(x0 + 1), (byte)(y0 + 1), TriggerTypes.Manipulate, (ushort)cellIndex);

                if (cellIndex != 0x83 && cellIndex != 0x84)
                    return;

                var waypoints = BuildPatrolPath(x0+3, y0+6);
                map.Npcs.Add(new MapNpc {
                    Id = (NpcId)Npc.Cuarnainn, // (NpcId)(UAlbion.Base.Npc)(index+1),
                    Type = NpcType.Npc,
                    SpriteOrGroup = (SpriteId)LargeNpc.Rainer, // (SpriteId)(UAlbion.Base.LargeNpc)(index+1),
                    Flags = 0, // (NpcFlags.SimpleMsg), // (NpcFlags)index,
                    Movement = NpcMovement.Waypoints,
                    Waypoints = waypoints,
                });
            });
        });

        var (map, mapText) = builder.Build();
        return new Dictionary<AssetId, object>
        {
            { map.Id, map },
            { map.Id.ToMapText(), mapText }
        };
    }
}