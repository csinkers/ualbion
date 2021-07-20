using UAlbion.Core.Veldrid.Textures;
using VeldridGen.Interfaces;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public partial class SimpleFramebuffer : FramebufferHolder
    {
        [DepthAttachment(PixelFormat.R32_Float)] public Texture2DHolder Depth { get; }
        [ColorAttachment(PixelFormat.B8_G8_R8_A8_UNorm)] public Texture2DHolder Color { get; }
    }
}

