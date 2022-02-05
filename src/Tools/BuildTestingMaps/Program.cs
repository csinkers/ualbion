using UAlbion;
using UAlbion.Api;
using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats.Assets;
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

        var assets = new Dictionary<AssetId, object>
        {
            [AssetId.FromUInt32(PaletteCommon.Id)] = PaletteCommon,
            [Palette1Id] = Palette1,
            [Tileset1.Tileset.Id] = Tileset1.Tileset,
            [(SpriteId)Tileset1.TilesetGfx.Id] = Tileset1.TilesetGfx,
        };

        void Merge(Dictionary<AssetId, object> newAssets)
        {
            foreach (var kvp in newAssets)
                assets[kvp.Key] = kvp.Value;
        }

        Merge(JumpMap.Build((Map)300, (Map)100, 5, 5));
        Merge(NpcMap.Build((Map)301));
        Merge(FlagTestMap.Build((Map)100));

        (object? asset, AssetInfo? info) LoaderFunc(AssetId id, string lang)
            => assets.TryGetValue(id, out var asset)
                ? (asset, new AssetInfo(new Dictionary<string, object> { { AssetProperty.PaletteId, Palette1Id.Id } }))
                : (null, null);

        // Create 3D lab graphics
        // Create 3D lab data
        // Create item data
        // Create player graphics
        //Tileset1.TilesetGfx.ToBitmap().Dump();

        testExchange.Resolve<IModApplier>().SaveAssets(LoaderFunc, () => { }, assets.Keys.ToHashSet(), null, null);
        repackedExchange.Resolve<IModApplier>().SaveAssets(LoaderFunc, () => { }, assets.Keys.ToHashSet(), null, null);
        Console.WriteLine("Done");
    }
}