using UAlbion.Api;
using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Parsers;
using static BuildTestingMaps.Constants;
using MonsterGroup = UAlbion.Base.MonsterGroup;

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
        int nextScriptId = 1;
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

            ushort n = 0;
/*
            void Add(int x, int y, string name, Func<Func<string, int>, string> scriptBuilder)
            {
                for (var index = 0; index < name.Length; index++)
                {
                    var c = name[index];
                    map.Underlay[Pos(x + index, y)] = Tileset1.IndexForChar(c);
                }

                builder!.SetChain(n, scriptBuilder);
                map.AddZone((byte)x, (byte)y, TriggerTypes.Manipulate, n);
                n++;
            }

            string Script(Func<Func<string, int>, string> scriptBuilder)
            {
                var text = scriptBuilder(builder!.AddMapText);
                var script = ScriptLoader.Parse(ApiUtil.SplitLines(text));
                var scriptId = new ScriptId(AssetType.Script, nextScriptId++);
                assets![scriptId] = script;
                return "do_script " + scriptId.Id;
            }

            Add(1, n+1, "S0", s => @$"
    text {s("Setting switch 0")}
    switch 1 0
");

            Add(1, n+1, "S!", s => @$"
    text {s("Setting switch 1023")}
    switch 1 1023
");

            Add(1, n+1, "D0", s =>
            {
                var openText = s("Opened door 0");
                var unlockText = s("Unlocked door 0");
                return @$"
    text {s("Opening door 0")}
    open_door 0 {mapId.ToMapText()} Item.FragrantWater 50 {openText} {unlockText}
";
            });

            Add(1, n+1, "D!", s =>
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
            Add(1, n+1, "C0", s =>
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

            Add(1, n+1, "C!", s =>
            {
                var openText = s("Opened chest 998");
                var unlockText = s("Unlocked chest 998");
                return @$"
    text {s("Opening chest 998")}
    open_chest 998 {mapId.ToMapText()} Item.FragrantWater 50 {openText} {unlockText}
";
            });

            Add(1, n+1, "N0", s => @$"
    text {s("Setting NPC 0 inactive")}
    modify_npc_off Set 0
    modify_npc_off Set 95
");

            var waypoints = BuildPatrolPath(18, 6);
            map.Npcs[0] = new MapNpc
            {
                Id = (MonsterGroupId)MonsterGroup.TwoSkrinn1OneKrondir1,
                Type = NpcType.Monster,
                Movement = NpcMovement.Waypoints,
                Waypoints = waypoints,
                SpriteOrGroup = (SpriteId)LargeNpc.Skrinn,
            };

            Add(1, n+1, "EC0", s => $@"
    chain_off Set 0
");

            Add( 9, 5, "<", _ => Script(s => $@"
npc_lock 0
npc_move 0 -4  0
update 1
text {s("A")}
update 1
text {s("B")}
update 1
text {s("C")}
update 1
text {s("D")}
npc_unlock 0
"));
            Add(10, 4, "^", _ => Script(_ => @"
npc_lock 0
npc_move 0  0 -2
update 4
npc_unlock 0
"));
            Add(11, 5, ">", _ => Script(_ => @"
npc_lock 0
npc_move 0  2  0
update 4
npc_unlock 0
"));
            Add(10, 6, "v", _ => Script(_ => @"
npc_lock 0
npc_move 0  0  2
update 4
npc_unlock 0
"));
            Add( 9, 4, "m", s => $@"
if (get_ticker 100 Equals 0) {{
    text {s("Setting movement to 1 (Random)")}
    ticker 100 SetAmount 1
    change_icon 0 0 AbsTemp NpcMovement 1
}} else if (get_ticker 100 Equals 1) {{ 
    text {s("Setting movement to 2 (Stationary)")}
    ticker 100 SetAmount 2
    change_icon 0 0 AbsTemp NpcMovement 2
}} else if (get_ticker 100 Equals 2) {{ 
    text {s("Setting movement to 3 (Chase)")}
    ticker 100 SetAmount 3
    change_icon 0 0 AbsTemp NpcMovement 3
}} else if (get_ticker 100 Equals 3) {{ 
    text {s("Setting movement to 4 (Unk4)")}
    ticker 100 SetAmount 4
    change_icon 0 0 AbsTemp NpcMovement 4
}} else if (get_ticker 100 Equals 4) {{ 
    text {s("Setting movement to 5 (Unk5)")}
    ticker 100 SetAmount 5
    change_icon 0 0 AbsTemp NpcMovement 5
}} else if (get_ticker 100 Equals 5) {{ 
    text {s("Setting movement to 6 (Unk6)")}
    ticker 100 SetAmount 6
    change_icon 0 0 AbsTemp NpcMovement 6
}} else if (get_ticker 100 Equals 6) {{ 
    text {s("Setting movement to 7 (Unk7)")}
    ticker 100 SetAmount 7
    change_icon 0 0 AbsTemp NpcMovement 7
}} else if (get_ticker 100 Equals 7) {{ 
    text {s("Setting movement to 8 (Unk8)")}
    ticker 100 SetAmount 8
    change_icon 0 0 AbsTemp NpcMovement 8
}} else if (get_ticker 100 Equals 8) {{ 
    text {s("Setting movement to 9 (Unk9)")}
    ticker 100 SetAmount 9
    change_icon 0 0 AbsTemp NpcMovement 9
}} else if (get_ticker 100 Equals 9) {{ 
    text {s("Setting movement to 10 (Unk10)")}
    ticker 100 SetAmount 10
    change_icon 0 0 AbsTemp NpcMovement 10
}} else {{
    text {s("Setting movement to 0 (Waypoints)")}
    ticker 100 SetAmount 0
    change_icon 0 0 AbsTemp NpcMovement 0
}}");

            Add(11, 4, "s", s => $@"
if (get_ticker 101 Equals 0) {{
    text {s("Setting sprite to 26 (Rainer)")}
    ticker 101 SetAmount 1
    change_icon 0 0 AbsTemp NpcSprite 26
}} else {{
    text {s("Setting sprite to 21 (Christine)")}
    ticker 101 SetAmount 0
    change_icon 0 0 AbsTemp NpcSprite 21
}}
");
            Add(9, 6, "L",  _ => Script(_ => "npc_lock 0"));
            Add(11, 6, "U", _ => Script(_ => "npc_unlock 0"));
            Add(9, 7, "+",  _ => Script(_ => "npc_on 0"));
            Add(11, 7, "-", _ => Script(_ => "npc_off 0"));

            Add(13, 5, "<", _ => Script(_ => "npc_turn 0 3"));
            Add(14, 4, "^", _ => Script(_ => "npc_turn 0 0"));
            Add(15, 5, ">", _ => Script(_ => "npc_turn 0 1"));
            Add(14, 6, "v", _ => Script(_ => "npc_turn 0 2"));
*/
            MajMin(8, 1, (i, j) =>
            {
                int x0 = 2 + i;
                int y0 = 2 + j;
                int num = i; // + j * 4;
                map.Overlay[Pos(x0, y0)]     = Tileset1.AnimLoopOffset  + num;
                map.Overlay[Pos(x0, y0 + 1)] = Tileset1.AnimLoopOverlayOffset  + num;
                map.Overlay[Pos(x0, y0 + 4)] = Tileset1.AnimCycleOffset + num;
                map.Overlay[Pos(x0, y0 + 5)] = Tileset1.AnimCycleOverlayOffset + num;
            });
        });

        var (finalMap, mapText) = builder.Build();
        assets.Add(finalMap.Id, finalMap);
        assets.Add(finalMap.Id.ToMapText(), mapText);
        return assets;
    }
}