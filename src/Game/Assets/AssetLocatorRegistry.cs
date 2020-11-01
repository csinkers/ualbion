using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public sealed class AssetLocatorRegistry : ServiceComponent<IAssetLocatorRegistry>, IAssetLocatorRegistry
    {
        readonly IDictionary<AssetType, List<IAssetLocator>> _locators = new Dictionary<AssetType, List<IAssetLocator>>();
        readonly IDictionary<Type, IAssetPostProcessor> _postProcessors = new Dictionary<Type, IAssetPostProcessor>();
        IAssetLocator _defaultLocator;

        public IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault = false)
        {
            if (locator == null) throw new ArgumentNullException(nameof(locator));
            if (locator is IComponent component)
                AttachChild(component);

            if (!useAsDefault)
            {
                foreach (var assetType in locator.SupportedTypes)
                {
                    if (!_locators.TryGetValue(assetType, out var locatorsForType))
                    {
                        locatorsForType = new List<IAssetLocator>();
                        _locators[assetType] = locatorsForType;
                    }

                    locatorsForType.Add(locator);
                }
            }
            else
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
            return _locators.TryGetValue(type, out var locatorsForType) 
                ? locatorsForType.FirstOrDefault(x => x != null) 
                : _defaultLocator;
        }

        public object LoadAsset(AssetId id, SerializationContext context, AssetInfo info)
        {
            try
            {
                ICoreFactory factory = Resolve<ICoreFactory>();
                IAssetLocator locator = GetLocator(id.Type);
                var asset = locator.LoadAsset(id, context, info);

                if (asset != null && _postProcessors.TryGetValue(asset.GetType(), out var processor))
                    asset = processor.Process(factory, id, asset, context);
                return asset;
            }
            catch (Exception e)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load asset {id}: {e}"));
                return e;
            }
        }
    }
}
