namespace UAlbion.Formats.Assets
{
    public class AlbionSpriteFrame
    {
        public AlbionSpriteFrame(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public override string ToString() => $"({X},{Y}) {Width}x{Height}";
    }
}
