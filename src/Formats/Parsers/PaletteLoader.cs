using System;
using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Palette, FileFormat.PaletteCommon)]
    public class PaletteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.Format == FileFormat.Palette)
                return new AlbionPalette(br, (int)streamLength, id);

            return br.ReadBytes(192); // Common palette
        }
    }
}
