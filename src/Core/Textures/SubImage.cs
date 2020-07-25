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

        /// <summary>
        /// The offset of the upper-left corner of the sub-image in the entire image (in pixels)
        /// </summary>
        public Vector2 Offset { get; }

        /// <summary>
        /// The size of the sub-image (in pixels)
        /// </summary>
        public Vector2 Size { get; }

        /// <summary>
        /// The offset in normalised texture coordinates
        /// </summary>
        public Vector2 TexOffset { get; }

        /// <summary>
        /// The size in normalised texture coordinates
        /// </summary>
        public Vector2 TexSize { get; }

        /// <summary>
        /// The layer of the entire image containing the sub-image
        /// </summary>
        public uint Layer { get; }
    }
}
