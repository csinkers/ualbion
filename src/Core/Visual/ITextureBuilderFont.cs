using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public interface ITextureBuilderFont
{
    ReadOnlyImageBuffer<byte> GetRegion(char c);
    bool IsTransparent(byte pixel);
}