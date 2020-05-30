namespace UAlbion.Formats.AssetIds
{
    public struct StringId
    {
        public StringId(AssetType type, ushort id, int subId) { Type = type; Id = id; SubId = subId; }
        public override string ToString() => $"String:{Type}:{Id}:{SubId}";
        public AssetType Type { get; }
        public ushort Id { get; }
        public int SubId { get; }

        public static implicit operator StringId(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);
        public static implicit operator StringId(UAlbionStringId id) => new StringId(AssetType.UAlbionText, (ushort)id, 0); 
        public static implicit operator StringId(WordId id) => new StringId(AssetType.Dictionary, (ushort)((int)id / 500), (int)id); 
        public static implicit operator StringId(ItemId id) => new StringId(AssetType.ItemNames, (ushort)id, 0);
    }
}
