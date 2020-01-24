using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Palette, FileFormat.PaletteCommon)]
    public class PaletteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            if(config.Format == FileFormat.Palette)
                return new AlbionPalette(br, (int)streamLength, name, config.Id);

            return br.ReadBytes(192); // Common palette
        }
    }
}
