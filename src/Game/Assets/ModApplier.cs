using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;
using static System.FormattableString;

namespace UAlbion.Game.Assets;

public class ModApplier : Component, IModApplier
{
    readonly AssetCache _assetCache = new();
    readonly Dictionary<string, ModInfo> _mods = new();
    readonly List<ModInfo> _modsInReverseDependencyOrder = new();
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

    public void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IReadOnlyList<string> mods)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (pathResolver == null) throw new ArgumentNullException(nameof(pathResolver));
        if (mods == null) throw new ArgumentNullException(nameof(mods));

        pathResolver.RegisterPath("ALBION", pathResolver.ResolvePathAbsolute(Var(UserVars.Path.Albion)));
        pathResolver.RegisterPath("SAVES", pathResolver.ResolvePathAbsolute(Var(UserVars.Path.Saves)));

        _mods.Clear();
        _modsInReverseDependencyOrder.Clear();
        TryResolve<IVarRegistry>()?.Clear();
        mapping.Clear();

        foreach (var mod in mods.Reverse())
            LoadMod(pathResolver.ResolvePathAbsolute("$(MODS)"), mod.Trim(), mapping);

        _modsInReverseDependencyOrder.Reverse();

        Raise(ModsLoadedEvent.Instance);
    }

    public IEnumerable<string> ShaderPaths =>
        _modsInReverseDependencyOrder
            .Where(x => !string.IsNullOrEmpty(x.ModConfig.ShaderPath))
            .Select(mod => mod.ModContext.Disk.ToAbsolutePath(mod.ShaderPath));

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
            foreach (var kvp in modConfig.SymLinks)
                modDisk = new RedirectionFileSystemDecorator(modDisk, kvp.Key, pathResolver.ResolvePath(kvp.Value));

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

            modMapping.MergeFrom(dependencyInfo.ModContext.Mapping);
        }

        var parentTypeConfig  = modConfig.InheritTypeConfigFrom  != null && _mods.TryGetValue(modConfig.InheritTypeConfigFrom, out var parentMod) ? parentMod.TypeConfig  : null;
        var parentAssetConfig = modConfig.InheritAssetConfigFrom != null && _mods.TryGetValue(modConfig.InheritAssetConfigFrom, out parentMod)    ? parentMod.AssetConfig : null;

        TypeConfig typeConfig = parentTypeConfig;
        if (modConfig.TypeConfig != null)
        {
            var typeConfigPath = Path.Combine(path, modConfig.TypeConfig);
            if (!modDisk.FileExists(typeConfigPath))
            {
                Error($"Mod {modName} specifies {modConfig.TypeConfig} as a type config file, but it could not be found.");
                return;
            }
            var tcl = new TypeConfigLoader(jsonUtil);
            typeConfig = tcl.Load(typeConfigPath, modName, parentTypeConfig, modMapping, modDisk);

            if (typeConfig.VarTypes != null)
                LoadVarTypes(modName, typeConfig.VarTypes);

            MergeTypesToMapping(modMapping, typeConfig, typeConfigPath);
        }
        typeConfig ??= new TypeConfig(modName, modMapping);

        AssetConfig assetConfig = parentAssetConfig;
        if (modConfig.AssetConfig != null)
        {
            var assetConfigPath = Path.Combine(path, modConfig.AssetConfig);
            if (!modDisk.FileExists(assetConfigPath))
            {
                Error($"Mod {modName} specifies {modConfig.AssetConfig} as an asset config file, but it could not be found.");
                return;
            }

            var acl = new AssetConfigLoader(modDisk, jsonUtil, pathResolver, typeConfig);
            assetConfig = acl.Load(assetConfigPath, modName, parentAssetConfig);
        }

        assetConfig ??= new AssetConfig(modName, new RangeLookup());

        var modInfo = new ModInfo(modName, typeConfig, assetConfig, modConfig, modMapping, jsonUtil, modDisk);

        if (typeConfig.Languages != null)
            foreach (var kvp in typeConfig.Languages)
                _languages[kvp.Key] = kvp.Value;

        mapping.MergeFrom(modMapping);

        _mods.Add(modName, modInfo);
        _modsInReverseDependencyOrder.Add(modInfo);
    }

    void LoadVarTypes(string modName, IEnumerable<string> varTypes)
    {
        var varRegistry = Resolve<IVarRegistry>();
        foreach (var typeName in varTypes)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                throw new InvalidOperationException($"Could not load type \"{typeName}\" as Var container from mod {modName}");

            varRegistry.Register(type);
        }
    }

    static void MergeTypesToMapping(AssetMapping mapping, TypeConfig config, string typeConfigPath)
    {
        foreach (var assetType in config.IdTypes.Values)
        {
            var enumType = Type.GetType(assetType.EnumType);
            if (enumType == null)
                throw new InvalidOperationException($"Could not load enum type \"{assetType.EnumType}\" defined in \"{typeConfigPath}\"");

            mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
        }
    }

    public AssetNode GetAssetInfo(AssetId key, string language = null)
    {
        foreach (var mod in _modsInReverseDependencyOrder)
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

    public object LoadAssetCached(AssetId assetId)
    {
        object asset = _assetCache.Get(assetId);
        if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
            return null;

        if (asset != null)
            return asset;

        try
        {
            asset = LoadAssetInternal(assetId);
            _assetCache.Add(asset ?? new AssetNotFoundException($"Could not load asset for {assetId}"), assetId);
            return asset is Exception ? null : asset;
        }
        catch (Exception e)
        {
            if (CoreUtil.IsCriticalException(e))
                throw;

            Error($"Could not load asset {assetId}: {e}");
            _assetCache.Add(e, assetId);
            return null;
        }
    }

    object LoadAssetInternal(AssetId id, string language = null, TextWriter annotationWriter = null)
    {
        if (id.IsNone)
            return null;

        if (id.Type == AssetType.MetaFont)
            return LoadMetaFont(id);

        object asset = null;
        Stack<IPatch> patches = null; // Create the stack lazily, as most assets won't have any patches.
        language ??= Var(UserVars.Gameplay.Language);

        List<string> filesSearched =
#if DEBUG
            new List<string>();

        bool isOptional = false;
        var loaderWarnings = new StringBuilder();
#else
            null;
#endif

        foreach (var mod in _modsInReverseDependencyOrder)
        {
#if DEBUG
            filesSearched.Clear();
#endif

            var assetLocations = mod.AssetConfig.GetAssetInfo(id)
#if DEBUG
                    .ToArray()
#endif
                ;

            bool anyFiles = false;
            foreach (AssetNode node in assetLocations)
            {
                anyFiles = true;
                var assetLang = node.GetProperty(AssetProps.Language);
                if (assetLang != null && !string.Equals(assetLang, language, StringComparison.OrdinalIgnoreCase))
                    continue;

                var context = new AssetLoadContext(id, node, mod.ModContext, language);
                var modAsset = _assetLocator.LoadAsset(context, annotationWriter, filesSearched);

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
                    goto assetFound;
                }

#if DEBUG
                isOptional |= node.GetProperty(AssetProps.Optional);
#endif
            }

#if DEBUG
            if (!isOptional && asset == null && anyFiles && filesSearched is { Count: > 0 })
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

        return asset;
    }

    object LoadMetaFont(AssetId id)
    {
        var assets = Resolve<IAssetManager>();
        var metaId = (MetaFontId)id;
        var font = assets.LoadFontDefinition(metaId.FontId);
        return font.Build(metaId.FontId, metaId.InkId, assets);
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
        IModApplier.AssetLoader loaderFunc,
        Action flushCacheFunc,
        ISet<AssetId> ids,
        ISet<AssetType> assetTypes,
        string[] languages,
        Regex filePattern)
    {
        if (loaderFunc == null) throw new ArgumentNullException(nameof(loaderFunc));
        if (flushCacheFunc == null) throw new ArgumentNullException(nameof(flushCacheFunc));

        var pathResolver = Resolve<IPathResolver>();
        var containerRegistry = Resolve<IContainerRegistry>();
        var writeDisk = Resolve<IFileSystem>();
        var target = _modsInReverseDependencyOrder.First();
        var filesWritten = new HashSet<string>();

        foreach (var rangeInfo in target.AssetConfig.Ranges.AllRanges)
        {
            if (assetTypes != null && !assetTypes.Contains(rangeInfo.Range.From.Type))
                continue;

            var assets = new Dictionary<string, List<(AssetLoadContext, byte[])>>();
            foreach (var assetId in rangeInfo.Range)
            {
                if (ids != null && !ids.Contains(assetId)) continue;
                flushCacheFunc();

                var nodes = target.AssetConfig.GetAssetInfo(assetId);
                foreach (var node in nodes)
                {
                    var filename = node.Filename;
                    if (filePattern != null && !filePattern.IsMatch(filename)) continue;
                    if (node.GetProperty(AssetProps.IsReadOnly)) continue;

                    var language = node.GetProperty(AssetProps.Language);
                    var asset = loaderFunc(assetId, language);
                    if (asset == null)
                    {
                        // if (language == null || languages == null || languages.Contains(language))
                        {
                            // Automaps should only load for 3D maps, no need for 'not found' errors, also unmapped ids might be getting requested
                            // due to populating the full range of an XLD, as the ids aren't actually in use it's fine to ignore their absence.
                            if (assetId.Type != AssetType.Automap && AssetMapping.Global.IsMapped(assetId) && !AssetMapping.Global.IsAssetOptional(assetId))
                                Error($"Could not load {assetId}");
                        }

                        continue;
                    }

                    if (filesWritten.Add(filename))
                        Info($"Saving {filename}...");

                    var saveContext = new AssetLoadContext(assetId, node, target.ModContext);
                    SaveAsset(saveContext, asset, assets);
                }
            }

            foreach (var kvp in assets)
            {
                var first = kvp.Value[0].Item1;
                var path = pathResolver.ResolvePath(first.Filename);
                var writeContainer = containerRegistry.GetContainer(path, first.Node.Container, writeDisk);
                writeContainer.Write(path, kvp.Value, target.ModContext);
            }
        }

        Info("Finished saving assets");
    }

    void SaveAsset(AssetLoadContext targetInfo, object asset, Dictionary<string, List<(AssetLoadContext, byte[])>> assets)
    {
        var loaderRegistry = Resolve<IAssetLoaderRegistry>();
        var loader = loaderRegistry.GetLoader(targetInfo.Node.Loader);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        using var s = new AlbionWriter(bw);

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
}