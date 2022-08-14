using System.Numerics;

namespace UAlbion.BinOffsetFinder;

public readonly struct Vertex2DTextured
{
    public Vector2 Position { get; }
    public Vector2 Texture { get; }

    public Vertex2DTextured(float x, float y, float u, float v)
    {
        Position = new Vector2(x, y);
        Texture = new Vector2(u, v);
    }
}