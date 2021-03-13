namespace UAlbion.Api.Visual
{
    public class SpriteSheetLayout
    {
        public SpriteSheetLayout(int width, int height, (int, int)[] positions)
        {
            Width = width;
            Height = height;
            Positions = positions;
        }

        public int Width { get; }
        public int Height { get; }
        public (int, int)[] Positions { get; }
    }
}