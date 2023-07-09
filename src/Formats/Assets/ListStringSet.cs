using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public class ListStringSet : List<string>, IStringSet
{
    public ListStringSet() { }
    public ListStringSet(int capacity) : base(capacity) { }
    public ListStringSet(IList<string> existing)
    {
        Clear();
        if (existing != null)
        {
            Capacity = existing.Count;
            foreach (var s in existing)
                Add(s);
        }
    }

    public string GetString(StringId id) => Count > id.SubId ? this[id.SubId] : null;
    public void SetString(StringId id, string value)
    {
        while (Count <= id.SubId)
            Add(null);

        this[id.SubId] = value;
    }

    public int FindOrAdd(string text)
    {
        for (int i = 0; i < Count; i++)
            if (this[i] == text)
                return i;

        Add(text);
        return Count - 1;
    }
}