using System.Numerics;

namespace UAlbion.Core.Textures
{
    public class SubImage
    {
        public SubImage(Vector2 offset, Vector2 size, Vector2 totalSize, uint layer)
        {
            Offset = offset;
            Size = size;
            TexOffset = offset / totalSize;
            TexSize = size / totalSize;
            Layer = layer;
        }

        public override string ToString() => $"({Size} {TexOffset} {TexSize} L{Layer})";

        public Vector2 Offset { get; }
        public Vector2 Size { get; }
        public Vector2 TexOffset { get; }
        public Vector2 TexSize { get; }
        public uint Layer { get; }
    }
}
