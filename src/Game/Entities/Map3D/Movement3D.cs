using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map3D;

public class Movement3D(Vector2 initialPos) : Component
{
    // TODO: Implement collision detection etc for 3D maps
    public static void Update<TContext>(
        IMovementState state,
        ICollisionManager detector,
        TContext context,
        Func<TContext, (int X, int Y)> getDesiredDirection,
        Action<int, int> onTileEntered)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(getDesiredDirection);

        // var (dx, dy) = getDesiredDirection(context);
        // (dx, dy) = CheckForCollisions(state.NoClip ? null : detector, state.X, state.Y, dx, dy);
        // onTileEntered?.Invoke(state.MoveToX, state.MoveToY);
        // state.MovementTick++;
    }
}