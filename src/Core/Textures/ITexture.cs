using UAlbion.Api;

namespace UAlbion.Core.Textures
{
    public interface ITexture
    {
        ITextureId Id { get; }
        string Name { get; }
        uint Width { get; }
        uint Height { get; }
        uint Depth { get; }
        uint MipLevels { get; }
        uint ArrayLayers { get; }
        bool IsDirty { get; }
        int SubImageCount { get; }
        int SizeInBytes { get; }
        PixelFormat Format { get; }
        SubImage GetSubImageDetails(int subImage);
        void Invalidate();
    }
}
