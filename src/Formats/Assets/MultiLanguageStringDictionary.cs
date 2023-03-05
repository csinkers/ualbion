using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public class MultiLanguageStringDictionary : Dictionary<string, ListStringSet>, IStringSet
{
    public string GetString(StringId id, string language)
        => TryGetValue(language, out var collection) ? collection.GetString(id, language) : null;
}