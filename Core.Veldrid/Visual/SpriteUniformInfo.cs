using System.Runtime.InteropServices;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Visual
{
    [StructLayout(LayoutKind.Sequential)]
    struct SpriteUniformInfo // Length must be multiple of 16
    {
        public SpriteKeyFlags Flags { get; set; } // 1 byte
        readonly byte _pad1;   // 2
        readonly ushort _pad2; // 4
        public float TextureWidth { get; set; } // 4 bytes
        public float TextureHeight { get; set; } // 4 bytes
        readonly uint _pad4; // total of 16
    }
}