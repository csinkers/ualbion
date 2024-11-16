using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers;

public class LabyrinthDataLoader : IAssetLoader<LabyrinthData>
{
    public LabyrinthData Serdes(LabyrinthData existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return LabyrinthData.Serdes(existing, context.AssetId, context.Mapping, s);
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as LabyrinthData, s, context);
}
