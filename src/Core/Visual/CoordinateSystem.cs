namespace UAlbion.Core.Visual;

public enum CoordinateSystem
{
    Ui2D, // UI coordinates
    World2D, // World coordinates in pixel units
    Camera2D, // Camera-relative coordinates
    Screen2D, // Screen coordinates in pixel units
    Norm2D, // Normalized device coordinates

    Tile3D, // World coordinates scaled to the tile grid
    World3D, // World coordinates in standard units
    Camera3D, // Camera-relative coordinates
    Norm3D, // Normalized device coordinates
}