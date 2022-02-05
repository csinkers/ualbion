using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game;

public interface ICollisionManager
{
    bool IsOccupied(int x, int y);
    Passability GetPassability(int x, int y);
    void Register(IMovementCollider collider);
    void Unregister(IMovementCollider collider);
}