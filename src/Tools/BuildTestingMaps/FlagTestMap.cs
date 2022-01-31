using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public static class FlagTestMap
{
    const byte MapWidth = 255;
    const byte MapHeight = 255;
    static int Pos(int x, int y) => y * MapWidth + x;
    public static Dictionary<AssetId, object> Build(MapId mapId)
    {
        var assets = new Dictionary<AssetId, object>();
        var builder = MapBuilder.Create2D(mapId, Palette1Id, Tileset1.Tileset.Id, MapWidth, MapHeight);
        builder.Draw2D(map =>
        {
            map.Flags |= MapFlags.Unk8000 | MapFlags.ExtraNpcs;  

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

            ushort n = 0;
            void Add(string name, Func<Func<string, int>, string> scriptBuilder)
            {
                for (var index = 0; index < name.Length; index++)
                {
                    var c = name[index];
                    map.Underlay[Pos(index+1, n + 1)] = Tileset1.IndexForChar(c);
                }

                builder.SetChain(n, scriptBuilder);
                map.AddZone(1, (byte)(n+1), TriggerTypes.Manipulate, n);
                n++;
            }

            Add("S0", s => @$"
    text {s("Setting switch 0")}
    switch 1 0
");

            Add("S!", s => @$"
    text {s("Setting switch 1023")}
    switch 1 1023
");

            Add("D0", s =>
            {
                var openText = s("Opened door 0");
                var unlockText = s("Unlocked door 0");
                return @$"
    text {s("Opening door 0")}
    open_door 0 {mapId.ToMapText()} Item.FragrantWater 50 {openText} {unlockText}
";
            });

            Add("D!", s =>
            {
                var openText = s("Opened door 998");
                var unlockText = s("Unlocked door 998");
                return @$"
    text {s("Opening door 998")}
    open_door 998 {mapId.ToMapText()} Item.FragrantWater 50 {openText} {unlockText}
";
            });

            var chest = new Inventory(new InventoryId(InventoryType.Chest, 2));
            chest.Slots[0].ItemId = Item.Sword;
            assets[new ChestId(AssetType.Chest, 2)] = chest;
            Add("C0", s =>
            {
                var openText = s("Opened chest 1");
                var unlockText = s("Unlocked chest 1");
                return @$"
    text {s("Opening chest 1")}
    if (open_chest 1 {mapId.ToMapText()} None 50 {openText} {unlockText}) {{
        text {s("Chest True")}
    }} else {{
        text {s("Chest False")}
    }}

    if (result) {{
        text {s("Result True")}
    }} else {{
        text {s("Result False")}
    }}
";
            });

            Add("C!", s =>
            {
                var openText = s("Opened chest 998");
                var unlockText = s("Unlocked chest 998");
                return @$"
    text {s("Opening chest 998")}
    open_chest 998 {mapId.ToMapText()} Item.FragrantWater 50 {openText} {unlockText}
";
            });

            while(map.Npcs.Count < 96)
                map.Npcs.Add(MapNpc.Default);

            Add("N0", s => @$"
    text {s("Setting NPC 0 inactive")}
    disable_npc 0
    disable_npc 95
    disable_npc 0 1 0 1
    disable_npc 95 1 0 512
");
            map.Npcs[0] = new MapNpc
            {
                Id = (NpcId)Npc.Christine,
                Type = NpcType.Npc,
                Movement = NpcMovement.Stationary,
                Waypoints = NpcPos(6, 4),
                SpriteOrGroup = (SpriteId)LargeNpc.Christine,
            };

            map.Npcs[95] = new MapNpc
            {
                Id = (NpcId)Npc.RainerHofstedt,
                Type = NpcType.Npc,
                Movement = NpcMovement.Stationary,
                Waypoints = NpcPos(6, 10),
                SpriteOrGroup = (SpriteId)LargeNpc.CrewMember3,
            };

            Add("EC0", s => $@"
    disable_event_chain {mapId.ToMapText()} 0 1 1
    disable_event_chain {mapId.ToMapText()} 0 1 {mapId.Id}
    disable_event_chain {mapId.ToMapText()} 249 1 512
");

        });

        var (finalMap, mapText) = builder.Build();
        assets.Add(finalMap.Id, finalMap);
        assets.Add(finalMap.Id.ToMapText(), mapText);
        return assets;
    }
}