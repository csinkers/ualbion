namespace UAlbion.Api.Visual;

public class SpriteSheetLayout
{
    public SpriteSheetLayout(int width, int height, int layers, (int, int, int)[] positions)
    {
        Width = width;
        Height = height;
        Layers = layers;
        Positions = positions;
    }

    public int Width { get; }
    public int Height { get; }
    public int Layers { get; }
    public (int X, int Y, int Layer)[] Positions { get; }
}