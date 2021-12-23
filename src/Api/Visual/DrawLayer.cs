namespace UAlbion.Api.Visual
{
    public enum DrawLayer : ushort
    {
        Background = 1, // Skybox in 3D
        OpaqueTerrain = 64, // Opaque 3D level geometry
        Billboards = 65, // Sprites in 3D levels
        TranslucentTerrain = 66, // Windows etc in 3D levels

        Overlay    = 0x100, // + (mapHeight - tilePosY - 1) (for 2D levels)
        Character  = 0x101, // + (mapHeight - tilePosY - 1) (for 2D levels)
        Underlay   = 0x102, // + (mapHeight - tilePosY - 1) (for 2D levels)
        Info       = 0x300, // Verb highlights on map etc   (for 2D levels)
        // Effects,
        Diagnostic  = 0x600, // Missing textures etc
        Interface   = 0x700, // GUI
        InterfaceOverlay = 0x800,

        Debug = 0xff0, // ImGui
        Cursor = 0xffd, // Mouse cursor
        Compositing = 0xffe, // Composing offscreen framebuffers etc
        MaxLayer = 0xfff // Mouse cursor hotspot
    }
}
