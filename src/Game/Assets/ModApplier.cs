using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets
{
    public class ModApplier : Component, IModApplier
    {
        readonly Dictionary<string, ModInfo> _mods = new Dictionary<string, ModInfo>();
        readonly List<ModInfo> _modsInDependencyOrder = new List<ModInfo>();
        readonly AssetCache _assetCache = new AssetCache();
        IAssetLocatorRegistry _assetLocatorRegistry;
        GameLanguage _language = GameLanguage.English;

        public ModApplier()
        {
            AttachChild(_assetCache);
            On<SetLanguageEvent>(e =>
            {
                if (_language == e.Language)
                    return;

                _language = e.Language;
                Raise(new ReloadAssetsEvent());
                Raise(new LanguageChangedEvent());
            });
        }

        protected override void Subscribed()
        {
            _assetLocatorRegistry = Resolve<IAssetLocatorRegistry>();
            var dataDir = Resolve<IGeneralConfig>().BaseDataPath;
            Exchange.Register<IModApplier>(this);

            foreach (var mod in Resolve<IGameplaySettings>().Mods)
                LoadMod(dataDir, mod);
        }

        void LoadMod(string dataDir, string modName)
        {
            if (string.IsNullOrEmpty(modName))
                return;

            var path = Path.Combine(dataDir, "Mods", modName);

            if (modName != "../Base" && modName.Any(c => c == '\\' || c == '/' || c == Path.DirectorySeparatorChar))
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Mod {modName} is not a simple directory name"));
                return;
            }

            var assetConfigPath = Path.Combine(path, "assets.json");
            var modConfigPath = Path.Combine(path, "modinfo.json");
            if (!File.Exists(assetConfigPath))
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Mod {modName} does not contain an asset.config file"));
                return;
            }

            if (!File.Exists(modConfigPath))
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Mod {modName} does not contain an modinfo.config file"));
                return;
            }

            var assetConfig = AssetConfig.Load(assetConfigPath);
            var modConfig = ModConfig.Load(modConfigPath);
            var modInfo = new ModInfo(modName, assetConfig, modConfig);

            // Load dependencies
            foreach (var dependency in modConfig.Dependencies)
            {
                LoadMod(dataDir, dependency);
                if (!_mods.TryGetValue(dependency, out var dependencyInfo))
                {
                    Raise(new LogEvent(LogEvent.Level.Error, $"Dependency {dependency} of mod {modName} could not be loaded, skipping load of {modName}"));
                    return;
                }

                modInfo.Mapping.MergeFrom(dependencyInfo.Mapping);
            }

            MergeTypesToMapping(modInfo.Mapping, assetConfig, assetConfigPath);
            AssetMapping.Global.MergeFrom(modInfo.Mapping);
            _mods.Add(modName, modInfo);
            _modsInDependencyOrder.Add(modInfo);
        }

        static void MergeTypesToMapping(AssetMapping mapping, AssetConfig config, string assetConfigPath)
        {
            foreach (var assetType in config.Types)
            {
                var enumType = Type.GetType(assetType.Key);
                if (enumType == null)
                    throw new InvalidOperationException($"Could not load enum type \"{assetType.Key}\" defined in \"{assetConfigPath}\"");

                mapping.RegisterAssetType(enumType, assetType.Value.AssetType);
            }
        }

        public AssetInfo GetAssetInfo(AssetId id)
        {
            var (enumType, enumId) = AssetMapping.Global.IdToEnum(id);
            // TODO: Allow partial modification by mods rather than only full replacement
            return _modsInDependencyOrder.Select(x => x.AssetConfig.GetAsset(enumType.FullName, enumId)).LastOrDefault();
        }

        public object LoadAsset(AssetId id) => LoadAssetInternal(id, false);
        public object LoadAssetCached(AssetId id) => LoadAssetInternal(id, true);

        object LoadAssetInternal(AssetId id, bool cache)
        {
            object asset = cache ? _assetCache.Get(id) : null;
            if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            var (enumType, enumId) = AssetMapping.Global.IdToEnum(id);
            foreach (var mod in _modsInDependencyOrder)
            {
                var info = mod.AssetConfig.GetAsset(enumType.FullName, enumId);
                if (info == null)
                    continue;

                var context = new SerializationContext(mod.Mapping, _language);
                var modAsset = _assetLocatorRegistry.LoadAsset(id, context, info);
                if(modAsset != null)
                    asset = modAsset; // TODO: Apply json patches etc
            }

            if (cache)
                _assetCache.Add(asset, id);

            return asset is Exception ? null : asset;
        }

        public SavedGame LoadSavedGame(string path)
        {
            throw new NotImplementedException();
        }
    }
}