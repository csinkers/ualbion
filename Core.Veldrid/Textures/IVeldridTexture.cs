using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public interface IVeldridTexture : ITexture
    {
        PixelFormat Format { get; }
        TextureType Type { get; }
        Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage);
    }
}
