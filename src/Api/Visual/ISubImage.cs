namespace UAlbion.Api.Visual
{
    public interface ISubImage
    {
        int X { get; }
        int Y { get; }
        int Width { get; }
        int Height { get; }
        public int PixelOffset { get; }
        public int PixelLength { get; }
    }
}