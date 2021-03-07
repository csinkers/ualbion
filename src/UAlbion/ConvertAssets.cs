using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;

namespace UAlbion
{
    static class ConvertAssets
    {
        static (ModApplier, EventExchange) BuildModApplier(string baseDir, string mod, IFileSystem disk)
        {
            var config = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk);
            var applier = new ModApplier();
            var exchange = new EventExchange(new LogExchange());
            exchange
                .Register(disk)
                .Register<IGeneralConfig>(config)
                .Register<ICoreFactory>(new VeldridCoreFactory())
                .Attach(new StdioConsoleLogger())
                .Attach(new AssetLoaderRegistry())
                .Attach(new ContainerRegistry())
                .Attach(new PostProcessorRegistry())
                .Attach(new AssetLocator())
                .Attach(new SettingsManager(new GeneralSettings())) // Used for event comments
                .Attach(applier)
                ;
            applier.LoadMods(config, new[] { mod });
            return (applier, exchange);
        }

        public static void Convert(
            string baseDir,
            string fromMod,
            string toMod,
            string[] ids,
            ISet<AssetType> assetTypes)
        {
            var disk = new FileSystem();
            var (from, fromExchange) = BuildModApplier(baseDir, fromMod, disk);
            var (to, toExchange) = BuildModApplier(baseDir, toMod, disk);

            using (fromExchange)
            using (toExchange)
            {
                // Give the "from" universe's asset manager "to" the to exchange so we can import the assets.
                toExchange.Attach(new AssetManager(from)); 
                fromExchange.Attach(new AssetManager(from)); // From also needs an asset manager for the inventory post-processor etc

                var parsedIds = ids?.Select(AssetId.Parse).ToHashSet();
                var paletteHints = PaletteHints.Load(Path.Combine(baseDir, "mods", "Base", "palette_hints.json"), disk);
                var cache = new Dictionary<(AssetId, string), object>();

                (object, AssetInfo) LoaderFunc(AssetId id, string language)
                {
                    StringId? stringId = TextId.ValidTypes.Contains(id.Type)
                        ? (TextId)id
                        : (StringId?)null;

                    if (stringId != null)
                    {
                        if (stringId.Value.Id == id)
                            stringId = null;
                        else
                            id = stringId.Value.Id;
                    }

                    var info = from.GetAssetInfo(id, language);
                    var asset = cache.TryGetValue((id, language), out var cached)
                        ? cached
                        : cache[(id,language)] = from.LoadAsset(id, language);

                    if (stringId.HasValue && asset is IStringCollection collection)
                        asset = collection.GetString(stringId.Value, language);

                    return (asset, info);
                }

                to.SaveAssets(LoaderFunc, () => cache.Clear(), paletteHints, parsedIds, assetTypes);
            }
        }
    }
}
