namespace UAlbion.Config
{
    public interface IAssetConfig
    {
        AssetInfo[] GetAssetInfo(AssetId id);
    }
}
