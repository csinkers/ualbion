namespace UAlbion.Formats.Assets.Maps;

public enum TileLayer
{
    Normal = 0,
    Layer1 = 1,
    Layer2 = 2,
    Layer3 = 3,
}

public static class TileLayerExtensions
{
    public static int ToDepthOffset(this TileLayer layer) => (int)layer;
}