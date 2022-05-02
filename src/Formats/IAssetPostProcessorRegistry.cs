namespace UAlbion.Formats;

public interface IAssetPostProcessorRegistry
{
    IAssetPostProcessor GetPostProcessor(string postProcessorName);
}