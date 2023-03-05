using System.Collections.Generic;
using UAlbion.Config;

namespace UAlbion.Formats.Assets;

public class AssetIdStringDictionary : Dictionary<AssetId, string>, IStringSet
{
    public string GetString(StringId id, string language) => this.GetValueOrDefault(id.Id);
}