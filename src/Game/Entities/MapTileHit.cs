using System.Numerics;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class MapTileHit
    {
        public MapTileHit(Vector2 tile, Vector3 intersectionPoint, WeakSpriteReference underlaySprite, WeakSpriteReference overlaySprite)
        {
            Tile = tile;
            IntersectionPoint = intersectionPoint;
            UnderlaySprite = underlaySprite;
            OverlaySprite = overlaySprite;
        }

        public override string ToString() => "MapTile";
        public Vector2 Tile { get; }
        public Vector3 IntersectionPoint { get; }
        public WeakSpriteReference UnderlaySprite { get; }
        public WeakSpriteReference OverlaySprite { get; }
    }
}
