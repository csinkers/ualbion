using System.Numerics;

namespace UAlbion.Game
{
    public interface ICollider
    {
        bool IsOccupied(Vector2 tilePosition);
    }
}
