namespace UAlbion.Api
{
    public enum DrawLayer : ushort
    {
        Background = 1, // Skybox in 3D
        Overlay    = 0x100,
        Character  = 0x101,
        Underlay   = 0x102,
        // Effects,
        Diagnostic  = 0x600, // Missing textures etc
        Interface   = 0x700, // GUI

        StatusBar = 0xf00,

        Debug = 0xff0, // ImGui
        Cursor = 0xffe, // Mouse cursor
        MaxLayer = 0xfff // Mouse cursor hotspot
    }
}
