namespace UAlbion.Config
{
    public interface IAssetConfig
    {
        AssetInfo GetAssetInfo(string typeName, int id);
    }
}
