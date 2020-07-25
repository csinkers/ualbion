using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Palette, FileFormat.PaletteCommon)]
    public class PaletteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if(config.Format == FileFormat.Palette)
                return new AlbionPalette(br, (int)streamLength, key, config.Id);

            return br.ReadBytes(192); // Common palette
        }
    }
}
