﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Containers;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets
{
    public class ModApplier : Component, IModApplier
    {
        readonly Dictionary<string, ModInfo> _mods = new();
        readonly List<ModInfo> _modsInReverseDependencyOrder = new();
        readonly AssetCache _assetCache = new();
        readonly Dictionary<string, LanguageConfig> _languages = new();
        readonly Dictionary<string, string> _extraPaths = new(); // Just used for $(MOD)

        IAssetLocator _assetLocator;

        public ModApplier()
        {
            Languages = new ReadOnlyDictionary<string, LanguageConfig>(_languages);
            AttachChild(_assetCache);
            On<SetLanguageEvent>(e =>
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

        public void LoadMods(IGeneralConfig config, IList<string> mods)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (mods == null) throw new ArgumentNullException(nameof(mods));

            _mods.Clear();
            _modsInReverseDependencyOrder.Clear();
            AssetMapping.Global.Clear();

            var disk = Resolve<IFileSystem>();
            foreach (var mod in mods)
                LoadMod(config.ResolvePath("$(MODS)"), mod, disk);

            _modsInReverseDependencyOrder.Reverse();

        }

        public IEnumerable<string> ShaderPaths =>
            _modsInReverseDependencyOrder
                .Select(mod => mod.ShaderPath)
                .Where(x => x != null);

        void LoadMod(string dataDir, string modName, IFileSystem disk)
        {
            if (string.IsNullOrEmpty(modName))
                return;

            if (_mods.ContainsKey(modName))
                return;

            if (modName.Any(c => c == '\\' || c == '/' || c == Path.DirectorySeparatorChar))
            {
                Error($"Mod {modName} is not a simple directory name");
                return;
            }

            string path = Path.Combine(dataDir, modName);

            var assetConfigPath = Path.Combine(path, "assets.json");
            if (!disk.FileExists(assetConfigPath))
            {
                Error($"Mod {modName} does not contain an asset.config file");
                return;
            }

            var modConfigPath = Path.Combine(path, "modinfo.json");
            if (!disk.FileExists(modConfigPath))
            {
                Error($"Mod {modName} does not contain an modinfo.config file");
                return;
            }

            var generalConfig = Resolve<IGeneralConfig>();
            var assetConfig = AssetConfig.Load(assetConfigPath, disk);
            var modConfig = ModConfig.Load(modConfigPath, disk);
            var modInfo = new ModInfo(modName, assetConfig, modConfig, path);

            // Load dependencies
            foreach (var dependency in modConfig.Dependencies)
            {
                LoadMod(dataDir, dependency, disk);
                if (!_mods.TryGetValue(dependency, out var dependencyInfo))
                {
                    Error($"Dependency {dependency} of mod {modName} could not be loaded, skipping load of {modName}");
                    return;
                }

                modInfo.Mapping.MergeFrom(dependencyInfo.Mapping);
            }

            foreach (var kvp in assetConfig.Languages)
                _languages[kvp.Key] = kvp.Value;

            MergeTypesToMapping(modInfo.Mapping, assetConfig, assetConfigPath);
            AssetMapping.Global.MergeFrom(modInfo.Mapping);
            modConfig.AssetPath ??= path;
            var extraPaths = new Dictionary<string, string> { ["MOD"] = modConfig.AssetPath };
            assetConfig.PopulateAssetIds(
                AssetMapping.Global,
                x => _assetLocator.GetSubItemRangesForFile(x, extraPaths),
                x => disk.ReadAllText(generalConfig.ResolvePath(x, extraPaths)));
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
                var asset = LoadAssetInternal(id, _extraPaths, language);
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

        public object LoadAssetCached(AssetId id)
        {
            object asset = _assetCache.Get(id);
            if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            asset = LoadAsset(id, null);

            _assetCache.Add(asset, id);
            return asset is Exception ? null : asset;
        }

        object LoadAssetInternal(AssetId id, Dictionary<string, string> extraPaths, string language = null)
        {
            if (id.IsNone)
                return null;

            if (id.Type == AssetType.MetaFont)
                return Resolve<IMetafontBuilder>().Build((MetaFontId)id.Id);

            object asset = null;
            Stack<IPatch> patches = null; // Create the stack lazily, as most assets won't have any patches.
            foreach (var mod in _modsInReverseDependencyOrder)
            {
                foreach (var info in mod.AssetConfig.GetAssetInfo(id))
                {
                    var assetLang = info.Get<string>(AssetProperty.Language, null);
                    if (assetLang != null)
                    {
                        language ??= Resolve<IGameplaySettings>().Language;
                        if (!string.Equals(assetLang, language, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    extraPaths ??= new Dictionary<string, string>();
                    extraPaths["MOD"] = mod.AssetPath;
                    var modAsset = _assetLocator.LoadAsset(id, mod.Mapping, info, extraPaths);

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
                        break;
                    }
                }
            }

            if (asset == null)
                throw new AssetNotFoundException($"Could not load asset for {id}");

            while (patches != null && patches.Count > 0)
                asset = patches.Pop().Apply(asset);

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
            return SavedGame.Serdes(null, AssetMapping.Global, s);
        }

        public void SaveAssets(
            Func<AssetId, string, (object, AssetInfo)> loaderFunc,
            Action flushCacheFunc,
            ISet<AssetId> ids,
            ISet<AssetType> assetTypes,
            Regex filePattern)
        {
            if (loaderFunc == null) throw new ArgumentNullException(nameof(loaderFunc));
            if (flushCacheFunc == null) throw new ArgumentNullException(nameof(flushCacheFunc));

            var config = Resolve<IGeneralConfig>();
            var loaderRegistry = Resolve<IAssetLoaderRegistry>();
            var containerRegistry = Resolve<IContainerRegistry>();
            var disk = Resolve<IFileSystem>();
            var target = _modsInReverseDependencyOrder.First();

            // Add any missing ids
            Info("Populating destination asset info...");
            var extraPaths = new Dictionary<string, string> { ["MOD"] = target.AssetPath };
            target.AssetConfig.PopulateAssetIds(
                AssetMapping.Global,
                file =>
                {
                    // Don't need to resolve the filename as we're not actually using the container - we just want to find the type.
                    var container = containerRegistry.GetContainer(file.Filename, file.Container, disk);
                    var firstAsset = file.Map[file.Map.Keys.Min()];
                    if (assetTypes != null && !assetTypes.Contains(firstAsset.AssetId.Type))
                        return new List<(int, int)> { (firstAsset.Index, 1) };

                    if (filePattern != null && !filePattern.IsMatch(file.Filename))
                        return new List<(int, int)> { (firstAsset.Index, 1) };

                    var assets = target.Mapping.EnumerateAssetsOfType(firstAsset.AssetId.Type).ToList();
                    var idsInRange =
                        assets
                        .Where(x => x.Id >= firstAsset.AssetId.Id)
                        .OrderBy(x => x.Id)
                        .Select(x => x.Id - firstAsset.AssetId.Id + firstAsset.Index);

                    if (container is XldContainer)
                        idsInRange = idsInRange.Where(x => x < 100);

                    int maxSubId = file.Get(AssetProperty.Max, -1);
                    if (maxSubId != -1)
                        idsInRange = idsInRange.Where(x => x <= maxSubId);

                    return FormatUtil.SortedIntsToRanges(idsInRange);
                }, x => disk.ReadAllText(config.ResolvePath(x, extraPaths)));

            Resolve<IGeneralConfig>().SetPath("MOD", target.AssetPath);
            foreach (var file in target.AssetConfig.Files.Values)
            {
                if (filePattern != null && !filePattern.IsMatch(file.Filename))
                    continue;

                bool notify = true;
                flushCacheFunc();
                var path = config.ResolvePath(file.Filename);
                var loader = loaderRegistry.GetLoader(file.Loader);
                var container = containerRegistry.GetContainer(path, file.Container, disk);
                var assets = new List<(AssetInfo, byte[])>();
                foreach (var assetInfo in file.Map.Values)
                {
                    if (ids != null && !ids.Contains(assetInfo.AssetId)) continue;
                    if (assetTypes != null && !assetTypes.Contains(assetInfo.AssetId.Type)) continue;

                    var language = assetInfo.Get<string>(AssetProperty.Language, null);
                    var (asset, sourceInfo) = loaderFunc(assetInfo.AssetId, language);
                    if (asset == null) continue;

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
                    loader.Serdes(asset, assetInfo, target.Mapping, s);

                    ms.Position = 0;
                    assets.Add((assetInfo, ms.ToArray()));
                }

                if (assets.Count > 0)
                    container.Write(path, assets, disk);
            }
        }
    }
}
