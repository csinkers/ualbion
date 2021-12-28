using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers;

public class LabyrinthDataLoader : IAssetLoader<LabyrinthData>
{
    public LabyrinthData Serdes(LabyrinthData existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return LabyrinthData.Serdes(existing, info, mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes(existing as LabyrinthData, info, mapping, s, jsonUtil);
}