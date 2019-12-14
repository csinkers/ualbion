using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets
{
    public class AssetLocator : Component, IDisposable
    {
        static readonly IDictionary<AssetType, IAssetLocator> Locators = GetAssetLocators();
        static readonly IDictionary<Type, IAssetPostProcessor> PostProcessors = GetPostProcessors();
        static IDictionary<AssetType, IAssetLocator> GetAssetLocators()
        {
            var dict = new Dictionary<AssetType, IAssetLocator>();
            foreach(var (locator, attribute) in ReflectionHelper.GetAttributeTypes<IAssetLocator, AssetLocatorAttribute>())
                foreach (var objectType in attribute.SupportedTypes)
                    dict.Add(objectType, locator);
            return dict;
        }

        static IDictionary<Type, IAssetPostProcessor> GetPostProcessors()
        {
            var dict = new Dictionary<Type, IAssetPostProcessor>();
            foreach(var (postProcessor, attribute) in ReflectionHelper.GetAttributeTypes<IAssetPostProcessor, AssetPostProcessorAttribute>())
                foreach (var type in attribute.Types)
                    dict.Add(type, postProcessor);
            return dict;
        }

        public AssetLocator() : base(null)
        {
            PerfTracker.StartupEvent("Building AssetLocator");
            _standardAssetLocator = new StandardAssetLocator();
            _assetCache = new AssetCache();
            Children.Add(_assetCache);
            foreach(var locator in Locators.Values.OfType<IComponent>())
                Children.Add(locator);
            PerfTracker.StartupEvent("Built AssetLocator");
        }

        readonly AssetCache _assetCache;
        readonly StandardAssetLocator _standardAssetLocator; 

        IAssetLocator GetLocator(AssetType type)
        {
            if (Locators.TryGetValue(type, out var locator))
                return locator;
            return _standardAssetLocator;
        }

        public object LoadAssetCached<T>(AssetType type, T enumId, GameLanguage language = GameLanguage.English)
        {
            int id = Convert.ToInt32(enumId);
            var key = new AssetKey(type, id, language);
            object asset = _assetCache.Get(key);
            if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            var name = 
                typeof(T) == typeof(int)
                ? $"{type}.{AssetNameResolver.GetName(type, (int)(object)enumId)}"
                : $"{type}.{enumId}";

            try
            {
                IAssetLocator locator = GetLocator(key.Type);
                asset = locator.LoadAsset(key, name, (x, y) => LoadAssetCached(x.Type, x.Id, x.Language));

                if (asset != null && PostProcessors.TryGetValue(asset.GetType(), out var processor))
                    asset = processor.Process(name, asset);
            }
            catch (Exception e)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load asset {name}: {e}"));
                asset = e;
            }

            _assetCache.Add(asset, key);
            return asset is Exception ? null : asset;
        }

        public void Dispose()
        {
            _standardAssetLocator?.Dispose();
        }
    }
}
