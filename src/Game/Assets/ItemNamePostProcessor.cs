using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets
{
    public class ItemNamePostProcessor : Component, IAssetPostProcessor
    {
        public object Process(ICoreFactory factory, AssetId key, object asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            var dict = (IDictionary<(int, GameLanguage), string>)asset;
            var result = new Dictionary<int, string>();
            var settings = Resolve<IGameplaySettings>();
            foreach (var kvp in dict)
                if (kvp.Key.Item2 == settings.Language)
                    result[kvp.Key.Item1] = kvp.Value;
            return result;
        }

        public IEnumerable<Type> SupportedTypes => new[]
        {
            typeof(IDictionary<(int, GameLanguage), string>),
            typeof(Dictionary<(int, GameLanguage), string>)
        };
    }
}