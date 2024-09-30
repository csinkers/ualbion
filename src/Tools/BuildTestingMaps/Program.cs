using UAlbion;
using UAlbion.Api;
using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public static class Program
{
    static void Main()
    {
        AssetSystem.LoadEvents();

        var disk = new FileSystem(@"C:\Depot\bb\ualbion");
        var baseExchange = AssetSystem.SetupSimple(disk, AssetMapping.Global, "Albion");
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

        var assetManager = baseExchange.Resolve<IAssetManager>();

        var tileset1 = new TestTilemap(assets, assetManager);
        // var lab1 = new TestLab(assets, assetManager);

        void Merge(Dictionary<AssetId, object> newAssets)
        {
            foreach (var kvp in newAssets)
                assets[kvp.Key] = kvp.Value;
        }

        // Merge(NpcMap.Build((Map)300, tileset1));
        Merge(FlagTestMap.Build((Map)100, tileset1));
        // Merge(Test3DMap.Build((Map)101, lab1));
        // Merge(EventMap.Build((Map)302, tileset1));
        // Merge(JumpMap.Build((Map)300, new[]
        // {
        //     ((MapId)(Map)301, "NPC test map"),
        //     ((Map)100, "Flag test map"),
        //     ((Map)101, "3D test map"),
        //     ((Map)302, "Event test map"),
        // }, tileset1));

        Merge(AutoJumpMap.Build((Map)300, (Map)100, 7, 7, tileset1));

        AssetLoadResult LoaderFunc(AssetId assetId, string language)
        {
            var node = new AssetNode(assetId);
            node.SetProperty(AssetProps.Palette, Palette1Id);
            return assets.TryGetValue(assetId, out var asset)
                ? new AssetLoadResult(assetId, asset, null)
                : null!;
        }

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

        var options = new AssetConversionOptions(LoaderFunc, () => { }, assets.Keys.ToHashSet(), null, null, null);
        testExchange.Resolve<IModApplier>().SaveAssets(options);
        repackedExchange.Resolve<IModApplier>().SaveAssets(options);
        Console.WriteLine("Done");
    }
}