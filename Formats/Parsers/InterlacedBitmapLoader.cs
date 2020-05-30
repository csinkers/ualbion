using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.InterlacedBitmap)]
    public class InterlacedBitmapLoader : IAssetLoader<InterlacedBitmap>
    {
        public InterlacedBitmap Serdes(InterlacedBitmap existing, ISerializer s, AssetKey key, AssetInfo config) 
            => InterlacedBitmap.Serdes(existing, s);

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config) 
            => InterlacedBitmap.Serdes(null,
                new GenericBinaryReader(br, streamLength, FormatUtil.BytesTo850String, ApiUtil.Assert));
    }
}
