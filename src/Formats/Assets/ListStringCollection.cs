using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public class ListStringCollection : List<string>, IStringCollection
{
    public ListStringCollection() { }
    public ListStringCollection(IList<string> existing)
    {
        Clear();
        if (existing != null)
            foreach (var s in existing)
                Add(s);
    }

    public string GetString(StringId id, string language) => Count > id.SubId ? this[id.SubId] : null;
}