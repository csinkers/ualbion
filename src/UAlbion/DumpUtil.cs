using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;

namespace UAlbion;

public static class DumpUtil
{
    public static IEnumerable<AssetId> All(AssetType type, AssetId[] dumpIds) 
        => AssetId.EnumerateAll(type).Where(x => dumpIds == null || dumpIds.Contains(x));

    public static IDictionary<AssetId, TValue> AllAssets<TValue>(AssetType type, AssetId[] dumpIds, Func<AssetId, TValue> fetcher)
        => All(type, dumpIds)
            .Select(x => (x, fetcher(x)))
            .Where(x => x.Item2 != null)
            .ToDictionary(x => x.x, x => x.Item2);
}