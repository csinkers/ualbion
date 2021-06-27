using System.Numerics;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class MapTileHit
    {
        public MapTileHit(Vector2 tile, Vector3 intersectionPoint, IWeakSpriteReference underlaySprite, IWeakSpriteReference overlaySprite)
        {
            Tile = tile;
            IntersectionPoint = intersectionPoint;
            UnderlaySprite = underlaySprite;
            OverlaySprite = overlaySprite;
        }

        public override string ToString() => "MapTile";
        public Vector2 Tile { get; }
        public Vector3 IntersectionPoint { get; }
        public IWeakSpriteReference UnderlaySprite { get; }
        public IWeakSpriteReference OverlaySprite { get; }
    }
}
