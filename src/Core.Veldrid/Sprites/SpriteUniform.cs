using System.Runtime.InteropServices;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

[StructLayout(LayoutKind.Sequential)]
internal struct SpriteUniform  : IUniformFormat // Length must be multiple of 16
{
    [Uniform("uFlags", EnumPrefix = "SKF")] public SpriteKeyFlags Flags { get; set; } // 4 bytes
    [Uniform("uTexSizeW")] public float TextureWidth { get; set; } // 4 bytes
    [Uniform("uTexSizeH")] public float TextureHeight { get; set; } // 4 bytes
    [Uniform("_pad1")] uint Padding { get; set; } // 4 bytes
}