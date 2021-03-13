using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures
{
    public interface ITexture : IImage
    {
        int Depth { get; }
        int MipLevels { get; }
        int ArrayLayers { get; }
        bool IsDirty { get; }
        PixelFormat Format { get; }
        void Invalidate();
    }
}
