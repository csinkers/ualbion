using System;

namespace UAlbion.Formats.Exporters.Tiled
{
    readonly struct ChainHint : IEquatable<ChainHint>, IComparable<ChainHint>
    {
        public static ChainHint None => new(ushort.MaxValue, 0);
        public ChainHint(ushort chainId, ushort dummyNumber)
        {
            RawChainId = chainId;
            DummyNumber = dummyNumber;
        }

        public ushort RawChainId { get; }
        public ushort DummyNumber { get; }
        public ushort ChainId => DummyNumber == 0 ? RawChainId : ushort.MaxValue;
        public bool IsChain => RawChainId != ushort.MaxValue && DummyNumber == 0;
        public bool IsDummy => RawChainId != ushort.MaxValue && DummyNumber != 0;
        public bool IsNone => RawChainId == ushort.MaxValue && DummyNumber == 0;

        public bool Equals(ChainHint other) => RawChainId == other.RawChainId && DummyNumber == other.DummyNumber;
        public override bool Equals(object obj) => obj is ChainHint other && Equals(other);
        public override int GetHashCode() => RawChainId << 16 | DummyNumber;
        public override string ToString() => $"{ChainId}.{DummyNumber}";

        public static bool operator ==(ChainHint x, ChainHint y) => x.Equals(y);
        public static bool operator !=(ChainHint x, ChainHint y) => !(x == y);
        public static bool operator <(ChainHint x, ChainHint y) => x.CompareTo(y) == -1;
        public static bool operator >(ChainHint x, ChainHint y) => x.CompareTo(y) == 1;
        public static bool operator <=(ChainHint x, ChainHint y) => x.CompareTo(y) != 1;
        public static bool operator >=(ChainHint x, ChainHint y) => x.CompareTo(y) != -1;
        public int CompareTo(ChainHint other)
        {
            var rawChainIdComparison = RawChainId.CompareTo(other.RawChainId);
            if (rawChainIdComparison != 0) return rawChainIdComparison;
            return DummyNumber.CompareTo(other.DummyNumber);
        }
    }
}