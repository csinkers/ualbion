using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game;

public interface IMovementCollider
{
    bool IsOccupied(int tx, int ty);
    Passability GetPassability(int tx, int ty);
}