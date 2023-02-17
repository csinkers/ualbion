using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public static class FlagTestMap
{
    const byte MapWidth = 36;
    const byte MapHeight = 20;
    static int Pos(int x, int y) => y * MapWidth + x;
    public static Dictionary<AssetId, object> Build(MapId mapId, TestTilemap tileset1)
    {
        if (tileset1 == null) throw new ArgumentNullException(nameof(tileset1));

        var assets = new Dictionary<AssetId, object>();
        var builder = new MapBuilder2D(mapId, Palette1Id, tileset1, MapWidth, MapHeight);
        int nextScriptId = 2;

        builder.DrawBorder();
        builder.Draw(map =>
        {
            map.Flags |= MapFlags.Unk8000;  

            ushort n = 0;
//* -- For investigating NPC behaviour and events --
            void Add(int x, int y, string name, string label, ScriptBuilderFunc scriptBuilder)
            {
                string IfBuilder(StringCreationFunc s) => $@"if (verb Examine) {{
    text {s(label)}
}} else {{
    {scriptBuilder(s)}
}}";
                for (var index = 0; index < name.Length; index++)
                {
                    var c = name[index];
                    map.Tiles[Pos(x + index, y)].Underlay = tileset1.IndexForChar(c);
                }

                builder.SetChain(n, IfBuilder);
                map.AddZone((byte)x, (byte)y, TriggerTypes.Examine | TriggerTypes.Manipulate, n);
                n++;
            }

            string Script(ScriptBuilderFunc scriptBuilder)
            {
                var text = scriptBuilder(builder.AddMapText);
                var script = ScriptLoader.Parse(text);
                var scriptId = new ScriptId(nextScriptId++);
                assets[scriptId] = script;
                return "do_script " + scriptId.Id;
            }

            Add(1, n+1, "S0", "Set switch 0", s => @$"
    text {s("Setting switch 0")}
    switch 1 0
");

            Add(1, n+1, "S!", "Set switch 1023", s => @$"
    text {s("Setting switch 1023")}
    switch 1 1023
");

            Add(1, n+1, "D0", "Open door 0", s =>
            {
                var openText = s("Opened door 0");
                var unlockText = s("Unlocked door 0");
                return @$"
    text {s("Opening door 0")}
    door 0 Item.FragrantWater 50 {openText} {unlockText}
";
            });

            Add(1, n+1, "D!", "Open door 998", s =>
            {
                var openText = s("Opened door 998");
                var unlockText = s("Unlocked door 998");
                return @$"
    text {s("Opening door 998")}
    door 998 Item.FragrantWater 50 {openText} {unlockText}
";
            });

            var chest = new Inventory(new InventoryId(InventoryType.Chest, 2));
            chest.Slots[0].Item = Item.Sword;
            assets[new ChestId(2)] = chest;
            Add(1, n+1, "C0", "Open chest 1", s =>
            {
                var openText = s("Opened chest 1");
                var unlockText = s("Unlocked chest 1");
                return @$"
    text {s("Opening chest 1")}
    if (chest 1 None 50 {openText} {unlockText}) {{
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

            Add(1, n+1, "C!", "Open chest 998", s =>
            {
                var openText = s("Opened chest 998");
                var unlockText = s("Unlocked chest 998");
                return @$"
    text {s("Opening chest 998")}
    chest 998 Item.FragrantWater 50 {openText} {unlockText}
";
            });

            Add(1, n+1, "N0", "Set NPC0 inactive", s => @$"
    text {s("Setting NPC 0 inactive")}
    modify_npc_off Set 0
    modify_npc_off Set 95
");

            var waypoints = BuildPatrolPath(18, 6);
            map.Npcs[0] = new MapNpc
            {
                Id = (MonsterGroupId)UAlbion.Base.MonsterGroup.TwoSkrinn1OneKrondir1,
                Type = NpcType.Monster,
                Movement = NpcMovement.Waypoints,
                Waypoints = waypoints,
                SpriteOrGroup = (SpriteId)NpcLargeGfx.Skrinn,
            };

            Add(1, n+1, "EC0", "Chain 0 off", _ => @"
    chain_off Set 0
");

            Add( 9, 5, "<", "NPC0 left", _ => Script(_ => @"
npc_lock 0
npc_move 0 -1  0
update 4
npc_move 0 -1  0
update 4
npc_move 0 -1  0
update 4
npc_move 0 -1  0
update 4
npc_move 0 -1  0
update 4
npc_move 0 -1  0
update 4
npc_unlock 0
"));
            Add(10, 4, "^", "NPC0 up", _ => Script(_ => @"
npc_lock 0
npc_move 0  0 -1
update 4
npc_unlock 0
"));
            Add(11, 5, ">", "NPC0 rigth", _ => Script(_ => @"
npc_lock 0
npc_move 0  1  0
update 4
npc_unlock 0
"));
            Add(10, 6, "v", "NPC0 down", _ => Script(_ => @"
npc_lock 0
npc_move 0  0  1
update 4
npc_unlock 0
"));

            Add(5, 2, "m0",  "Setting movement to 0 (Waypoints)",  _ => "change_npc_movement 0 0 AbsTemp");
            Add(5, 3, "m1",  "Setting movement to 1 (Random)",     _ => "change_npc_movement 0 1 AbsTemp");
            Add(5, 4, "m2",  "Setting movement to 2 (Stationary)", _ => "change_npc_movement 0 2 AbsTemp");
            Add(5, 5, "m3",  "Setting movement to 3 (Chase)",      _ => "change_npc_movement 0 3 AbsTemp");
            Add(5, 6, "m4",  "Setting movement to 4 (Unk4)",       _ => "change_npc_movement 0 4 AbsTemp");
            Add(5, 7, "m5",  "Setting movement to 5 (Unk5)",       _ => "change_npc_movement 0 5 AbsTemp");
            Add(5, 8, "m6",  "Setting movement to 6 (Unk6)",       _ => "change_npc_movement 0 6 AbsTemp");
            Add(5, 9, "m7",  "Setting movement to 7 (Unk7)",       _ => "change_npc_movement 0 7 AbsTemp");
            Add(5, 10, "m8", "Setting movement to 8 (Unk8)",       _ => "change_npc_movement 0 8 AbsTemp");
            Add(5, 11, "m9", "Setting movement to 9 (Unk9)",       _ => "change_npc_movement 0 9 AbsTemp");
            Add(5, 12, "mA", "Setting movement to 10 (Unk10)",     _ => "change_npc_movement 0 10 AbsTemp");

            Add(11, 4, "s", "Cycle sprite", s => $@"
if (get_ticker 101 Equals 0) {{
    text {s("Setting sprite to 26 (Rainer)")}
    ticker 101 SetAmount 1
    change_npc_sprite 0 NpcLargeGfx.Rainer AbsTemp
}} else {{
    text {s("Setting sprite to 21 (Christine)")}
    ticker 101 SetAmount 0
    change_npc_sprite 0 NpcLargeGfx.Christine AbsTemp
}}
");
            Add(9, 6, "L",  "Lock NPC0", _ => Script(_ => "npc_lock 0"));
            Add(11, 6, "U", "Unlock NPC0", _ => Script(_ => "npc_unlock 0"));
            Add(9, 7, "+",  "NPC0 on", _ => Script(_ => "npc_on 0"));
            Add(11, 7, "-", "NPC0 off", _ => Script(_ => "npc_off 0"));

            Add(13, 5, "<", "NPC0 turn west",  _ => Script(_ => "npc_turn 0 3"));
            Add(14, 4, "^", "NPC0 turn north", _ => Script(_ => "npc_turn 0 0"));
            Add(15, 5, ">", "NPC0 turn east",  _ => Script(_ => "npc_turn 0 1"));
            Add(14, 6, "v", "NPC0 turn south", _ => Script(_ => "npc_turn 0 2"));

            Add(9, 9, "11", "Jump NPC0 to (1,1)", _ => Script(_ => "npc_jump 0 1 1"));
            Add(9, 10, "66", "Jump NPC0 to (6,6)", _ => Script(_ => "npc_jump 0 6 6"));
//*/

/* -- For investigating map tile animation flags --
            MajMin(8, 1, (i, j) =>
            {
                int x0 = 2 + i;
                int y0 = 2 + j;
                int num = i; // + j * 4;
                map.Tiles[Pos(x0, y0)].Overlay     = (ushort)(tileset1.AnimLoopOffset  + num);
                map.Tiles[Pos(x0, y0 + 1)].Overlay = (ushort)(tileset1.AnimLoopOverlayOffset  + num);
                map.Tiles[Pos(x0, y0 + 4)].Overlay = (ushort)(tileset1.AnimCycleOffset + num);
                map.Tiles[Pos(x0, y0 + 5)].Overlay = (ushort)(tileset1.AnimCycleOverlayOffset + num);
            }); //*/
/*
            MajMin(8, 8, (i, j) =>
            {
                int x0 = 2 + i;
                int y0 = 2 + j;
                for (int unk7 = 0; unk7 < 8; unk7++)
                    map.Tiles[Pos(x0 + (unk7 % 4) * 9, y0 + (unk7 / 4) * 9)].Overlay = (ushort)(tileset1.Unk7Type0Offset + unk7);
            }); //*/
        });

        var (finalMap, mapText) = builder.Build();
        assets.Add(finalMap.Id, finalMap);
        assets.Add(finalMap.Id.ToMapText(), mapText);
        return assets;
    }
}
