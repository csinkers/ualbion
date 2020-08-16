namespace UAlbion.Formats.AssetIds
{
    public struct StringId : System.IEquatable<StringId>
    {
        public StringId(AssetType type, ushort id, int subId) { Type = type; Id = id; SubId = subId; }
        public override string ToString() => $"String:{Type}:{Id}:{SubId}";
        public AssetType Type { get; }
        public ushort Id { get; }
        public int SubId { get; }

        public static implicit operator StringId(SystemTextId id)    => ToStringId(id);
        public static implicit operator StringId(UAlbionStringId id) => ToStringId(id);
        public static implicit operator StringId(WordId id)          => ToStringId(id);
        public static implicit operator StringId(ItemId id)          => ToStringId(id);
        public static StringId ToStringId(SystemTextId id)    => new StringId(AssetType.SystemText, 0, (int)id);
        public static StringId ToStringId(UAlbionStringId id) => new StringId(AssetType.UAlbionText, (ushort)id, 0);
        public static StringId ToStringId(WordId id)          => new StringId(AssetType.Dictionary, (ushort)((int)id / 500), (int)id);
        public static StringId ToStringId(ItemId id)          => new StringId(AssetType.ItemNames, (ushort)id, 0);

        public override bool Equals(object obj) => obj is StringId other && Equals(other);
        public bool Equals(StringId other) => Type == other.Type && Id == other.Id && SubId == other.SubId;
        public static bool operator ==(StringId left, StringId right) => left.Equals(right);
        public static bool operator !=(StringId left, StringId right) => !(left == right);
        public override int GetHashCode() => 17 * (17 * (int)Type ^ Id) ^ SubId;

    }
}
