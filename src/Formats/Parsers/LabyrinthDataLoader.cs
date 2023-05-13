using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers;

public class LabyrinthDataLoader : IAssetLoader<LabyrinthData>
{
    public LabyrinthData Serdes(LabyrinthData existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        return LabyrinthData.Serdes(existing, info, context.Mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as LabyrinthData, info, s, context);
}
