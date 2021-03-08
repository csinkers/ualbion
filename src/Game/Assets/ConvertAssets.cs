using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets
{
    public static class ConvertAssets
    {
        static (ModApplier, EventExchange) BuildModApplier(string baseDir, string mod, IFileSystem disk, ICoreFactory factory)
        {
            var config = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk);
            var applier = new ModApplier();
            var exchange = new EventExchange(new LogExchange());
            exchange
                .Register(disk)
                .Register<IGeneralConfig>(config)
                .Register(factory)
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
            IFileSystem disk,
            ICoreFactory factory,
            string fromMod,
            string toMod,
            string[] ids,
            ISet<AssetType> assetTypes)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var baseDir = ConfigUtil.FindBasePath(disk);
            var (from, fromExchange) = BuildModApplier(baseDir, fromMod, disk, factory);
            var (to, toExchange) = BuildModApplier(baseDir, toMod, disk, factory);

            using (fromExchange)
            using (toExchange)
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

                to.SaveAssets(LoaderFunc, () => cache.Clear(), parsedIds, assetTypes);
            }
        }
    }
}
