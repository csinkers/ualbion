namespace UAlbion.Game.Assets
{
    public interface IAssetLocatorRegistry
    {
        IAssetLocator GetLocator(string locatorName);
    }
}