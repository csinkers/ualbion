using UAlbion;
using UAlbion.Api;
using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Game.Assets;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public static class Program
{
    static void Main()
    {
        AssetSystem.LoadEvents();

        var disk = new FileSystem { CurrentDirectory = @"C:\Depot\bb\ualbion" };
        var baseExchange = AssetSystem.SetupSimple(disk, AssetMapping.Global, "Base");
        var testExchange = AssetSystem.SetupSimple(disk, AssetMapping.Global, "UATestDev");
        var repackedExchange = AssetSystem.SetupSimple(disk, AssetMapping.Global, "Repacked");
        baseExchange.Name = "BaseExchange";
        testExchange.Name = "TestExchange";
        repackedExchange.Name = "RepackedExchange";

        var assets = new Dictionary<AssetId, object>
        {
            [AssetId.FromUInt32(PaletteCommon.Id)] = PaletteCommon,
            [Palette1Id] = Palette1,
        };

        foreach (var kvp in Tileset1.Assets) assets[kvp.Key] = kvp.Value;
        foreach (var kvp in Lab1.Assets) assets[kvp.Key] = kvp.Value;

        void Merge(Dictionary<AssetId, object> newAssets)
        {
            foreach (var kvp in newAssets)
                assets[kvp.Key] = kvp.Value;
        }

        Merge(JumpMap.Build((Map)300, (Map)101, 5, 5));
        Merge(NpcMap.Build((Map)301));
        Merge(FlagTestMap.Build((Map)100));
        Merge(Test3DMap.Build((Map)101));

        (object? asset, AssetInfo? info) LoaderFunc(AssetId id, string lang)
            => assets.TryGetValue(id, out var asset)
                ? (asset, new AssetInfo(new Dictionary<string, object> { { AssetProperty.PaletteId, Palette1Id.Id } }))
                : (null, null);

        // Create 3D lab graphics
        // Create 3D lab data
        // Create item data
        // Create player graphics
        //Tileset1.TilesetGfx.ToBitmap().Dump();

        /*NPC Test maps:
        npc_on / npc_off
        npc_lock / npc_unlock
        npc_jump
        npc_move
        npc_turn
        npc_text
        is_npc_active_on_map
        is_npc_active
        is_npc_x
        is_npc_y
        disable_npc
        change_icon npcnum 0 AbsTemp NpcMovement 0..3
        change_icon npcnum 0 AbsTemp NpcSprite id

        types: party, npc, monster, prop
        movement: wp, static, random, chase

        mem monitoring, movement details
         */

        testExchange.Resolve<IModApplier>().SaveAssets(LoaderFunc, () => { }, assets.Keys.ToHashSet(), null, null);
        repackedExchange.Resolve<IModApplier>().SaveAssets(LoaderFunc, () => { }, assets.Keys.ToHashSet(), null, null);
        Console.WriteLine("Done");
    }
}