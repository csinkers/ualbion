using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public class MultiLanguageStringDictionary : Dictionary<string, ListStringCollection>, IStringCollection
{
    public string GetString(StringId id, string language)
        => TryGetValue(language, out var collection) ? collection.GetString(id, language) : null;
}