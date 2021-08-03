using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class IntStringDictionary : Dictionary<int, string>, IStringCollection
    {
        public string GetString(StringId id, string language) => this.GetValueOrDefault(id.SubId);
    }
}