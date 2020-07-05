using System.Numerics;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Game
{
    public interface IMovementCollider
    {
        bool IsOccupied(Vector2 tilePosition);
        Passability GetPassability(Vector2 tilePosition);
    }
}
