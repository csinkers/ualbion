using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemNameCollector : Component, IAssetLoader<MultiLanguageStringDictionary>
{
    public MultiLanguageStringDictionary Serdes(MultiLanguageStringDictionary existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (s.IsWriting()) return existing;

        var assets = Resolve<IAssetManager>();
        var ids = 
            context.Mapping.EnumerateAssetsOfType(AssetType.ItemName)
                // make sure TextId->StringId resolution won't happen
                // and get stuck in infinite recursion
                .Select(x => new StringId(x, 0))
                .ToArray();

        var german = ids.ToDictionary(x => x.Id.Id, x => assets.LoadString(x, Base.Language.German));
        var english = ids.ToDictionary(x => x.Id.Id, x => assets.LoadString(x, Base.Language.English));
        var french = ids.ToDictionary(x => x.Id.Id, x => assets.LoadString(x, Base.Language.French));

        static void Add(ListStringSet collection, int i, string value)
        {
            while (collection.Count <= i)
                collection.Add(null);
            collection[i] = value;
        }

        static ListStringSet Build(IDictionary<int, string> dict)
        {
            var list = new ListStringSet();
            foreach (var kvp in dict)
                Add(list, kvp.Key, kvp.Value);
            return list;
        }

        return new MultiLanguageStringDictionary
        {
            [Base.Language.German] = Build(german),
            [Base.Language.English] = Build(english),
            [Base.Language.French] = Build(french)
        };
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((MultiLanguageStringDictionary)existing, info, s, context);
}