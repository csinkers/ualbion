using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;

namespace UAlbion.Game.Text;

public class WordLookup : GameServiceComponent<IWordLookup>, IWordLookup
{
    readonly Dictionary<string, List<WordId>> _lookup = [];
    readonly Dictionary<WordId, string> _reverse = [];

    public WordLookup() => On<LanguageChangedEvent>(_ => _lookup.Clear());
    public WordId Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            return WordId.None;

        if (_lookup.Count == 0)
            Rebuild();

        var key = s.Trim().ToUpperInvariant();
        return _lookup.TryGetValue(key, out var ids)
            ? ids[0]
            : WordId.None;
    }

    public IEnumerable<WordId> GetHomonyms(WordId word) =>
        _reverse.TryGetValue(word, out var text) 
            ? _lookup[text] 
            : Array.Empty<WordId>();

    public string GetText(WordId id, string language) 
        => _reverse.TryGetValue(id, out var text) ? text : id.ToString();

    void Rebuild()
    {
        _reverse.Clear();

        var assets = Assets;
        foreach (var id in AssetId.EnumerateAll(AssetType.Word))
        {
            var text = assets.LoadStringSafe(id).Trim().ToUpperInvariant();
            if (!_lookup.TryGetValue(text, out var ids))
            {
                ids = [];
                _lookup[text] = ids;
            }

            ids.Add(id);
            _reverse[id] = text;
        }
    }
}
