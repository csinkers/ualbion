using System.Numerics;

namespace UAlbion.Game.Entities
{
    public class MapTileHit
    {
        public MapTileHit(Vector2 tile, Vector3 intersectionPoint)
        {
            Tile = tile;
            IntersectionPoint = intersectionPoint;
        }

        public override string ToString() => $"MapTile (Tile: {Tile}) (World: {IntersectionPoint})";
        public Vector2 Tile { get; }
        public Vector3 IntersectionPoint { get; }
    }
}