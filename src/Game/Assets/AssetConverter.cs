using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Game.Magic;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets;

public sealed class AssetConverter : IDisposable
{
    readonly ModApplier _from;
    readonly ModApplier _to;
    readonly EventExchange _fromExchange;
    readonly EventExchange _toExchange;
    readonly AssetLoaderRegistry _fromLoaderRegistry;
    readonly AssetLoaderRegistry _toLoaderRegistry;

    static (ModApplier, EventExchange, AssetLoaderRegistry) BuildModApplier(string baseDir, string[] mods, IFileSystem disk, IJsonUtil jsonUtil, AssetMapping mapping)
    {
        var config = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk, jsonUtil);
        var applier = new ModApplier();
        var exchange = new EventExchange(new LogExchange()) { Name = $"EventExchange for {string.Join(", ", mods)}"};
        var assetLoaderRegistry = new AssetLoaderRegistry();
        exchange
            .Register(disk)
            .Register(jsonUtil)
            .Register<IGeneralConfig>(config)
            .Attach(new StdioConsoleLogger())
            .Attach(assetLoaderRegistry)
            .Attach(new ContainerRegistry())
            .Attach(new PostProcessorRegistry())
            .Attach(new AssetLocator())
            .Attach(new SpellManager())
            .Attach(new SettingsManager(new GeneralSettings())) // Used for event comments
            .Attach(applier)
            ;

        applier.LoadMods(mapping, config, mods);
        return (applier, exchange, assetLoaderRegistry);
    }

    public AssetConverter(AssetMapping mapping, IFileSystem disk, IJsonUtil jsonUtil, string[] fromMods, string toMod)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        var baseDir = ConfigUtil.FindBasePath(disk);
        (_from, _fromExchange, _fromLoaderRegistry) = BuildModApplier(baseDir, fromMods, disk, jsonUtil, mapping);
        (_to, _toExchange, _toLoaderRegistry) = BuildModApplier(baseDir, new[] { toMod }, disk, jsonUtil, mapping);
    }

    public void Convert(string[] ids, ISet<AssetType> assetTypes, Regex filePattern)
    {
        // Give the "from" universe's asset manager "to" the to exchange so we can import the assets.
        _toExchange.Attach(new AssetManager(_from));
        _fromExchange.Attach(new AssetManager(_from)); // From also needs an asset manager for the inventory post-processor etc

        var parsedIds = ids?.Select(AssetId.Parse).ToHashSet();
        var cache = new Dictionary<(AssetId, string), object>();

        (object, AssetInfo) LoaderFunc(AssetId id, string language)
        {
            StringId? stringId = TextId.ValidTypes.Contains(id.Type)
                ? (TextId)id
                : null;

            if (stringId != null)
            {
                if (stringId.Value.Id == id)
                    stringId = null;
                else
                    id = stringId.Value.Id;
            }

            var info = _from.GetAssetInfo(id, language);
            var asset = cache.TryGetValue((id, language), out var cached)
                ? cached
                : cache[(id, language)] = _from.LoadAsset(id, language);

            if (stringId.HasValue && asset is IStringCollection collection)
                asset = collection.GetString(stringId.Value, language);

            return (asset, info);
        }

        _to.SaveAssets(LoaderFunc, () => cache.Clear(), parsedIds, assetTypes, filePattern);
    }

    public void Dispose()
    {
        _fromExchange.Dispose();
        _fromLoaderRegistry.Dispose();
        _toExchange.Dispose();
        _toLoaderRegistry.Dispose();
    }
}