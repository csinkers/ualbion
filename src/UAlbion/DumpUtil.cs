using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;

namespace UAlbion
{
    public static class DumpUtil
    {
        public static IEnumerable<AssetId> All(AssetType type) => AssetId.EnumerateAll(type);
        public static IDictionary<AssetId, TValue> AllAssets<TValue>(AssetType type, Func<AssetId, TValue> fetcher)
            => All(type)
                .Select(x => (x, fetcher(x)))
                .Where(x => x.Item2 != null)
                .ToDictionary(x => x.x, x => x.Item2);
    }
}