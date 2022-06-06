using System;

namespace UAlbion.Api.Visual;

public static class DepthUtil
{
    public const int LayerCount = 4096;
    public const float MaxLayer = LayerCount - 1;
    public static float GetAbsDepth(float yCoordinateInTiles) => 1.0f - (float)Math.Ceiling(yCoordinateInTiles) / MaxLayer;
    public static float GetRelDepth(int tiles) => -tiles / MaxLayer;
    public static int DepthToLayer(float depth) => (int)((1.0f - depth) * MaxLayer);
}