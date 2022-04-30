using System;

namespace UAlbion.Api.Visual;

public static class DepthUtil
{
    public static float LayerToDepth(int layer, float yCoordinateInTiles) => 1.0f - (layer + (int)Math.Ceiling(yCoordinateInTiles)) / 4095.0f;
    public static int DepthToLayer(float depth) => (int)((1.0f - depth) * 4095.0f);
}