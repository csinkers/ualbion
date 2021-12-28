using System;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets;

public class WordCollector : Component, IAssetLoader<ListStringCollection>
{
    static readonly AssetId Words1 = AssetId.From(Base.Special.Words1);
    static readonly AssetId Words2 = AssetId.From(Base.Special.Words2);
    static readonly AssetId Words3 = AssetId.From(Base.Special.Words3);

    public ListStringCollection Serdes(ListStringCollection existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (s.IsWriting()) return existing;

        var predicate = info.AssetId switch
        {
            { } x when x == Words1 => (Func<int, bool>)(x => x <= 500),
            { } x when x == Words2 => (Func<int, bool>)(x => x > 500 && x <= 1000),
            { } x when x == Words3 => (Func<int, bool>)(x => x > 1000),
            _ => throw new ArgumentOutOfRangeException(nameof(info))
        };

        var assets = Resolve<IAssetManager>();
        var ids =
            mapping.EnumerateAssetsOfType(AssetType.Word)
                .Where(x => predicate(x.Id))
                // make sure TextId->StringId resolution won't happen
                // and get stuck in infinite recursion
                .Select(x => new StringId(x, 0))
                .ToArray();

        var language = info.Get(AssetProperty.Language, Base.Language.English);
        var dict = ids.ToDictionary(
            x => x.Id.Id,
            x => assets.LoadString(x, language));

        var list = new ListStringCollection();
        for (int i = 0; i < 1500; i++)
            list.Add(null);

        foreach (var kvp in dict)
            list[kvp.Key - 1] = kvp.Value;
        return list;
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((ListStringCollection) existing, info, mapping, s, jsonUtil);
}