using System;
using System.Collections;
using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class StringCollection : IList<string>
    {
        readonly string[] _table;
        public StringCollection(string[] table) => _table = table ?? throw new ArgumentNullException(nameof(table));
        public IEnumerator<string> GetEnumerator() => ((IList<string>)_table).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _table.GetEnumerator();
        public void Add(string item) => throw new InvalidOperationException();
        public void Clear() => throw new InvalidOperationException();
        public bool Contains(string item) => ((IList<string>)_table).Contains(item);
        public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(string item) => throw new InvalidOperationException();
        public int Count => _table.Length;
        public bool IsReadOnly => false;
        public int IndexOf(string item) => ((IList<string>)_table).IndexOf(item);
        public void Insert(int index, string item) => throw new InvalidOperationException();
        public void RemoveAt(int index) => throw new InvalidOperationException();
        public string this[int index] { get => _table[index]; set => _table[index] = value; }
    }
}