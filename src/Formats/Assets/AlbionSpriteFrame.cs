using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets
{
    public class AlbionSpriteFrame : ISubImage
    {
        public AlbionSpriteFrame(int x, int y, int width, int height, int stride)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            PixelOffset = X + stride * Y;
            PixelLength = Width + (Height - 1) * stride;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public int PixelOffset { get; }
        public int PixelLength { get; }

        public override string ToString() => $"({X},{Y}) {Width}x{Height}";
    }
}
