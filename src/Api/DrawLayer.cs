namespace UAlbion.Api
{
    public enum DrawLayer : ushort
    {
        Background = 1, // Skybox in 3D
        Overlay    = 0x100, // + (mapHeight - tilePosY - 1)
        Character  = 0x101, // + (mapHeight - tilePosY - 1)
        Underlay   = 0x102, // + (mapHeight - tilePosY - 1)
        Info       = 0x300, // Verb highlights on map etc
        // Effects,
        Diagnostic  = 0x600, // Missing textures etc
        Interface   = 0x700, // GUI
        InterfaceOverlay = 0x800,

        Debug = 0xff0, // ImGui
        Cursor = 0xffe, // Mouse cursor
        MaxLayer = 0xfff // Mouse cursor hotspot
    }
}
