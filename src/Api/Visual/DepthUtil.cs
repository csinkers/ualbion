using System;

namespace UAlbion.Api.Visual
{
    public static class DepthUtil
    {
        const int YMultiplier = 3;
        const int CharacterOffset1 = 1;
        const int CharacterOffset2 = 3;
        public const int NormalAdjustment = 0;
        public const int Layer1Adjustment = 1;
        public const int Layer2Adjustment = 2;
        public const int Layer3Adjustment = 8;
        public const int TypeNormalAdjustment = 0;
        public const int TypeOverlay1Adjustment = 1;
        public const int TypeOverlay2Adjustment = 2;
        public const int TypeOverlay3Adjustment = 3;

        /*
        const int YMultiplier = 1;
        const int CharacterOffset1 = 1;
        const int CharacterOffset2 = 1;
        public const int NormalAdjustment = 0;
        public const int Layer1Adjustment = 1;
        public const int Layer2Adjustment = 1;
        public const int Layer3Adjustment = 2;
        public const int TypeNormalAdjustment = 0;
        public const int TypeOverlay1Adjustment = 0;
        public const int TypeOverlay2Adjustment = 0;
        public const int TypeOverlay3Adjustment = 2;
        */

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