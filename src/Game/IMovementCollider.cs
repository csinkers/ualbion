namespace UAlbion.Game;

public interface IMovementCollider
{
    bool IsOccupied(int fromX, int fromY, int toX, int toY);
}