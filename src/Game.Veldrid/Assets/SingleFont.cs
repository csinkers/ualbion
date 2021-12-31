using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Veldrid.Assets;

public class SingleFont : ITextureBuilderFont
{
    readonly MultiFont _font;
    readonly int _size;

    internal SingleFont(MultiFont font, int size)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _size = size;
    }

    public ReadOnlyImageBuffer<byte> GetRegion(char c) => _font.GetRegion(_size, c);
    public bool IsTransparent(byte pixel) => pixel == 0;
}