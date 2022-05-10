namespace UAlbion.Formats.Assets.Maps;

public record TileFrameSummary(string[] Paths)
{
    public int RegionOffset { get; set; }
    public override string ToString() =>
        Paths.Length > 1
        ? $"TileOffset {RegionOffset} {Paths.Length} PalFrames"
        : $"TileOffset {RegionOffset}";
}