using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Containers;
using UAlbion.Game.Assets;

namespace UAlbion.SingleAssetConverter;

static class Program
{
    static readonly AssetLoaderRegistry Registry = new();
    static readonly Dictionary<string, string> Loaders = new()
    {
        { "amorphous", "UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats" },
        { "automap", "UAlbion.Formats.Parsers.AutomapLoader, UAlbion.Formats" },
        { "block", "UAlbion.Formats.Parsers.BlockListLoader, UAlbion.Formats" },
        { "chest", "UAlbion.Formats.Parsers.ChestLoader, UAlbion.Formats" },
        { "dummy", "UAlbion.Formats.Parsers.DummyLoader, UAlbion.Formats" },
        { "eset", "UAlbion.Formats.Parsers.EventSetLoader, UAlbion.Formats" },
        { "eventSetScript", "UAlbion.Game.Assets.EventSetScriptLoader, UAlbion.Game" },
        { "fixedsize", "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats" },
        { "flic", "UAlbion.Formats.Parsers.FlicLoader, UAlbion.Formats" },
        { "afont", "UAlbion.Game.Assets.FontSpriteLoader`1[[UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats]], UAlbion.Game" },
        { "vfont", "UAlbion.Game.Assets.FontSpriteLoader`1[[UAlbion.Game.Veldrid.Assets.PngSheetLoader, UAlbion.Game.Veldrid]], UAlbion.Game" },
        { "header", "UAlbion.Formats.Parsers.SingleHeaderSpriteLoader, UAlbion.Formats" },
        { "interlaced", "UAlbion.Formats.Parsers.InterlacedBitmapLoader, UAlbion.Formats" },
        { "itemNameCollector", "UAlbion.Game.Assets.ItemNameCollector, UAlbion.Game" },
        { "itemdata", "UAlbion.Game.Assets.ItemDataLoader, UAlbion.Game" },
        { "itemname", "UAlbion.Formats.Parsers.ItemNameLoader, UAlbion.Formats" },
        { "json", "UAlbion.Formats.Parsers.JsonStringLoader, UAlbion.Formats" },
        { "jsonEventSet", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.EventSet, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonInv", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.Inventory, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonItems", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.ItemData, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonLab", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.Labyrinth.LabyrinthData, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonMonsterGroup", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.MonsterGroup, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonPal", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.AlbionPalette, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonSheet", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.CharacterSheet, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonSpell", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.SpellData, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonText", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.ListStringCollection, UAlbion.Formats]], UAlbion.Formats" },
        { "jsonTileset", "UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.Maps.TilesetData, UAlbion.Formats]], UAlbion.Formats" },
        { "lab", "UAlbion.Formats.Parsers.LabyrinthDataLoader, UAlbion.Formats" },
        { "map", "UAlbion.Formats.Parsers.MapLoader, UAlbion.Formats" },
        { "merchant", "UAlbion.Formats.Parsers.MerchantLoader, UAlbion.Formats" },
        { "mongrp", "UAlbion.Formats.Parsers.MonsterGroupLoader, UAlbion.Formats" },
        { "multiheader", "UAlbion.Formats.Parsers.MultiHeaderSpriteLoader, UAlbion.Formats" },
        { "pal", "UAlbion.Formats.Parsers.PaletteLoader, UAlbion.Formats" },
        { "png", "UAlbion.Game.Veldrid.Assets.PngLoader, UAlbion.Game.Veldrid" },
        { "pngsheet", "UAlbion.Game.Veldrid.Assets.PngSheetLoader, UAlbion.Game.Veldrid" },
        { "raw", "UAlbion.Formats.Parsers.RawLoader, UAlbion.Formats" },
        { "sample", "UAlbion.Formats.Parsers.SampleLoader, UAlbion.Formats" },
        { "script", "UAlbion.Formats.Parsers.ScriptLoader, UAlbion.Formats" },
        { "sheet", "UAlbion.Game.Assets.CharacterSheetLoader, UAlbion.Game" },
        { "slab", "UAlbion.Formats.Parsers.SlabLoader, UAlbion.Formats" },
        { "song", "UAlbion.Formats.Parsers.SongLoader, UAlbion.Formats" },
        { "soundbank", "UAlbion.Game.Assets.SoundBankLoader, UAlbion.Game" },
        { "spell", "UAlbion.Formats.Parsers.SpellLoader, UAlbion.Formats" },
        { "stext", "UAlbion.Formats.Parsers.SystemTextLoader, UAlbion.Formats" },
        { "stringtable", "UAlbion.Formats.Parsers.AlbionStringTableLoader, UAlbion.Formats" },
        { "tiledLabyrinth", "UAlbion.Game.Veldrid.Assets.IsometricLabyrinthLoader, UAlbion.Game.Veldrid" },
        { "tiledMap", "UAlbion.Game.Assets.TiledMapLoader, UAlbion.Game" },
        { "tiledNpcTileset", "UAlbion.Game.Veldrid.Assets.NpcTilesetLoader, UAlbion.Game.Veldrid" },
        { "tiledStamp", "UAlbion.Formats.Exporters.Tiled.StampLoader, UAlbion.Formats" },
        { "tiledTileGfx", "UAlbion.Formats.Exporters.Tiled.TileGraphicsLoader, UAlbion.Formats" },
        { "tiledTileset", "UAlbion.Game.Assets.TiledTilesetLoader, UAlbion.Game" },
        { "tileset", "UAlbion.Formats.Parsers.TilesetLoader, UAlbion.Formats" },
        { "utf8", "UAlbion.Formats.Parsers.Utf8Loader, UAlbion.Formats" },
        { "wav", "UAlbion.Formats.Parsers.WavLoader, UAlbion.Formats" },
        { "wavlib", "UAlbion.Formats.Parsers.WaveLibLoader, UAlbion.Formats" },
        { "wavlibwav", "UAlbion.Formats.Parsers.WaveLibWavLoader, UAlbion.Formats" },
        { "wordCollector", "UAlbion.Game.Assets.WordCollector, UAlbion.Game" },
        { "wordlist", "UAlbion.Formats.Parsers.WordListLoader, UAlbion.Formats" },
    };

    record Options(IAssetLoader Loader, IAssetLoader Saver, string InputPath, string OutputPath, int? SubItem, AssetInfo Info);
    static Options? ParseCommandLine(string[] args)
    {
        if (args.Length < 4) { Console.WriteLine("Not enough arguments"); return null; }

        var inputPath = args[0];
        var outputPath = args[1];

        int index = inputPath.LastIndexOf(':');
        int? subItem = null;
        if (index != -1)
        {
            subItem = int.Parse(inputPath[(index + 1)..]);
            inputPath = inputPath[..index];
        }

        var loaderAlias = args[2].ToLowerInvariant();
        var saverAlias = args[3].ToLowerInvariant();
        var loader = Resolve(loaderAlias);
        var saver = Resolve(saverAlias);

        if (loader == null) { Console.WriteLine($"Could not find loader \"{loaderAlias}\""); return null; } 
        if (saver == null) { Console.WriteLine($"Could not find loader \"{saverAlias}\""); return null; } 
        if (!File.Exists(inputPath)) { Console.WriteLine($"Could not find file {inputPath}"); return null; }

        var info = new AssetInfo();
        for (int i = 4; i < args.Length; i++)
        {
            var arg = args[i];
            index = arg.IndexOf('=');
            if (index == -1)
                continue;

            var key = arg[..index];
            var value = arg[(index + 1)..];
            if (int.TryParse(value, out var asInt))
                info.Set(key, asInt);
            else
                info.Set(key, value);
        }

        return new Options(loader, saver, inputPath, outputPath, subItem, info);
    }

    public static void Main(string[] args)
    {
        var options = ParseCommandLine(args);
        if (options == null)
        {
            ShowUsage();
            return;
        }

        var exchange = new EventExchange();
        var assets = new DummyAssetManager();
        exchange.Register<IAssetManager>(assets);

        if (options.Loader is IComponent loaderComponent)
            exchange.Attach(loaderComponent);

        if (options.Saver is IComponent saverComponent)
            exchange.Attach(saverComponent);

        var disk = new FileSystem();
        var jsonUtil = new FormatJsonUtil();
        var container = options.SubItem.HasValue ? (IAssetContainer)new XldContainer() : new RawContainer();
        if (options.SubItem.HasValue)
            options.Info.Index = options.SubItem.Value;

        using var inputSerializer = container.Read(options.InputPath, options.Info, disk, jsonUtil);
        if (inputSerializer == null)
        {
            Console.WriteLine($"Could not extract sub-asset {options.SubItem} from {options.InputPath}");
            return;
        }

        var context = new LoaderContext(assets, jsonUtil, AssetMapping.Global);
        var asset = options.Loader.Serdes(null, options.Info, inputSerializer, context);
        File.WriteAllBytes(options.OutputPath, FormatUtil.SerializeToBytes(s => options.Saver.Serdes(asset, options.Info, s, context)));
    }

    static IAssetLoader? Resolve(string alias) => !Loaders.TryGetValue(alias, out var loaderName) ? null : Registry.GetLoader(loaderName);

    static void ShowUsage()
    {
        Console.WriteLine("Usage: ua <InputFile> <OutputFile> <Loader> <Saver> [Properties]");
        Console.WriteLine("where InputFile can be an XLD with sub-item identified like FOO.XLD:23");
        Console.WriteLine("and each property is of the form Key=Value");
        Console.WriteLine("e.g. ua 3DOBJEC2.XLD:90 test.png fixedSize png Width=57");
        Console.WriteLine();
        Console.WriteLine("Valid loaders/savers:");
        var maxLen = Loaders.Max(x => x.Key.Length);
        foreach (var loader in Loaders.OrderBy(x => x.Key))
            Console.WriteLine($"    {loader.Key.PadRight(maxLen)} : {loader.Value}");
    }
}