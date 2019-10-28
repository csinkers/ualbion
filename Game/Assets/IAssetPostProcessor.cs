namespace UAlbion.Game.Assets
{
    public interface IAssetPostProcessor
    {
        object Process(string name, object asset);
    }
}