using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public class TilemapRequest
{
    public IAssetId Id { get; init; }
    public int TileCount { get; set; }
    public IPalette DayPalette { get; set; }
    public IPalette NightPalette { get; set; }
    public DungeonTilemapPipeline Pipeline { get; set; }
    public Vector3 Scale { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Origin { get; set; }
    public Vector3 HorizontalSpacing { get; set; }
    public Vector3 VerticalSpacing { get; set; }
    public uint Width { get; set; }
    public uint AmbientLightLevel { get; set; }
    public uint FogColor { get; set; }
    public float ObjectYScaling { get; set; } = 1.0f;
}