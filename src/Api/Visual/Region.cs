using System.Numerics;

namespace UAlbion.Api.Visual;

public class Region
{
    public Region(int x, int y, int w, int h, int totalWidth, int totalHeight, int layer)
        : this(
            new Vector2(x, y),
            new Vector2(w, h),
            new Vector2(totalWidth, totalHeight),
            layer)
    {
    }

    public Region(Vector2 offset, Vector2 size, Vector2 totalSize, int layer)
    {
        Offset = offset;
        Size = size;
        TexOffset = offset / totalSize;
        TexSize = size / totalSize;
        Layer = layer;

        int stride = (int)totalSize.X;
        int layerSize = stride * (int)totalSize.Y;
        PixelOffset = layer * layerSize + Y * stride + X;
        PixelLength = Width + (Height - 1) * stride;
    }

    public override string ToString() => $"({Offset} {Size} {TexOffset} {TexSize} L{Layer})";

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
    public int Layer { get; }

    public int X => (int)Offset.X;
    public int Y => (int)Offset.Y;
    public int Width => (int)Size.X;
    public int Height => (int)Size.Y;

    /// <summary>
    /// The offset into the raw pixel buffer where the subimage begins
    /// </summary>
    public int PixelOffset { get; }

    /// <summary>
    /// The number of pixels in the raw pixel buffer occupied by the subimage.
    /// </summary>
    public int PixelLength { get; }
}