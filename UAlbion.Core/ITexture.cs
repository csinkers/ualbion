using Veldrid;

namespace UAlbion.Core
{
    public interface ITexture
    {
        PixelFormat Format { get; }
        TextureType Type { get; }
        uint Width { get; }
        uint Height { get; }
        uint Depth { get; }
        uint MipLevels { get; }
        uint ArrayLayers { get; }
        string Name { get; }
        Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage);
    }
}