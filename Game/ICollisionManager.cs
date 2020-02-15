using System.Numerics;
using UAlbion.Formats.Assets;

namespace UAlbion.Game
{
    public interface ICollisionManager
    {
        bool IsOccupied(Vector2 tilePosition);
        TilesetData.Passability GetPassability(Vector2 tilePosition);
        void Register(ICollider collider);
        void Unregister(ICollider collider);
    }
}