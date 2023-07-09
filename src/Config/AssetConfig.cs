using System;
using System.Collections.Generic;

namespace UAlbion.Config;

/// <summary>
/// A JSON configuration file containing all details of how to load and save assets for a given mod.
/// </summary>
public class AssetConfig : IAssetConfig
{
    public AssetConfig(string modName, RangeLookup ranges)
    {
        ModName = modName;
        Ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));
    }

    public string ModName { get; } // The mod name

    /// <summary>
    /// The collection of asset ranges that are provided by the mod.
    /// </summary>
    public RangeLookup Ranges { get; }

    public IEnumerable<AssetNode> GetAssetInfo(AssetId id)
    {
        var range = Ranges.TryFindAssetRangeInfo(id);
        if (range == null)
            yield break;

        if (range.Files == null || range.Files.Count == 0)
        {
            yield return range.Node;
        }
        else
        {
            foreach (var file in range.Files)
            {
                if (file.Map != null && file.Map.TryGetValue(id, out var assetInfo))
                    yield return assetInfo.Node;
                else
                    yield return file.Node;
            }
        }
    }
}