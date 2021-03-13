namespace UAlbion.Api.Visual
{
    public interface IImage
    {
        ITextureId Id { get; }
        string Name { get; }
        int Width { get; }
        int Height { get; }
        int SizeInBytes { get; }
        ISubImage GetSubImage(int subImage);
        int SubImageCount { get; }
    }
}