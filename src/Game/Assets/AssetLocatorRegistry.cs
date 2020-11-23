using System;
using System.Collections.Generic;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public class AssetLocatorRegistry : ServiceComponent<IAssetLocatorRegistry>, IAssetLocatorRegistry
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<string, IAssetLocator> _locators = new Dictionary<string, IAssetLocator>();

        public IAssetLocator GetLocator(string locatorName)
        {
            lock (_syncRoot)
                return _locators.TryGetValue(locatorName, out var locator) ? locator : Instantiate(locatorName);
        }

        IAssetLocator Instantiate(string locatorName)
        {
            if(string.IsNullOrEmpty(locatorName))
                throw new ArgumentNullException(nameof(locatorName));

            var type = Type.GetType(locatorName);
            if(type == null)
                throw new InvalidOperationException($"Could not find locator type \"{locatorName}\"");

            var constructor = type.GetConstructor(Array.Empty<Type>());
            if(constructor == null)
                throw new InvalidOperationException($"Could not find parameterless constructor for locator type \"{type}\"");

            var locator = (IAssetLocator)constructor.Invoke(Array.Empty<object>());

            if (locator is IComponent component)
                AttachChild(component);

            _locators[locatorName] = locator;
            return locator;
        }
    }
}