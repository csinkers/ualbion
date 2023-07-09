using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class BlockListLoader : IAssetLoader<BlockList>
{
    public BlockList Serdes(BlockList existing, ISerializer s, AssetLoadContext context)
        => BlockList.Serdes(context?.AssetId.Id ?? 0, existing, s);

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as BlockList, s, context);
}
