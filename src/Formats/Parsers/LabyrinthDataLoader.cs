using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader<LabyrinthData>
    {
        public LabyrinthData Serdes(LabyrinthData existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return LabyrinthData.Serdes(config.Id, null, mapping, s);
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as LabyrinthData, config, mapping, s);
    }
}
