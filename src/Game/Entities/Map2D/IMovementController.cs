using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D;

public interface IMovementController
{
    /// <summary>
    /// Updates the movement state by one tick, returns true if the movement state was changed.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="state"></param>
    /// <param name="settings"></param>
    /// <param name="detector"></param>
    /// <param name="context"></param>
    /// <param name="getDesiredDirection"></param>
    /// <param name="onTileEntered"></param>
    /// <returns></returns>
    bool Update<TContext>(
        IMovementState state,
        IMovementSettings settings,
        ICollisionManager detector,
        TContext context,
        Func<TContext, (int X, int Y)> getDesiredDirection,
        Action<int, int> onTileEntered);
}