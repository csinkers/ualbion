using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public class AssetLocatorRegistry : Component, IDisposable
    {
        readonly IDictionary<AssetType, IAssetLocator> _locators = new Dictionary<AssetType, IAssetLocator>();
        readonly IDictionary<Type, IAssetPostProcessor> _postProcessors = new Dictionary<Type, IAssetPostProcessor>();
        /*
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
        */

        public AssetLocatorRegistry()
        {
            PerfTracker.StartupEvent("Building AssetLocatorRegistry");
            _standardAssetLocator = new StandardAssetLocator();
            _assetCache = AttachChild(new AssetCache());
            //foreach(var locator in Locators.Values.OfType<IComponent>())
            //    AttachChild(locator);
            PerfTracker.StartupEvent("Built AssetLocatorRegistry");
        }

        public void AddAssetLocator(IAssetLocator locator)
        {
            if (locator is IComponent component)
                AttachChild(component);

            foreach (var assetType in locator.SupportedTypes)
            {
                if (_locators.ContainsKey(assetType))
                    throw new InvalidOperationException($"A locator is already defined for {assetType}");
                _locators[assetType] = locator;
            }
        }

        public void AddAssetPostProcessor(IAssetPostProcessor postProcessor)
        {
            foreach (var type in postProcessor.SupportedTypes)
            {
                if(_postProcessors.ContainsKey(type))
                    throw new InvalidOperationException($"A post-processor is already defined for {type}");
                _postProcessors[type] = postProcessor;
            }
        }

        readonly AssetCache _assetCache;
        readonly StandardAssetLocator _standardAssetLocator;

        IAssetLocator GetLocator(AssetType type)
        {
            if (_locators.TryGetValue(type, out var locator))
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

            asset = LoadAssetInternal(key, name, language);

            _assetCache.Add(asset, key);
            return asset is Exception ? null : asset;
        }

        public object LoadAsset<T>(AssetType type, T enumId, GameLanguage language = GameLanguage.English)
        {
            int id = Convert.ToInt32(enumId);
            var key = new AssetKey(type, id, language);
            var name =
                typeof(T) == typeof(int)
                ? $"{type}.{AssetNameResolver.GetName(type, (int)(object)enumId)}"
                : $"{type}.{enumId}";

            var asset = LoadAssetInternal(key, name, language);

            return asset is Exception ? null : asset;
        }

        object LoadAssetInternal(AssetKey key, string name, GameLanguage language)
        {
            try
            {
                ICoreFactory factory = Resolve<ICoreFactory>();
                IAssetLocator locator = GetLocator(key.Type);
                var asset = locator.LoadAsset(key, name, (x, y) => LoadAssetCached(x.Type, x.Id, x.Language));

                if (asset != null && _postProcessors.TryGetValue(asset.GetType(), out var processor))
                    asset = processor.Process(factory, key, name, asset);
                return asset;
            }
            catch (Exception e)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load asset {name}: {e}"));
                return e;
            }
        }

        public void Dispose()
        {
            _standardAssetLocator?.Dispose();
        }
    }
}
