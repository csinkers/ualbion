using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.InterlacedBitmap)]
    public class InterlacedBitmapLoader : IAssetLoader<InterlacedBitmap>
    {
        public InterlacedBitmap Serdes(InterlacedBitmap existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config) 
            => InterlacedBitmap.Serdes(existing, s);

        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config) 
            => InterlacedBitmap.Serdes(null,
                new GenericBinaryReader(br, streamLength, FormatUtil.BytesTo850String, ApiUtil.Assert));
    }
}
