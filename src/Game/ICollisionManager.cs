namespace UAlbion.Game;

public interface ICollisionManager
{
    bool IsOccupied(int fromX, int fromY, int toX, int toY);
    void Register(IMovementCollider collider);
    void Unregister(IMovementCollider collider);
}