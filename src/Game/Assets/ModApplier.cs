using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Containers;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;
using static System.FormattableString;

namespace UAlbion.Game.Assets;

public class ModApplier : Component, IModApplier
{
    readonly Dictionary<string, ModInfo> _mods = new();
    readonly List<ModInfo> _modsInReverseDependencyOrder = new();
    readonly AssetCache _assetCache = new();
    readonly Dictionary<string, LanguageConfig> _languages = new();

    IAssetLocator _assetLocator;

    public ModApplier()
    {
        Languages = new ReadOnlyDictionary<string, LanguageConfig>(_languages);
        AttachChild(_assetCache);
        On<SetLanguageEvent>(_ =>
        {
            // TODO: Different languages could have different sub-id ranges in their
            // container files, so we should really be invalidating / rebuilding the whole asset config too.
            Raise(new ReloadAssetsEvent());
            Raise(new LanguageChangedEvent());
        });
    }

    public IReadOnlyDictionary<string, LanguageConfig> Languages { get; }

    protected override void Subscribed()
    {
        _assetLocator ??= Resolve<IAssetLocator>();
        Exchange.Register<IModApplier>(this);
    }

    public void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IList<string> mods)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (pathResolver == null) throw new ArgumentNullException(nameof(pathResolver));
        if (mods == null) throw new ArgumentNullException(nameof(mods));

        pathResolver.RegisterPath("ALBION", pathResolver.ResolvePathAbsolute(GetVar(UserVars.Path.Albion)));
        pathResolver.RegisterPath("SAVES", pathResolver.ResolvePathAbsolute(GetVar(UserVars.Path.Saves)));

        _mods.Clear();
        _modsInReverseDependencyOrder.Clear();
        mapping.Clear();

        foreach (var mod in mods.Reverse())
            LoadMod(pathResolver.ResolvePathAbsolute("$(MODS)"), mod.Trim(), mapping);

        _modsInReverseDependencyOrder.Reverse();
        Raise(ModsLoadedEvent.Instance);
    }

    public IEnumerable<string> ShaderPaths =>
        _modsInReverseDependencyOrder
            .Where(x => !string.IsNullOrEmpty(x.ModConfig.ShaderPath))
            .Select(mod => mod.SerdesContext.Disk.ToAbsolutePath(mod.ShaderPath));

    void LoadMod(string dataDir, string modName, AssetMapping mapping)
    {
        if (string.IsNullOrEmpty(modName))
            return;

        if (_mods.ContainsKey(modName))
            return;

        if (modName.Any(c => c is '\\' or '/' || c == Path.DirectorySeparatorChar))
        {
            Error($"Mod {modName} is not a simple directory name");
            return;
        }

        var disk = Resolve<IFileSystem>();
        var pathResolver = Resolve<IPathResolver>();
        var jsonUtil = Resolve<IJsonUtil>();

        string path = Path.Combine(dataDir, modName);
        if (!disk.DirectoryExists(path))
        {
            Error($"Mod directory {modName} does not exist in {dataDir}");
            return;
        }

        var modDisk = disk.Duplicate(path);
        var modConfigPath = Path.Combine(path, ModConfig.ModConfigFilename);
        if (!modDisk.FileExists(modConfigPath))
        {
            Error($"Mod {modName} does not contain a {ModConfig.ModConfigFilename} file");
            return;
        }

        var modConfig = ModConfig.Load(modConfigPath, modDisk, jsonUtil);
        if (modConfig.SymLinks != null)
        {
            foreach (var kvp in modConfig.SymLinks)
                modDisk = new RedirectionFileSystemDecorator(modDisk, kvp.Key, pathResolver.ResolvePath(kvp.Value));
        }

        var assetConfigPath = Path.Combine(path, modConfig.AssetConfig);
        if (!modDisk.FileExists(assetConfigPath))
        {
            Error($"Mod {modName} does not contain an {modConfig.AssetConfig} file");
            return;
        }

        var modMapping = new AssetMapping();

        // Load dependencies
        foreach (var dependency in modConfig.Dependencies)
        {
            LoadMod(dataDir, dependency, mapping);
            if (!_mods.TryGetValue(dependency, out var dependencyInfo))
            {
                Error($"Dependency {dependency} of mod {modName} could not be loaded, skipping load of {modName}");
                return;
            }

            modMapping.MergeFrom(dependencyInfo.SerdesContext.Mapping);
        }

        var parentConfig = modConfig.InheritAssetConfigFrom != null && _mods.TryGetValue(modConfig.InheritAssetConfigFrom, out var parent) ? parent.AssetConfig : null;
        var assetConfig = AssetConfig.Load(assetConfigPath, parentConfig, modMapping, modDisk, jsonUtil);
        assetConfig.Validate(assetConfigPath);
        var modInfo = new ModInfo(modName, assetConfig, modConfig, modMapping, jsonUtil, modDisk);

        foreach (var kvp in assetConfig.Languages)
            _languages[kvp.Key] = kvp.Value;

        MergeTypesToMapping(modMapping, assetConfig, assetConfigPath);
        mapping.MergeFrom(modMapping);

        assetConfig.PopulateAssetIds(
            jsonUtil,
            x => _assetLocator.GetSubItemRangesForFile(x, modInfo.SerdesContext),
            x => modInfo.SerdesContext.Disk.ReadAllBytes(pathResolver.ResolvePath(x)));
        _mods.Add(modName, modInfo);
        _modsInReverseDependencyOrder.Add(modInfo);
    }

    static void MergeTypesToMapping(AssetMapping mapping, AssetConfig config, string assetConfigPath)
    {
        foreach (var assetType in config.IdTypes.Values)
        {
            var enumType = Type.GetType(assetType.EnumType);
            if (enumType == null)
                throw new InvalidOperationException(
                    $"Could not load enum type \"{assetType.EnumType}\" defined in \"{assetConfigPath}\"");

            mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
        }

        config.RegisterStringRedirects(mapping);
    }

    public AssetInfo GetAssetInfo(AssetId id, string language = null) =>
        _modsInReverseDependencyOrder
            .SelectMany(x => x.AssetConfig.GetAssetInfo(id))
            .FirstOrDefault(x =>
            {
                var assetLanguage = x.Get<string>(AssetProperty.Language, null);
                return language == null || 
                       assetLanguage == null || 
                       string.Equals(assetLanguage, language, StringComparison.OrdinalIgnoreCase);
            });

    public object LoadAsset(AssetId id) => LoadAsset(id, null);
    public object LoadAsset(AssetId id, string language)
    {
        try
        {
            var asset = LoadAssetInternal(id, language);
            return asset is Exception ? null : asset;
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not load asset {id}: {e}");
            return null;
        }
    }

    public string LoadAssetAnnotated(AssetId id, string language)
    {
        using var ms = new MemoryStream();
        using var annotationWriter = new StreamWriter(ms);
        LoadAssetInternal(id, language, annotationWriter);
        annotationWriter.Flush();

        ms.Position = 0;
        using var reader = new StreamReader(ms, null, true, -1, true);
        return reader.ReadToEnd();
    }

    public object LoadAssetCached(AssetId id)
    {
        object asset = _assetCache.Get(id);
        if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
            return null;

        if (asset != null)
            return asset;

        try
        {
            asset = LoadAssetInternal(id);
            _assetCache.Add(asset ?? new AssetNotFoundException($"Could not load asset for {id}"), id);
            return asset is Exception ? null : asset;
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not load asset {id}: {e}");
            _assetCache.Add(e, id);
            return null;
        }
    }

    object LoadAssetInternal(AssetId id, string language = null, TextWriter annotationWriter = null)
    {
        if (id.IsNone)
            return null;

        if (id.Type == AssetType.MetaFont)
        {
            var assets = Resolve<IAssetManager>();
            var metaId = (MetaFontId)id;
            var font = assets.LoadFontDefinition(metaId.FontId);
            return font.Build(metaId.FontId, metaId.InkId, assets);
        }

        object asset = null;
        Stack<IPatch> patches = null; // Create the stack lazily, as most assets won't have any patches.

        List<string> filesSearched =
#if DEBUG
            new List<string>();
        var loaderWarnings = new StringBuilder();
#else
            null;
#endif

        foreach (var mod in _modsInReverseDependencyOrder)
        {
#if DEBUG
            filesSearched.Clear();
#endif

            var assetLocations = mod.AssetConfig.GetAssetInfo(id);
            foreach (var info in assetLocations)
            {
                var assetLang = info.Get<string>(AssetProperty.Language, null);
                if (assetLang != null)
                {
                    language ??= GetVar(UserVars.Gameplay.Language);
                    if (!string.Equals(assetLang, language, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                var modAsset = _assetLocator.LoadAsset(info, mod.SerdesContext, annotationWriter, filesSearched);

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
                    if (!string.IsNullOrEmpty(info.File.Post))
                    {
                        var registry = Resolve<IAssetPostProcessorRegistry>();
                        var postProcessor = registry.GetPostProcessor(info.File.Post);
                        if (postProcessor != null)
                            modAsset = postProcessor.Process(modAsset, info);
                    }

                    asset = modAsset;
                    goto assetFound;
                }
            }

#if DEBUG
            if (asset == null && assetLocations.Length > 0 && filesSearched is { Count: > 0 })
            {
                loaderWarnings.AppendLine(Invariant($"Tried to load asset {id} from mod {mod.Name}"));
                loaderWarnings.AppendLine("  Files searched:");
                foreach (var info in assetLocations)
                {
                    var hash = string.IsNullOrEmpty(info.File.Sha256Hash) ? "" : $" (expected hash {info.File.Sha256Hash})";
                    loaderWarnings.AppendLine(Invariant($"    {info.File.Filename}{hash}"));
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

        return asset;
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

    public void SaveAssets(
        IModApplier.AssetLoaderDelegate loaderFunc,
        Action flushCacheFunc,
        ISet<AssetId> ids,
        ISet<AssetType> assetTypes,
        Regex filePattern)
    {
        if (loaderFunc == null) throw new ArgumentNullException(nameof(loaderFunc));
        if (flushCacheFunc == null) throw new ArgumentNullException(nameof(flushCacheFunc));

        var pathResolver = Resolve<IPathResolver>();
        var loaderRegistry = Resolve<IAssetLoaderRegistry>();
        var containerRegistry = Resolve<IContainerRegistry>();
        var writeDisk = Resolve<IFileSystem>();
        var jsonUtil = Resolve<IJsonUtil>();
        var target = _modsInReverseDependencyOrder.First();

        // Add any missing ids
        Info("Populating destination asset info...");
        target.AssetConfig.PopulateAssetIds(
            jsonUtil,
            file =>
            {
                // Don't need to resolve the filename as we're not actually using the container - we just want to find the type.
                var container = containerRegistry.GetContainer(file.Filename, file.Container, writeDisk);
                var firstAsset = file.Map[file.Map.Keys.Min()];
                if (assetTypes != null && !assetTypes.Contains(firstAsset.AssetId.Type))
                    return new List<(int, int)> { (firstAsset.Index, 1) };

                if (filePattern != null && !filePattern.IsMatch(file.Filename))
                    return new List<(int, int)> { (firstAsset.Index, 1) };

                var assets = target.SerdesContext.Mapping.EnumerateAssetsOfType(firstAsset.AssetId.Type).ToList();
                var idsInRange =
                    assets
                        .Where(x => x.Id >= firstAsset.AssetId.Id)
                        .OrderBy(x => x.Id)
                        .Select(x => x.Id - firstAsset.AssetId.Id + firstAsset.Index);

                if (container is XldContainer)
                    idsInRange = idsInRange.Where(x => x < 100);

                int? maxSubId = file.Max;
                if (maxSubId.HasValue)
                    idsInRange = idsInRange.Where(x => x <= maxSubId.Value);

                return FormatUtil.SortedIntsToRanges(idsInRange);
            }, x => writeDisk.ReadAllBytes(pathResolver.ResolvePath(x)));

        foreach (var file in target.AssetConfig.Files.Values)
        {
            if (filePattern != null && !filePattern.IsMatch(file.Filename))
                continue;

            if (file.Get(AssetProperty.IsReadOnly, false))
                continue;

            bool notify = true;
            flushCacheFunc();
            var path = pathResolver.ResolvePath(file.Filename);
            var loader = loaderRegistry.GetLoader(file.Loader);
            var writeContainer = containerRegistry.GetContainer(path, file.Container, writeDisk);
            var assets = new List<(AssetInfo, byte[])>();
            foreach (var assetInfo in file.Map.Values)
            {
                if (ids != null && !ids.Contains(assetInfo.AssetId)) continue;
                if (assetTypes != null && !assetTypes.Contains(assetInfo.AssetId.Type)) continue;

                var language = assetInfo.Get<string>(AssetProperty.Language, null);
                var (asset, sourceInfo) = loaderFunc(assetInfo.AssetId, language);
                if (asset == null)
                {
                    // Automaps should only load for 3D maps, no need for 'not found' errors, also unmapped ids might be getting requested
                    // due to populating the full range of an XLD, as the ids aren't actually in use it's fine to ignore their absence.
                    var id = assetInfo.AssetId;
                    if (id.Type != AssetType.Automap && AssetMapping.Global.IsMapped(id))
                        Error($"Could not load {assetInfo.AssetId}");
                    continue;
                }

                if (notify)
                {
                    Info($"Saving {file.Filename}...");
                    notify = false;
                }

                var paletteId = sourceInfo.Get(AssetProperty.PaletteId, 0);
                if (paletteId != 0)
                    assetInfo.Set(AssetProperty.PaletteId, paletteId);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                using var s = new AlbionWriter(bw);
                loader.Serdes(asset, assetInfo, s, target.SerdesContext);

                ms.Position = 0;
                assets.Add((assetInfo, ms.ToArray()));
            }

            if (assets.Count > 0)
                writeContainer.Write(path, assets, target.SerdesContext);
        }

        Info("Finished saving assets");
    }
}