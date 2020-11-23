using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class BlockListLoader : IAssetLoader<BlockList>
    {
        public BlockList Serdes(BlockList existing, AssetInfo config, AssetMapping mapping, ISerializer s)
         => Block.Serdes(config?.Id ?? 0, existing, s);

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as BlockList, config, mapping, s);
    }
}
