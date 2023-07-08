using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Containers;
using UAlbion.Game.Assets;

namespace UAlbion.SingleAssetConverter;

static class Program
{
    #if false // TODO
    static readonly AssetLoaderRegistry Registry = new();
    record Options(IAssetLoader Loader, IAssetLoader Saver, string InputPath, string OutputPath, int? SubItem, AssetNode Info);
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

        var node = new AssetNode();
        for (int i = 4; i < args.Length; i++)
        {
            var arg = args[i];
            index = arg.IndexOf('=');
            if (index == -1)
                continue;

            var key = arg[..index];
            var value = arg[(index + 1)..];
            if (int.TryParse(value, out var asInt))
                node.SetProperty(key, asInt);
            else
                node.SetProperty(key, value);
        }

        return new Options(loader, saver, inputPath, outputPath, subItem, info);
    }

    #endif
    public static void Main(string[] args)
    {
        #if false
        var options = ParseCommandLine(args);
        if (options == null)
        {
            ShowUsage();
            return;
        }

        var exchange = new EventExchange();
        var assets = new StubAssetManager();
        exchange.Register<IAssetManager>(assets);

        if (options.Loader is IComponent loaderComponent)
            exchange.Attach(loaderComponent);

        if (options.Saver is IComponent saverComponent)
            exchange.Attach(saverComponent);

        var disk = new FileSystem(Directory.GetCurrentDirectory());
        var jsonUtil = new FormatJsonUtil();
        var modContext = new ModContext("AdHoc", jsonUtil, disk, AssetMapping.Global);

        var container = options.SubItem.HasValue ? (IAssetContainer)new XldContainer() : new RawContainer();
        if (options.SubItem.HasValue)
            options.Info.Index = options.SubItem.Value;

        var context = new AssetLoadContext(options.AssetId, options.Info, modContext);
        using var inputSerializer = container.Read(options.InputPath, context);
        if (inputSerializer == null)
        {
            Console.WriteLine($"Could not extract sub-asset {options.SubItem} from {options.InputPath}");
            return;
        }

        var asset = options.Loader.Serdes(null, inputSerializer, context);
        File.WriteAllBytes(options.OutputPath, FormatUtil.SerializeToBytes(s => options.Saver.Serdes(asset, s, context)));
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
            #endif
    }
}