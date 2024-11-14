using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets;

class ModLoader : Component // Shouldn't be referenced outside ModApplier
{
    readonly List<ModInfo> _modsInReverseDependencyOrder = [];
    readonly Dictionary<string, ModInfo> _mods = [];
    readonly Dictionary<string, LanguageConfig> _languages = [];

    public IReadOnlyList<ModInfo> ModsInReverseDependencyOrder => _modsInReverseDependencyOrder;
    public IReadOnlyDictionary<string, LanguageConfig> Languages { get; }

    public ModLoader()
    {
        Languages = new ReadOnlyDictionary<string, LanguageConfig>(_languages);
    }

    public void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IReadOnlyList<string> mods)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(pathResolver);
        ArgumentNullException.ThrowIfNull(mods);

        pathResolver.RegisterPath("ALBION", pathResolver.ResolvePathAbsolute(ReadVar(V.User.Path.Albion)));
        pathResolver.RegisterPath("SAVES", pathResolver.ResolvePathAbsolute(ReadVar(V.User.Path.Saves)));

        _mods.Clear();
        _modsInReverseDependencyOrder.Clear();
        TryResolve<IVarRegistry>()?.Clear();
        mapping.Clear();

        foreach (var mod in mods.Reverse())
            LoadMod(pathResolver.ResolvePathAbsolute("$(MODS)"), mod.Trim(), mapping);

        _modsInReverseDependencyOrder.Reverse();

        Raise(ModsLoadedEvent.Instance);
    }

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
}