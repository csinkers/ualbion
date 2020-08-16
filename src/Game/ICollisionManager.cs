using System.Numerics;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game
{
    public interface ICollisionManager
    {
        bool IsOccupied(Vector2 tilePosition);
        Passability GetPassability(Vector2 tilePosition);
        void Register(IMovementCollider collider);
        void Unregister(IMovementCollider collider);
    }
}
