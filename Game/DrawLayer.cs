using UAlbion.Api;

namespace UAlbion.Game
{
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
