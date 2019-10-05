namespace UAlbion.Formats.AssetIds
{
    public struct StringId
    {
        public StringId(AssetType type, int id, int subId) { Type = type; Id = id; SubId = subId; }

        public AssetType Type { get; }
        public int Id { get; }
        public int SubId { get; }
    }
}