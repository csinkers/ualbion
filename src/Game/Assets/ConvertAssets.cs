using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets
{
    public static class ConvertAssets
    {
        static (ModApplier, EventExchange, AssetLoaderRegistry) BuildModApplier(string baseDir, string mod, IFileSystem disk)
        {
            var config = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk);
            var applier = new ModApplier();
            var exchange = new EventExchange(new LogExchange());
            var assetLoaderRegistry = new AssetLoaderRegistry();
            exchange
                .Register(disk)
                .Register<IGeneralConfig>(config)
                .Attach(new StdioConsoleLogger())
                .Attach(assetLoaderRegistry)
                .Attach(new ContainerRegistry())
                .Attach(new PostProcessorRegistry())
                .Attach(new AssetLocator())
                .Attach(new SettingsManager(new GeneralSettings())) // Used for event comments
                .Attach(applier)
                ;
            applier.LoadMods(config, new[] { mod });
            return (applier, exchange, assetLoaderRegistry);
        }

        public static void Convert(IFileSystem disk,
            string fromMod,
            string toMod,
            string[] ids,
            ISet<AssetType> assetTypes,
            Regex filePattern)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            var baseDir = ConfigUtil.FindBasePath(disk);
            var (from, fromExchange, fromLoaderRegistry) = BuildModApplier(baseDir, fromMod, disk);
            var (to, toExchange, toLoaderRegistry) = BuildModApplier(baseDir, toMod, disk);

            using (fromExchange)
            using (fromLoaderRegistry)
            using (toExchange)
            using (toLoaderRegistry)
            {
                // Give the "from" universe's asset manager "to" the to exchange so we can import the assets.
                toExchange.Attach(new AssetManager(from)); 
                fromExchange.Attach(new AssetManager(from)); // From also needs an asset manager for the inventory post-processor etc

                var parsedIds = ids?.Select(AssetId.Parse).ToHashSet();
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

                to.SaveAssets(LoaderFunc, () => cache.Clear(), parsedIds, assetTypes, filePattern);
            }
        }
    }
}
