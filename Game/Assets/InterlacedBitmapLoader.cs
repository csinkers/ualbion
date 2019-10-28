using System.IO;
using SixLabors.ImageSharp;
using UAlbion.Formats;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    // In UAlbion.Game instead of Formats to avoid adding an ImageSharp dependency to Formats.
    // Also need to add a proper loader for the interlaced image format, rather than relying
    // on pre-converted PNGs.
    [AssetLoader(FileFormat.InterlacedBitmap)]
    public class InterlacedBitmapLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            return Image.Load(br.BaseStream);
        }
    }
}