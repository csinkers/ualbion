namespace UAlbion.Formats.AssetIds
{
    public struct StringId
    {
        public StringId(AssetType type, ushort id, int subId) { Type = type; Id = id; SubId = subId; }
        public override string ToString() => $"String:{Type}:{Id}:{SubId}";
        public AssetType Type { get; }
        public ushort Id { get; }
        public int SubId { get; }
    }
}
