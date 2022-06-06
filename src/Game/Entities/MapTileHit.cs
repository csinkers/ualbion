using System.Numerics;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities;

public class MapTileHit
{
    public override string ToString() => "MapTile";
    public Vector2 Tile { get; set; }
}

public class DebugMapTileHit
{
    public override string ToString() => "DebugTileInfo";
    public Vector2 Tile { get; set; }
    public Vector3 IntersectionPoint { get; set; }
    public TileData UnderlayTile { get; set; }
    public TileData OverlayTile { get; set; }
}