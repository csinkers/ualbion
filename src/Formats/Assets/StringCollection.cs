using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class StringCollection : List<string>
    {
        public StringCollection() { }

        public StringCollection(IList<string> existing)
        {
            Clear();
            if (existing != null)
                foreach (var s in existing)
                    Add(s);
        }
    }
}