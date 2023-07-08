using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public class IntStringDictionary : Dictionary<int, string>, IStringSet
{
    public string GetString(StringId id) => this.GetValueOrDefault(id.SubId);
    public void SetString(StringId id, string value) => this[id.SubId] = value;
}