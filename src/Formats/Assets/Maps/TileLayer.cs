using UAlbion.Api.Visual;

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
    public static int ToDepthOffset(this TileLayer layer)
    {
        int adjustment;
        switch ((int)layer & 0x7)
        {
            case (int)TileLayer.Normal: adjustment = DepthUtil.NormalAdjustment; break;
            case (int)TileLayer.Layer1: adjustment = DepthUtil.Layer1Adjustment; break;
            case (int)TileLayer.Layer2: adjustment = DepthUtil.Layer2Adjustment; break;
            case (int)TileLayer.Layer3: adjustment = DepthUtil.Layer3Adjustment; break;
            default: adjustment = 0; break;
        }
        return adjustment;
    }
}