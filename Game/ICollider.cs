using System.Numerics;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Game
{
    public interface ICollider
    {
        bool IsOccupied(Vector2 tilePosition);
        Passability GetPassability(Vector2 tilePosition);
    }
}
