using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public interface IVeldridTexture : ITexture
    {
        TextureType Type { get; }
        Texture CreateDeviceTexture(GraphicsDevice gd, TextureUsage usage);
    }
}
