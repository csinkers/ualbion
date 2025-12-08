using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.ScriptEvents;

namespace UAlbion.Game.Entities.Map3D;

public class Movement3D : Component
{
    public Movement3D()
    {
        On<PartyMoveEvent>(OnMove);
        On<PartyJumpEvent>(OnJump);
        On<PartyTurnEvent>(OnTurn);
    }

    void OnMove(PartyMoveEvent e)
    {
    }

    void OnTurn(PartyTurnEvent e)
    {
    }

    void OnJump(PartyJumpEvent e)
    {
    }

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