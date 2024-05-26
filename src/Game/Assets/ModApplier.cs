using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;

#if DEBUG
using System.Text;
using static System.FormattableString;
#endif

namespace UAlbion.Game.Assets;

public class ModApplier : GameComponent, IModApplier
{
    readonly AssetCache _assetCache = new();
    readonly ModLoader _mods = new();
    IAssetLocator _assetLocator;

    public ModApplier()
    {
        AttachChild(_mods);
        AttachChild(_assetCache);
        On<SetLanguageEvent>(_ =>
        {
            // TODO: Different languages could have different sub-id ranges in their
            // container files, so we should really be invalidating / rebuilding the whole asset config too.
            Raise(new ReloadAssetsEvent());
            Raise(new LanguageChangedEvent());
        });
    }

    public IReadOnlyDictionary<string, LanguageConfig> Languages => _mods.Languages;

    protected override void Subscribed()
    {
        _assetLocator ??= Resolve<IAssetLocator>();
        Exchange.Register<IModApplier>(this);
    }

    public IEnumerable<string> ShaderPaths =>
        _mods.ModsInReverseDependencyOrder
            .Where(x => !string.IsNullOrEmpty(x.ModConfig.ShaderPath))
            .Select(mod => mod.ModContext.Disk.ToAbsolutePath(mod.ShaderPath));


    public void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IReadOnlyList<string> mods) 
        => _mods.LoadMods(mapping, pathResolver, mods);

    public AssetNode GetAssetInfo(AssetId key, string language = null)
    {
        foreach (var mod in _mods.ModsInReverseDependencyOrder)
        {
            var assetNodes = mod.AssetConfig.GetAssetInfo(key);
            foreach(var node in assetNodes)
            {
                if (language == null)
                    return node;

                var assetLanguage = node.GetProperty(AssetProps.Language);
                if (assetLanguage == null)
                    return node;

                if (string.Equals(assetLanguage, language, StringComparison.OrdinalIgnoreCase))
                    return node;
            }
        }

        return null;
    }

    public object LoadAsset(AssetId id, string language = null)
    {
        try
        {
            var result = LoadAssetInternal(id, language);
            return result?.Asset is Exception ? null : result.Asset;
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not load asset {id}: {e}");
            return null;
        }
    }

    public AssetLoadResult LoadAssetAndNode(AssetId assetId, string language = null)
    {
        try
        {
            var result = LoadAssetInternal(assetId, language);
            return result.Asset is Exception ? null : result;
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not load asset {assetId}: {e}");
            return null;
        }
    }

    public string LoadAssetAnnotated(AssetId id, string language = null)
    {
        using var ms = new MemoryStream();
        using var annotationWriter = new StreamWriter(ms);
        LoadAssetInternal(id, language, annotationWriter);
        annotationWriter.Flush();

        ms.Position = 0;
        using var reader = new StreamReader(ms, null, true, -1, true);
        return reader.ReadToEnd();
    }

    public object LoadAssetCached(AssetId id, string language = null)
    {
        var result = GetCached(id, language);
        if (result != null)
            return result.Asset;

        try
        {
            result = LoadAssetInternal(id, language);
            _assetCache.Add(id, language, result?.Asset ?? new AssetNotFoundException($"Could not load asset for {id}"), result?.Node);
            return result?.Asset is Exception ? null : result?.Asset;
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not load asset {id}: {e}");
            _assetCache.Add(id, language, e, null);
            return null;
        }
    }

    AssetLoadResult GetCached(AssetId assetId, string language)
    {
        var loadResult = _assetCache.Get(assetId, language);
        return loadResult?.Asset is null or Exception 
            ? null // If it failed to load once then stop trying (at least until an asset:reload / cycle)
            : loadResult;
    }

    AssetLoadResult LoadAssetInternal(AssetId id, string language, TextWriter annotationWriter = null)
    {
        if (id.IsNone)
            return null;

        if (id.Type == AssetType.MetaFont)
            return LoadMetaFont(id);

        object asset = null;
        AssetNode loadedNode = null;
        Stack<IPatch> patches = null; // Create the stack lazily, as most assets won't have any patches.
        language ??= ReadVar(V.User.Gameplay.Language);

#if DEBUG
        var filesSearched = new List<string>();
        bool isOptional = false;
        var loaderWarnings = new StringBuilder();

        foreach (var mod in _mods.ModsInReverseDependencyOrder)
        {
            filesSearched.Clear();
            bool anyFiles = false;
            var assetLocations = mod.AssetConfig.GetAssetInfo(id).ToArray();
#else
        foreach (var mod in _mods.ModsInReverseDependencyOrder)
        {
            var assetLocations = mod.AssetConfig.GetAssetInfo(id);
#endif

            foreach (AssetNode node in assetLocations)
            {
#if DEBUG
                anyFiles = true;
#endif
                var assetLang = node.GetProperty(AssetProps.Language);
                if (assetLang != null && !string.Equals(assetLang, language, StringComparison.OrdinalIgnoreCase))
                    continue;

                var context = new AssetLoadContext(id, node, mod.ModContext, language);

#if DEBUG
                var modAsset = _assetLocator.LoadAsset(context, annotationWriter, filesSearched);
#else
                var modAsset = _assetLocator.LoadAsset(context, annotationWriter, null);
#endif

                if (modAsset is IPatch patch)
                {
                    patches ??= new Stack<IPatch>();
                    patches.Push(patch);
                }
                else if (modAsset is Exception)
                {
                    asset ??= modAsset;
                }
                else if (modAsset != null)
                {
                    var postProcessorType = node.PostProcessor;
                    if (postProcessorType != null)
                    {
                        var registry = Resolve<IAssetPostProcessorRegistry>();
                        var postProcessor = registry.GetPostProcessor(postProcessorType);
                        if (postProcessor != null)
                            modAsset = postProcessor.Process(modAsset, context);
                    }

                    asset = modAsset;
                    loadedNode = node;
                    goto assetFound;
                }

#if DEBUG
                isOptional |= node.GetProperty(AssetProps.Optional);
#endif
            }

#if DEBUG
            if (!isOptional && asset == null && anyFiles && filesSearched is { Count: > 0 } && AssetMapping.Global.IsMapped(id))
            {
                loaderWarnings.AppendLine(Invariant($"Tried to load asset {id} from mod {mod.Name}"));
                loaderWarnings.AppendLine("  Files searched:");
                foreach (var node in assetLocations)
                {
                    var hash = string.IsNullOrEmpty(node.Sha256Hash) ? "" : $" (expected hash {node.Sha256Hash})";
                    loaderWarnings.AppendLine(Invariant($"    {node.Filename}{hash}"));
                }

                loaderWarnings.AppendLine("  Files found:");
                foreach (var path in filesSearched.Distinct())
                {
                    loaderWarnings.Append("    ");
                    loaderWarnings.AppendLine(path);
                }
            }
#endif
        }

        assetFound:
        while (patches is { Count: > 0 })
            asset = patches.Pop().Apply(asset);

#if DEBUG
        if (asset == null && loaderWarnings.Length > 0)
            Warn(loaderWarnings.ToString());
#endif

        return new AssetLoadResult(id, asset, loadedNode);
    }

    AssetLoadResult LoadMetaFont(AssetId id)
    {
        var metaId = (MetaFontId)id;
        var font = Assets.LoadFontDefinition(metaId.FontId);
        var metaFont = font.Build(metaId.FontId, metaId.InkId, Assets);
        return new AssetLoadResult(id, metaFont, null);
    }

    public SavedGame LoadSavedGame(string path)
    {
        var disk = Resolve<IFileSystem>();
        if (!disk.FileExists(path))
        {
            Error($"Could not find save game file \"{path}\"");
            return null;
        }

        using var stream = disk.OpenRead(path);
        using var br = new BinaryReader(stream);
        using var s = new AlbionReader(br, stream.Length);
        var spellManager = Resolve<ISpellManager>();
        return SavedGame.Serdes(null, AssetMapping.Global, s, spellManager);
    }

    public void SaveAssets(AssetConversionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.LoaderFunc == null) throw new ArgumentException(nameof(options.LoaderFunc));
        if (options.FlushCacheFunc == null) throw new ArgumentException(nameof(options.FlushCacheFunc));

        var pathResolver = Resolve<IPathResolver>();
        var containerRegistry = Resolve<IContainerRegistry>();
        var writeDisk = Resolve<IFileSystem>();
        var target = _mods.ModsInReverseDependencyOrder[0];
        var filesWritten = new HashSet<string>();

        foreach (var rangeInfo in target.AssetConfig.Ranges.AllRanges)
        {
            if (options.AssetTypes != null && !options.AssetTypes.Contains(rangeInfo.Range.From.Type))
                continue;

            var assets = new Dictionary<string, List<(AssetLoadContext Context, byte[] Bytes)>>();
            foreach (var assetId in rangeInfo.Range)
            {
                if (!AssetMapping.Global.IsMapped(assetId)) continue;
                if (options.Ids != null && !options.Ids.Contains(assetId)) continue;
                options.FlushCacheFunc();

                var nodes = target.AssetConfig.GetAssetInfo(assetId);
                foreach (var node in nodes)
                {
                    var saveContext = new AssetLoadContext(assetId, node, target.ModContext);
                    ConvertAsset(options, saveContext, filesWritten, assets);
                }
            }

            foreach (var kvp in assets.OrderBy(x => x.Key))
            {
                var first = kvp.Value[0].Context;
                var path = pathResolver.ResolvePath(first.Filename);
                var writeContainer = containerRegistry.GetContainer(path, first.Node.Container, writeDisk);
                writeContainer.Write(path, kvp.Value, target.ModContext);
            }
        }

        Info("Finished saving assets");
    }

    void ConvertAsset(
        AssetConversionOptions options,
        AssetLoadContext saveContext,
        HashSet<string> filesWritten,
        Dictionary<string, List<(AssetLoadContext Context, byte[] Bytes)>> assets)
    {
        var filename = saveContext.Filename;
        if (options.FilePattern != null && !options.FilePattern.IsMatch(filename)) return;
        if (saveContext.GetProperty(AssetProps.IsReadOnly)) return;

        var language = saveContext.GetProperty(AssetProps.Language);
        if (language != null && options.Languages != null && !options.Languages.Contains(language))
            return;

        AssetLoadResult result;
        if (saveContext.GetProperty(AssetProps.UseDummyRead))
        {
            result = new AssetLoadResult(saveContext.AssetId, new object(), saveContext.Node);
        }
        else
        {
            result = options.LoaderFunc(saveContext.AssetId, language);
            if (result?.Asset == null)
            {
                if (AssetMapping.Global.IsMapped(saveContext.AssetId) && !saveContext.GetProperty(AssetProps.Optional))
                    Error($"Could not load {saveContext.AssetId} {language}");

                return;
            }
        }

        // Hacky copying of palette property to make png export/import work
        var sourcePalette = result.Node?.PaletteId ?? AssetId.None;
        if (!sourcePalette.IsNone)
        {
            saveContext = saveContext with { Node = new AssetNode(saveContext.Node) }; // Clone the node to prevent affecting any other assets that share the node
            saveContext.SetProperty(AssetProps.Palette, sourcePalette);
        }

        if (filesWritten.Add(filename))
            Info($"Saving {filename}...");

        SaveAsset(saveContext, result.Asset, assets);
    }

    void SaveAsset(AssetLoadContext targetInfo, object asset, Dictionary<string, List<(AssetLoadContext, byte[])>> assets)
    {
        var loaderRegistry = Resolve<IAssetLoaderRegistry>();
        var loader = loaderRegistry.GetLoader(targetInfo.Node.Loader);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        using var s = new AlbionWriter(bw);

        try
        {
            loader.Serdes(asset, s, targetInfo);

            ms.Position = 0;

            assets ??= new Dictionary<string, List<(AssetLoadContext, byte[])>>();
            if (!assets.TryGetValue(targetInfo.Filename, out var list))
            {
                list = new List<(AssetLoadContext, byte[])>();
                assets[targetInfo.Filename] = list;
            }

            list.Add((targetInfo, ms.ToArray()));
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not save asset {targetInfo.AssetId}: {e}");
        }
    }
}
