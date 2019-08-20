namespace UAlbion.Game
{
    public enum DrawLayer
    {
        Background = 0, // Skybox in 3D
        Underlay = 0,
        Overlay1,
        Characters1,
        Overlay2,
        Characters2,
        Overlay3,
        // Items,
        // Effects,
        Diagnostic, // Missing textures etc
        Interface, // GUI
        Debug, // ImGui
    }
}