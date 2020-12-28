using System.Runtime.InteropServices;

namespace UAlbion.Game.Veldrid.Visual
{
    [StructLayout(LayoutKind.Sequential)]
    struct InfoOverlayUniforms // Length must be multiple of 16
    {
        public float Examine;  // Opacities, in the range [0..1]
        public float Manipulate; 
        public float Talk; 
        public float Take; 

        public float Width;
        public float Height;
        public float TileWidth;
        public float TileHeight;
    }
}