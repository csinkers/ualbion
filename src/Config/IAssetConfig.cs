namespace UAlbion.Config
{
    public interface IAssetConfig
    {
        AssetInfo GetAsset(string typeName, int id);
        // AssetInfo GetAsset(AssetId id);
        // AssetInfo GetAsset(string xldName, int xldSubObject, int id);
    }
}
