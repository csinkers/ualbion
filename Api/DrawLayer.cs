using System;

namespace UAlbion.Api
{
    public enum DrawLayer : ushort
    {
        Background  = 1, // Skybox in 3D
        Underlay    = 0x100,
        Overlay1    = 0x101, // + y coord
        Characters1 = 0x102, // + y coord
        Overlay2    = 0x103, // + y coord
        Characters2 = 0x104, // + y coord
        Overlay3    = 0x105, // + y coord
        // Effects,
        Diagnostic  = 0x300, // Missing textures etc
        Interface   = 0x400, // GUI

        StatusBar = 0xf00,

        Debug = 0xff0, // ImGui
        Cursor = 0xffe, // Mouse cursor
        MaxLayer = 0xfff // Mouse cursor hotspot
    }

    public static class DrawLayerExtensions
    {
        public static float ToZCoordinate(this DrawLayer layer, float yCoordinateInTiles)
        {
            int adjusted = (int)layer + (int)Math.Ceiling(yCoordinateInTiles); //(255.0f - yCoordinateInTiles);
            return 1.0f - adjusted / 4095.0f;
        }

        public static int ToDebugZCoordinate(this DrawLayer layer, float yCoordinateInTiles) => (int)layer + (int)Math.Ceiling(yCoordinateInTiles);
    }
}
