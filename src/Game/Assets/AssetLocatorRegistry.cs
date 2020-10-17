using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public sealed class AssetLocatorRegistry : ServiceComponent<IAssetLocatorRegistry>, IAssetLocatorRegistry
    {
        readonly IDictionary<AssetType, IAssetLocator> _locators = new Dictionary<AssetType, IAssetLocator>();
        readonly IDictionary<Type, IAssetPostProcessor> _postProcessors = new Dictionary<Type, IAssetPostProcessor>();
        readonly AssetCache _assetCache;
        IAssetLocator _defaultLocator;

        public AssetLocatorRegistry()
        {
            PerfTracker.StartupEvent("Building AssetLocatorRegistry");
            _assetCache = AttachChild(new AssetCache());
            PerfTracker.StartupEvent("Built AssetLocatorRegistry");
        }

        public IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault = false)
        {
            if (locator == null) throw new ArgumentNullException(nameof(locator));
            if (locator is IComponent component)
                AttachChild(component);

            foreach (var assetType in locator.SupportedTypes)
            {
                if (_locators.ContainsKey(assetType))
                    throw new InvalidOperationException($"A locator is already defined for {assetType}");
                _locators[assetType] = locator;
            }

            if (useAsDefault)
            {
                if(_defaultLocator != null)
                    throw new InvalidOperationException($"Tried to set a second default asset locator \"{locator.GetType()}\", but one of type \"{_defaultLocator.GetType()}\" is already set.");
                _defaultLocator = locator;
            }

            return this;
        }

        public IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor)
        {
            if (postProcessor == null) throw new ArgumentNullException(nameof(postProcessor));
            foreach (var type in postProcessor.SupportedTypes)
            {
                if(_postProcessors.ContainsKey(type))
                    throw new InvalidOperationException($"A post-processor is already defined for {type}");
                _postProcessors[type] = postProcessor;
            }

            return this;
        }

        IAssetLocator GetLocator(AssetType type)
        {
            if (_locators.TryGetValue(type, out var locator))
                return locator;
            return _defaultLocator;
        }

        public object LoadAssetCached<T>(T key, SerializationContext context) where T : unmanaged, Enum => LoadAssetCached(AssetId.From(key), context);
        public object LoadAssetCached(AssetId key, SerializationContext context)
        {
            object asset = _assetCache.Get(key);
            if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            asset = LoadAssetInternal(key, context);
            _assetCache.Add(asset, key);
            return asset is Exception ? null : asset;
        }

        public object LoadAsset<T>(T key, SerializationContext context) where T : unmanaged, Enum => LoadAsset(AssetId.From(key), context);
        public object LoadAsset(AssetId key, SerializationContext context)
        {
            var asset = LoadAssetInternal(key, context);
            return asset is Exception ? null : asset;
        }

        public AssetInfo GetAssetInfo(AssetId key) => GetLocator(key.Type).GetAssetInfo(key, LoadAssetCached);

        object LoadAssetInternal(AssetId key, SerializationContext context)
        {
            try
            {
                ICoreFactory factory = Resolve<ICoreFactory>();
                IAssetLocator locator = GetLocator(key.Type);
                var asset = locator.LoadAsset(key, context, LoadAssetCached);

                if (asset != null && _postProcessors.TryGetValue(asset.GetType(), out var processor))
                    asset = processor.Process(factory, key, asset, context, LoadAssetCached);
                return asset;
            }
            catch (Exception e)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load asset {key}: {e}"));
                return e;
            }
        }
    }
}
