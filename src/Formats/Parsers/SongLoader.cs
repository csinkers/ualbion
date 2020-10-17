using System;
using System.IO;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Song)]
    public class SongLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            return br.ReadBytes((int) streamLength);
        }
    }
}
