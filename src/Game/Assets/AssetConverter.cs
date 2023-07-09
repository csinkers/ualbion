using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Game.Magic;

namespace UAlbion.Game.Assets;

public sealed class AssetConverter : IDisposable
{
    readonly ModApplier _from;
    readonly ModApplier _to;
    readonly EventExchange _fromExchange;
    readonly EventExchange _toExchange;
    readonly AssetLoaderRegistry _fromLoaderRegistry;
    readonly AssetLoaderRegistry _toLoaderRegistry;

    static (ModApplier, EventExchange, AssetLoaderRegistry) BuildModApplier(string baseDir, string appName, string[] mods, IFileSystem disk, IJsonUtil jsonUtil, AssetMapping mapping)
    {
        var pathResolver = new PathResolver(baseDir, appName);
        var applier = new ModApplier();
        var exchange = new EventExchange(new LogExchange()) { Name = $"EventExchange for {string.Join(", ", mods)}"};
        var assetLoaderRegistry = new AssetLoaderRegistry();
        var settings = new VarSet("ConfigOnly");

        exchange
            .Register<IPathResolver>(pathResolver)
            .Register<IVarSet>(settings)
            .Register(disk)
            .Register(jsonUtil)
            .Attach(new StdioConsoleLogger())
            .Attach(assetLoaderRegistry)
            .Attach(new ContainerRegistry())
            .Attach(new PostProcessorRegistry())
            .Attach(new VarRegistry())
            .Attach(new AssetLocator())
            .Attach(new SpellManager())
            .Attach(applier)
            ;

        applier.LoadMods(mapping, pathResolver, mods);

        var config = (IVarSet)applier.LoadAsset(AssetId.From(Base.Special.GameConfig));
        if (config != null) // This might fail for Unpacked etc prior to an export being done
            settings.Apply(config);

        return (applier, exchange, assetLoaderRegistry);
    }

    public AssetConverter(string appName, AssetMapping mapping, IFileSystem disk, IJsonUtil jsonUtil, string[] fromMods, string toMod)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        var baseDir = ConfigUtil.FindBasePath(disk);
        (_from, _fromExchange, _fromLoaderRegistry) = BuildModApplier(baseDir, appName, fromMods, disk, jsonUtil, mapping);
        (_to, _toExchange, _toLoaderRegistry) = BuildModApplier(baseDir, appName, new[] { toMod }, disk, jsonUtil, mapping);

        // Give the "from" universe's asset manager "to" the to exchange so we can import the assets.
        _toExchange.Attach(new AssetManager(_from));
        _fromExchange.Attach(new AssetManager(_from)); // From also needs an asset manager for the inventory post-processor etc
    }

    public void Convert(ISet<AssetId> ids, ISet<AssetType> assetTypes, Regex filePattern, Func<AssetLoadResult, AssetLoadResult> converter = null, string[] validLanguages = null)
    {
        var cache = new Dictionary<(AssetId, string), AssetLoadResult>();

        AssetLoadResult LoaderFunc(AssetId assetId, string language)
        {
            AssetLoadResult result;

            if (cache.TryGetValue((assetId, language), out var cached))
            {
                result = cached;
            }
            else
            {
                result = _from.LoadAssetAndNode(assetId, language);
                cache[(assetId, language)] = result;
            }

            if (converter != null)
                result = converter(result);

            return result;
        }

        _to.SaveAssets(LoaderFunc, () => cache.Clear(), ids, assetTypes, validLanguages, filePattern);
    }

    public string[] DiscoverLanguages(TextId? languageSearchId = null)
    {
        languageSearchId ??= Base.SystemText.MainMenu_MainMenu;
        var fromAssets = _fromExchange.Resolve<IAssetManager>();

        // Return null if no languages are found, this will cause all languages defined to be searched for
        // on language-specific assets. In practice this scenario will happen in minimal round-trip tests
        // which don't export system text.
        List<string> validLanguages = null;
        foreach (var language in _from.Languages.Keys)
        {
            if (!fromAssets.IsStringDefined(languageSearchId.Value, language)) continue;
            validLanguages ??= new List<string>();
            validLanguages.Add(language);
        }

        return validLanguages?.ToArray();
    }

    public void Dispose()
    {
        _fromExchange.Dispose();
        _fromLoaderRegistry.Dispose();
        _toExchange.Dispose();
        _toLoaderRegistry.Dispose();
    }
}
