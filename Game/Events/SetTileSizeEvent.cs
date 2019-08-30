using System.Numerics;

namespace UAlbion.Game.Events
{
    public class SetTileSizeEvent : GameEvent
    {
        public SetTileSizeEvent(Vector3 tileSize, float baseCameraHeight)
        {
            TileSize = tileSize;
            BaseCameraHeight = baseCameraHeight;
        }
        public Vector3 TileSize { get; }
        public float BaseCameraHeight { get; }
    }
}