using System.Numerics;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Input;

public interface ICursorManager
{
    /// <summary>
    /// The cursor position in pixel coordinates
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// The current cursor
    /// </summary>
    SpriteId CursorId { get; }
}