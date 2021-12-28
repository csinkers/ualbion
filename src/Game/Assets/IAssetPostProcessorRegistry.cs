namespace UAlbion.Game.Assets;

public interface IAssetPostProcessorRegistry
{
    IAssetPostProcessor GetPostProcessor(string postProcessorName);
}