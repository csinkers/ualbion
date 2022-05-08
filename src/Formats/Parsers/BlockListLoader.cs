using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class BlockListLoader : IAssetLoader<BlockList>
{
    public BlockList Serdes(BlockList existing, AssetInfo info, ISerializer s, SerdesContext context)
        => BlockList.Serdes(info?.AssetId.Id ?? 0, existing, s);

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as BlockList, info, s, context);
}
