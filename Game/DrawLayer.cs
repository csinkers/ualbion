namespace UAlbion.Game
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
        Interface   = 0x301, // GUI
        Debug, // ImGui

        MaxLayer    = 0xfff
    }

    public static class DrawLayerExtensions
    {
        public static float ToZCoordinate(this DrawLayer layer, float yCoordinateInTiles)
        {
            float adjusted = (int) layer + (255.0f - yCoordinateInTiles);
            float normalised = 1.0f - adjusted / 4095.0f;
            return normalised;
        }
    }
}
