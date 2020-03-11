using System.Numerics;

namespace UAlbion.Core
{
    public struct Vertex2DTextured
    {
        public float X;
        public float Y;
        public float U;
        public float V;

        public Vertex2DTextured(Vector2 position, Vector2 textureCoordinates)
        {
            X = position.X;
            Y = position.Y;
            U = textureCoordinates.X;
            V = textureCoordinates.Y;
        }

        public Vertex2DTextured(float x, float y, float u, float v) { X = x; Y = y; U = u; V = v; }
    }
}
