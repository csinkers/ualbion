using System.Numerics;

namespace UAlbion.Game.Input;

public interface ICursorManager
{
    /// <summary>
    /// The cursor position in pixel coordinates
    /// </summary>
    Vector2 Position { get; }
}