using System.Runtime.InteropServices;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox
{
#pragma warning disable 649
    [StructLayout(LayoutKind.Sequential)]
    struct SkyboxUniformInfo // Length must be multiple of 16
    {
        [Uniform("uYaw")] public float uYaw; // 4
        [Uniform("uPitch")] public float uPitch;  // 8
        [Uniform("uVisibleProportion")] public float uVisibleProportion;  // 12
        [Uniform("_pad1")] readonly uint _pad1;   // 16
    }
#pragma warning restore 649
}