namespace UAlbion.Api.Visual
{
    public interface IImage
    {
        IAssetId Id { get; }
        string Name { get; }
        int Width { get; }
        int Height { get; }
        int SizeInBytes { get; }
        ISubImage GetSubImage(int subImage);
        int SubImageCount { get; }
    }
}