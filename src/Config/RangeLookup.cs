using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Config;

public class RangeLookup // Note: it is assumed that id ranges in a given config are disjoint, i.e. there should be no AssetId which satisfies multiple ranges in a single assets.json
{
    readonly AssetRangeInfo[][] _byType = new AssetRangeInfo[256][];
    readonly RangeLookup _parent;

    public RangeLookup() { } // Empty range for mods without an asset config
    public RangeLookup(RangeLookup parent, IEnumerable<AssetRangeInfo> ranges)
    {
        _parent = parent;

        var groups = ranges
            .GroupBy(x => x.Range.From.Type)
            .Select(x => (x.Key, x.OrderBy(x => x.Range.From).ToArray()));

        foreach (var (type, group) in groups)
            _byType[(int)type] = group;
    }

    public IEnumerable<AssetRangeInfo> AllRanges =>
        _byType
            .Where(x => x != null)
            .SelectMany(x => x)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Range.From.ToString());

    public AssetRangeInfo TryFindAssetRangeInfo(AssetId id)
    {
        var rangesForType = _byType[(int)id.Type];
        if (rangesForType == null)
            return _parent?.TryFindAssetRangeInfo(id);

        int index = FindNearest(rangesForType, id);
        if (index < 0 || index >= rangesForType.Length)
            return _parent?.TryFindAssetRangeInfo(id);

        var rangeInfo = rangesForType[index];
        var range = rangeInfo.Range;
        return range.From <= id && range.To >= id 
            ? rangeInfo 
            : _parent?.TryFindAssetRangeInfo(id);
    }

    static int FindNearest(AssetRangeInfo[] collection, AssetId id) // Binary search
    {
        int first = 0;
        int last = collection.Length - 1;
        int mid;

        do
        {
            mid = first + (last - first) / 2;
            if (id > collection[mid].Range.To)
                first = mid + 1;
            else
                last = mid - 1;

            if (collection[mid].Range.From <= id && collection[mid].Range.To >= id)
                return mid;
        } while (first <= last);

        if (collection[mid].Range.From > id && mid != 0)
            mid--;

        return mid;
    }
}