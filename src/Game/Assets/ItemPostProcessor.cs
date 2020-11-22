using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class ItemPostProcessor : Component, IAssetPostProcessor
    {
        public object Process(ICoreFactory factory, AssetId key, object asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            var item = (ItemData)asset;
            var assets = Resolve<IAssetManager>();
            var names = assets.LoadItemNames();
            if (names.TryGetValue(item.Id, out var name))
                item.Name = name;
            return item;
        }

        public IEnumerable<Type> SupportedTypes => new[] { typeof(ItemData) };
    }
}
