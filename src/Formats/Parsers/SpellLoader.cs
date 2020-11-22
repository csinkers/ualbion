using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SpellData)]
    public class SpellLoader : IAssetLoader<IList<SpellData>>
    {
        public IList<SpellData> Serdes(IList<SpellData> existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            existing ??= new SpellData[SpellData.SpellClasses * SpellData.MaxSpellsPerClass];
            s.List(nameof(SpellData), existing, existing.Count, SpellData.Serdes);
            return existing;
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as IList<SpellData>, config, mapping, s);
    }
}
