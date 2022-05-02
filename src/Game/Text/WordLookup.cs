using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Text;

public class WordLookup : ServiceComponent<IWordLookup>, IWordLookup
{
    readonly Dictionary<string, WordId> _lookup = new();

    public WordLookup() => On<LanguageChangedEvent>(_ => _lookup.Clear());
    public WordId Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            return AssetId.None;

        if (_lookup.Count == 0)
            Rebuild();

        return _lookup.TryGetValue(s.Trim().ToUpperInvariant(), out var id) ? id : (WordId)AssetId.None;
    }

    void Rebuild()
    {
        var assets = Resolve<IAssetManager>();
        foreach(var id in AssetId.EnumerateAll(AssetType.Word))
        {
            var text = assets.LoadString(id);
            _lookup[text.Trim().ToUpperInvariant()] = id;
        }
    }
}