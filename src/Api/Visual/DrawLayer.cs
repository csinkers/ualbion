namespace UAlbion.Api.Visual;

public enum DrawLayer : ushort
{
    Background = 1, // Skybox in 3D
    OpaqueTerrain = 64, // Opaque 3D level geometry
    Billboards = 65, // Sprites in 3D levels
    TranslucentTerrain = 66, // Windows etc in 3D levels

    Underlay   = 0x100, // for 2D levels
    Overlay    = 0x101, // for 2D levels
    Character  = 0x102, // for 2D levels
    // The above will get increased by up to 256, so everything up to 0x202 should be blocked out.

    Info       = 0x300, // Verb highlights on map etc   (for 2D levels)
    Interface   = 0x301, // GUI
    InterfaceOverlay = 0xf00, // GUI effects, like item transitions, discards etc

    Debug = 0xff0, // ImGui
    Cursor = 0xffd, // Mouse cursor
    Compositing = 0xffe, // Composing offscreen framebuffers etc
    MaxLayer = 0xfff // Mouse cursor hotspot
}