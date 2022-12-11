using System;
using UAlbion.Api;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D;

public class Movement2D : IMovementController
{
    Movement2D() {}
    public static Movement2D Instance { get; } = new();

    public bool Update<TContext>(
        IMovementState state,
        IMovementSettings settings,
        ICollisionManager detector,
        TContext context,
        Func<TContext, (int X, int Y)> getDesiredDirection,
        Action<int, int> onTileEntered)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        if (getDesiredDirection == null) throw new ArgumentNullException(nameof(getDesiredDirection));

        GameTrace.Log.MoveStart(state.Id.ToString(), state.X, state.Y, state.PixelX, state.PixelY);

        bool moved = false;
        if (!state.HasTarget)
        {
            var (dx, dy) = getDesiredDirection(context);
            if (dx != 0 || dy != 0)
                (dx, dy) = CheckForCollisions(state.NoClip ? null : detector, state.X, state.Y, dx, dy);

            if (dx == 0 && dy == 0)
            {
                GameTrace.Log.MoveStop(state.Id.ToString(), false);
                return false;
            }

            var oldDirection = state.FacingDirection;
            var desiredDirection = state.FacingDirection;

            if (dx > 0) desiredDirection = Direction.East;
            else if (dx < 0) desiredDirection = Direction.West;
            else if (dy > 0) desiredDirection = Direction.South;
            else if (dy < 0) desiredDirection = Direction.North;

            // Only start moving if we're already facing the right way
            state.HasTarget = desiredDirection == state.FacingDirection;
            state.MoveToX = (ushort)(state.X + dx);
            state.MoveToY = (ushort)(state.Y + dy);
            state.StartTick = state.MovementTick;

            state.FacingDirection = (state.FacingDirection, desiredDirection) switch
            {
                (Direction.West, Direction.East) => Direction.South,
                (Direction.East, Direction.West) => Direction.South,
                (Direction.North, Direction.South) => Direction.East,
                (Direction.South, Direction.North) => Direction.East,
                _ => desiredDirection
            };

            if (!state.HasTarget) // If it's just a direction change, we should still update the active frame
                moved = true;
            else
                onTileEntered?.Invoke(state.MoveToX, state.MoveToY);

            GameTrace.Log.MoveDir(oldDirection, desiredDirection, state.FacingDirection);
            GameTrace.Log.MovePos(state.X, state.Y, state.MoveToX, state.MoveToY, 0);
        }

        if (state.HasTarget)
        {
            state.MovementTick++;
            var t = (float)(state.MovementTick - state.StartTick) / settings.TicksPerTile;
            state.PixelX = settings.TileWidth * ApiUtil.Lerp(state.X, state.MoveToX, t);
            state.PixelY = settings.TileHeight * ApiUtil.Lerp(state.Y, state.MoveToY, t);
            moved = true;

            GameTrace.Log.MoveDir(state.FacingDirection, state.FacingDirection, state.FacingDirection);
            GameTrace.Log.MovePos(state.X, state.Y, state.MoveToX, state.MoveToY, state.MovementTick - state.StartTick);

            if (state.MovementTick - state.StartTick >= settings.TicksPerTile)
            {
                state.X = state.MoveToX;
                state.Y = state.MoveToY;
                state.HasTarget = false;
            }
        }
        else
        {
            state.PixelX = settings.TileWidth * state.X;
            state.PixelY = settings.TileHeight * state.Y;
        }

        GameTrace.Log.MoveStop(state.Id.ToString(), moved);
        return moved;
    }

    public bool Update(
        IMovementState state,
        IMovementSettings settings,
        ICollisionManager detector,
        int dx,
        int dy,
        Action<int,int> enteredTile) => Update(state, settings, detector, (dx,dy), dir => dir, enteredTile);

    static (int x, int y) CheckForCollisions(ICollisionManager detector, int curX, int curY, int dx, int dy) // Returns direction to move in
    {
        (dx, dy) = (Math.Sign(dx), Math.Sign(dy));

        if (detector == null)
            return (dx, dy);

        if (!detector.IsOccupied(curX, curY, curX + dx, curY + dy))
            return (dx, dy);

        if (dx != 0 && dy != 0) // First try and reduce diagonal movement to an axis-aligned movement
        {
            var result = CheckForCollisions(detector, curX, curY, dx, 0);
            if (result.x != 0 || result.y != 0)
                return result;

            return CheckForCollisions(detector, curX, curY, 0, dy);
        }

        bool Probe(int x, int y) => !detector.IsOccupied(curX, curY, x, y);
        return (dx, dy) switch // First probe
        {
            // West
            (-1, 0) when Probe(curX - 1, curY + 1) && Probe(curX + 0, curY + 1) => (0, 1), // South
            (-1, 0) when Probe(curX - 1, curY - 1) && Probe(curX + 0, curY - 1) => (0, -1), // North

            // East
            (1, 0) when Probe(curX + 1, curY - 1) && Probe(curX + 0, curY - 1) => (0, -1), // North
            (1, 0) when Probe(curX + 1, curY + 1) && Probe(curX + 0, curY + 1) => (0, 1), // South

            // North
            (0, -1) when Probe(curX - 1, curY - 1) && Probe(curX - 1, curY + 0) => (-1, 0), // West
            (0, -1) when Probe(curX + 1, curY - 1) && Probe(curX + 1, curY + 0) => (1, 0), // East

            // South
            (0, 1) when Probe(curX + 1, curY + 1) && Probe(curX + 1, curY + 0) => (1, 0), // East
            (0, 1) when Probe(curX - 1, curY + 1) && Probe(curX - 1, curY + 0) => (-1, 0), // West

            _ => (0, 0)
        };
    }
}