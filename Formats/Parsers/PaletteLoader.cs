using System.IO;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.Palette, XldObjectType.PaletteCommon)]
    public class PaletteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            if(config.Type == XldObjectType.Palette)
                return new AlbionPalette(br, (int)streamLength, name, config.Id);

            return br.ReadBytes(192); // Common palette
        }
    }
}