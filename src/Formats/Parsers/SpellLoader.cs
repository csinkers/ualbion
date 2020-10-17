using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SpellData)]
    public class SpellLoader : IAssetLoader<IList<SpellData>>
    {
        public IList<SpellData> Serdes(IList<SpellData> existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            existing ??= new SpellData[SpellData.SpellClasses * SpellData.MaxSpellsPerClass];
            s.List(nameof(SpellData), existing, existing.Count, SpellData.Serdes);
            return existing;
        }

        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
            => Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);
    }
}
