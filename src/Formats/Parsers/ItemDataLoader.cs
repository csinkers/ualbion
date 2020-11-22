using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ItemData)]
    public class ItemDataLoader : IAssetLoader<ItemData>
    {
        public ItemData Serdes(ItemData existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => ItemData.Serdes(config, existing, mapping, s);
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as ItemData, config, mapping, s);
    }
}
