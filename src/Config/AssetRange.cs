using System.Collections;
using System.Collections.Generic;

namespace UAlbion.Config;

public readonly record struct AssetRange(AssetId From, AssetId To) : IEnumerable<AssetId>
{
    public override string ToString() => $"{From}-{To} [{From.Id}-{To.Id}]";
    public IEnumerator<AssetId> GetEnumerator() => new RangeEnumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    sealed class RangeEnumerator : IEnumerator<AssetId>
    {
        readonly AssetRange _range;
        public RangeEnumerator(AssetRange range) => _range = range;

        public void Reset() => Current = AssetId.None;
        public AssetId Current { get; private set; }
        object IEnumerator.Current => Current;
        public void Dispose() { }

        public bool MoveNext()
        {
            if (Current >= _range.To)
                return false;

            if (Current.IsNone)
            {
                Current = _range.From;
                return true;
            }

            Current = new AssetId(Current.Type, Current.Id + 1);
            return true;
        }
    }
};