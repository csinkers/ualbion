using System;

namespace UAlbion.Api
{
    public static class DepthUtil
    {
        const int YMultiplier = 3;
        const int CharacterOffset1 = 1;
        const int CharacterOffset2 = 3;

        public static float LayerToDepth(int layer, float yCoordinateInTiles)
        {
            int adjusted = layer + YMultiplier * (int)Math.Ceiling(yCoordinateInTiles);
            return 1.0f - adjusted / 4095.0f;
        }

        public static float IndoorCharacterDepth(float yCoordinateInTiles)
            => LayerToDepth(CharacterOffset1, yCoordinateInTiles);
        public static float OutdoorCharacterDepth(float yCoordinateInTiles)
            => LayerToDepth(CharacterOffset2, yCoordinateInTiles);

        public static int DepthToLayer(float depth) => (int)((1.0f - depth) * 4095.0f);
    }
}