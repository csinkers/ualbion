using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SpellData)]
    public class SpellLoader : IAssetLoader<IList<SpellData>>
    {
        public IList<SpellData> Serdes(IList<SpellData> existing, ISerializer s, AssetKey key, AssetInfo config)
        {
            existing ??= new SpellData[SpellData.SpellClasses * SpellData.MaxSpellsPerClass];
            s.List(nameof(SpellData), existing, existing.Count, SpellData.Serdes);
            return existing;
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config) 
            => Serdes(null, new AlbionReader(br, streamLength), key, config);
    }
}