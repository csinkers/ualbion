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
        Debug = 0x500, // ImGui

        MaxLayer    = 0xfff // Mouse cursor
    }
}
