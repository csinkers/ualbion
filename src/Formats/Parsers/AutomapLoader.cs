﻿using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class AutomapLoader : IAssetLoader<Automap>
    {
        public Automap Serdes(Automap existing, AssetInfo info, AssetMapping mapping, ISerializer s) => Automap.Serdes(existing, s);
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((Automap)existing, info, mapping, s);
    }
}
