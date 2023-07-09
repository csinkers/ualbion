using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers;

public class LabyrinthDataLoader : IAssetLoader<LabyrinthData>
{
    public LabyrinthData Serdes(LabyrinthData existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return LabyrinthData.Serdes(existing, context.AssetId, context.Mapping, s);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as LabyrinthData, s, context);
}
