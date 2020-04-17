namespace UAlbion.Formats.AssetIds
{
    public static class StringIdExtensions
    {
        public static StringId ToId(this SystemTextId systemTextId) 
            => new StringId(AssetType.SystemText, 0, (int)systemTextId);

        public static StringId ToId(this UAlbionStringId id) 
            => new StringId(AssetType.UAlbionText, (int)id, 0);
    }
}
