using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public class AssetIdStringDictionary : Dictionary<AssetId, string>, IStringCollection
    {
        public string GetString(StringId id, string language) => this.GetValueOrDefault(id.Id);
    }
}