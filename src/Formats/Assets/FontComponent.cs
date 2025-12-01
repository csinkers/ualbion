using System;
using UAlbion.Api.Visual;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class FontComponent
{
    public SpriteId GraphicsId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int XAdvance { get; set; } // 0 = use Height and YAdvance instead
    public int YAdvance { get; set; }
    public string Mapping { get; set; }

    public Region TryGetRegion(char c, ITexture texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        int index = Mapping.IndexOf(c, StringComparison.Ordinal);
        if (index == -1)
            return null;

        /* This needs to work with native layouts and sprite-sheet layouts
        e.g. native:
            a
            b
            c
            d
            e
            ...

        spritesheet:
            a b c d e
            f g h i j
            ...
        */

        int x = X + XAdvance * index;
        int y = Y;
        if (XAdvance == 0)
            y += YAdvance * index;

        while (x >= texture.Width)
        {
            x -= texture.Width;
            y += YAdvance;
        }

        if (x + Width > texture.Width || y + Height > texture.Height)
        {
            throw new ArgumentOutOfRangeException(
                $"Char \'{c}\' was out of bonds: would be placed at ({x}, {y}) with " +
                $"size ({Width},{Height}) but the texture ({texture.Id}) had size ({texture.Width}, {texture.Height})");
        }

        return new Region(x, y, Width, Height, texture.Width, texture.Height, 0);
    }
}