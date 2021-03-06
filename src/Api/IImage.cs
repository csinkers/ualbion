namespace UAlbion.Api
{
    public interface IImage
    {
        ITextureId Id { get; }
        string Name { get; }
        int Width { get; }
        int Height { get; }
        int SubImageCount { get; }
        int SizeInBytes { get; }
        ISubImage GetSubImage(int subImage);
    }
}