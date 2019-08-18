namespace UAlbion.Game
{
    public enum DrawLayer
    {
        Background = 0, // Skybox in 3D
        Underlay = 0,
        Characters1,
        Overlay1,
        Characters2,
        Overlay2,
        Overlay3,
        // Items,
        // Effects,
        Diagnostic, // Missing textures etc
        Interface, // GUI
        Debug, // ImGui
    }
}