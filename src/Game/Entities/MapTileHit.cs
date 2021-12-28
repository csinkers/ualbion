using System.Numerics;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities;

public class MapTileHit
{
    public override string ToString() => "MapTile";
    public Vector2 Tile { get; set; }
    public Vector3 IntersectionPoint { get; set; }
    public WeakSpriteReference UnderlaySprite { get; set; }
    public WeakSpriteReference OverlaySprite { get; set; }
}