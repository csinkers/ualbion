using System.Collections.Generic;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class TextIdStringDictionary : Dictionary<TextId, string>, IStringSet
{
    public string GetString(StringId id, string language) => this.GetValueOrDefault(id.Id);
}