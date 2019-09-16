using System.Numerics;
using Veldrid;

namespace UAlbion.Core.Textures
{
    public interface ITexture
    {
        string Name { get; }
        PixelFormat Format { get; }
        TextureType Type { get; }
        uint Width { get; }
        uint Height { get; }
        uint Depth { get; }
        uint MipLevels { get; }
        uint ArrayLayers { get; }
        bool IsDirty { get; }
        int SubImageCount { get; }
        void GetSubImageDetails(int subImage, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer);
        Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage);
    }
}