using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class ItemDataLoader : Component, IAssetLoader<ItemData>
    {
        public ItemData Serdes(ItemData existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => ItemData.Serdes(info, existing, s, Resolve<ISpellManager>());
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes(existing as ItemData, info, mapping, s, jsonUtil);
    }
}