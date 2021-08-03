using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class TextIdStringDictionary : Dictionary<TextId, string>, IStringCollection
    {
        public string GetString(StringId id, string language) => this.GetValueOrDefault(id.Id);
    }
}