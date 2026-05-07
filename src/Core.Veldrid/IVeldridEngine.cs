using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IVeldridEngine : IEngine
{
    GraphicsDevice Device { get; }
    SixLabors.ImageSharp.Image<Bgra32> ReadTexture2D(ITextureHolder textureHolder);
    SixLabors.ImageSharp.Image<Bgra32> ReadTexture2D(Texture texture);
}