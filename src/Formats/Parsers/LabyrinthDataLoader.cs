using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers;

public class LabyrinthDataLoader : IAssetLoader<LabyrinthData>
{
    public LabyrinthData Serdes(LabyrinthData existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return LabyrinthData.Serdes(existing, info, context.Mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as LabyrinthData, info, s, context);
}
