using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public class AssetLocatorRegistry : ServiceComponent<IAssetLocatorRegistry>, IAssetLocatorRegistry
    {
        readonly IDictionary<AssetType, IAssetLocator> _locators = new Dictionary<AssetType, IAssetLocator>();
        readonly IDictionary<Type, IAssetPostProcessor> _postProcessors = new Dictionary<Type, IAssetPostProcessor>();

        public AssetLocatorRegistry()
        {
            PerfTracker.StartupEvent("Building AssetLocatorRegistry");
            _standardAssetLocator = new StandardAssetLocator();
            _assetCache = AttachChild(new AssetCache());
            //foreach(var locator in Locators.Values.OfType<IComponent>())
            //    AttachChild(locator);
            PerfTracker.StartupEvent("Built AssetLocatorRegistry");
        }

        public IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator)
        {
            if (locator is IComponent component)
                AttachChild(component);

            foreach (var assetType in locator.SupportedTypes)
            {
                if (_locators.ContainsKey(assetType))
                    throw new InvalidOperationException($"A locator is already defined for {assetType}");
                _locators[assetType] = locator;
            }

            return this;
        }

        public IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor)
        {
            foreach (var type in postProcessor.SupportedTypes)
            {
                if(_postProcessors.ContainsKey(type))
                    throw new InvalidOperationException($"A post-processor is already defined for {type}");
                _postProcessors[type] = postProcessor;
            }

            return this;
        }

        readonly AssetCache _assetCache;
        readonly StandardAssetLocator _standardAssetLocator;

        IAssetLocator GetLocator(AssetType type)
        {
            if (_locators.TryGetValue(type, out var locator))
                return locator;
            return _standardAssetLocator;
        }

        public object LoadAssetCached(AssetKey key)
        {
            object asset = _assetCache.Get(key);
            if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            asset = LoadAssetInternal(key);
            _assetCache.Add(asset, key);
            return asset is Exception ? null : asset;
        }

        public object LoadAsset(AssetKey key)
        {
            var asset = LoadAssetInternal(key);
            return asset is Exception ? null : asset;
        }

        object LoadAssetInternal(AssetKey key)
        {
            var name = $"{key.Type}.{key.Id}";
            try
            {
                ICoreFactory factory = Resolve<ICoreFactory>();
                IAssetLocator locator = GetLocator(key.Type);
                var asset = locator.LoadAsset(key, name, LoadAssetCached);

                if (asset != null && _postProcessors.TryGetValue(asset.GetType(), out var processor))
                    asset = processor.Process(factory, key, asset, LoadAssetCached);
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
