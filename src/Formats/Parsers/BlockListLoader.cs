using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class BlockListLoader : IAssetLoader<BlockList>
{
    public BlockList Serdes(BlockList existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => BlockList.Serdes(info?.AssetId.Id ?? 0, existing, s);

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes(existing as BlockList, info, mapping, s, jsonUtil);
}