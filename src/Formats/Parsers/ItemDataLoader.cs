using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class ItemDataLoader : IAssetLoader<ItemData>
    {
        public ItemData Serdes(ItemData existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => ItemData.Serdes(info, existing, mapping, s);
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes(existing as ItemData, info, mapping, s, jsonUtil);
    }
}
