using System;
using System.Collections;
using System.Collections.Generic;

namespace UAlbion.Formats.Assets.Save;

public class VisitedEventCollection : IList<VisitedEvent>
{
    readonly List<VisitedEvent> _list = new();
    readonly HashSet<VisitedEvent> _set = new();
    public IEnumerator<VisitedEvent> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    public void Add(VisitedEvent item)
    {
        if (_set.Add(item))
            _list.Add(item);
    }

    public void Clear()
    {
        _list.Clear();
        _set.Clear();
    }

    public bool Contains(VisitedEvent item) => _set.Contains(item);
    public bool Remove(VisitedEvent item)
    {
        _set.Remove(item);
        return _list.Remove(item);
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;
    public int IndexOf(VisitedEvent item) => _list.IndexOf(item);
    public void CopyTo(VisitedEvent[] array, int arrayIndex) => throw new NotSupportedException();
    public void Insert(int index, VisitedEvent item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
    public VisitedEvent this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }
}