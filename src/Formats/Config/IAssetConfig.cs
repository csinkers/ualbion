namespace UAlbion.Formats.Config
{
    public interface IAssetConfig
    {
        AssetInfo GetAsset(string xldName, int xldSubObject, int id);
    }
}
