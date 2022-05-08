using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemDataLoader : Component, IAssetLoader<ItemData>
{
    public ItemData Serdes(ItemData existing, AssetInfo info, ISerializer s, SerdesContext context)
        => ItemData.Serdes(info, existing, s, Resolve<ISpellManager>());
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as ItemData, info, s, context);
}