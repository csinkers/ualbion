using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public struct StringId : System.IEquatable<StringId>
    {
        public StringId(TextId id, ushort subId) { Id = id; SubId = subId; }
        public override string ToString() => $"S:{Id}:{SubId}";
        public AssetId Id { get; }
        public ushort SubId { get; }

        public static implicit operator StringId(TextId id) => ToStringId(id);
        public static StringId ToStringId(TextId id) => FormatUtil.ResolveTextId(id);

        public override bool Equals(object obj) => obj is StringId other && Equals(other);
        public bool Equals(StringId other) => Id == other.Id && SubId == other.SubId;
        public static bool operator ==(StringId left, StringId right) => left.Equals(right);
        public static bool operator !=(StringId left, StringId right) => !(left == right);
        public override int GetHashCode() => 17 * Id.ToInt32() ^ SubId;
    }
}
