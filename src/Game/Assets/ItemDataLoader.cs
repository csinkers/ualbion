using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets;

public class ItemDataLoader : Component, IAssetLoader<ItemData>
{
    public ItemData Serdes(ItemData existing, AssetInfo info, ISerializer s, LoaderContext context)
        => ItemData.Serdes(info, existing, s, Resolve<ISpellManager>());
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as ItemData, info, s, context);
}
