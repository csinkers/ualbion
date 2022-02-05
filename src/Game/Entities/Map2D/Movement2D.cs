using System;
using UAlbion.Api;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D;

public static class Movement2D
{
    public static bool Update(
        IMovementState state,
        IMovementSettings settings,
        ICollisionManager detector,
        int dx,
        int dy,
        Action<int,int> enteredTile)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        bool moved = false;
        if (!state.HasTarget && (dx != 0 || dy != 0))
            (dx, dy) = CheckForCollisions(state.NoClip ? null : detector, state.X, state.Y, dx, dy);

        if (!state.HasTarget && (dx != 0 || dy != 0))
        {
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
                enteredTile?.Invoke(state.MoveToX, state.MoveToY);

            GameTrace.Log.Move(
                oldDirection, desiredDirection, state.FacingDirection,
                state.X, state.Y, state.MoveToX, state.MoveToY, 0);
        }

        if (state.HasTarget)
        {
            state.MovementTick++;
            var t = (float)(state.MovementTick - state.StartTick) / settings.TicksPerTile;
            state.PixelX = settings.TileWidth * ApiUtil.Lerp(state.X, state.MoveToX, t);
            state.PixelY = settings.TileHeight * ApiUtil.Lerp(state.Y, state.MoveToY, t);
            moved = true;

            GameTrace.Log.Move(
                state.FacingDirection, state.FacingDirection, state.FacingDirection,
                state.X, state.Y, state.MoveToX, state.MoveToY, state.MovementTick - state.StartTick);

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

        return moved;
    }

    static (int x, int y) CheckForCollisions(ICollisionManager detector, int x, int y, int dx, int dy) // Returns direction to move in
    {
        (dx, dy) = (Math.Sign(dx), Math.Sign(dy));

        if (detector == null)
            return (dx, dy);

        if (!detector.IsOccupied(x + dx, y + dy))
            return (dx, dy);

        if (dx != 0 && dy != 0) // First try and reduce diagonal movement to an axis-aligned movement
        {
            var result = CheckForCollisions(detector, x, y, dx, 0);
            if (result.x != 0 || result.y != 0)
                return result;

            return CheckForCollisions(detector, x, y, 0, dy);
        }

        static bool Probe(ICollisionManager detector, int x, int y) => !detector.IsOccupied(x, y);
        return (dx, dy) switch // First probe
        {
            // West
            (-1, 0) when Probe(detector, x - 1, y + 1) && Probe(detector, x + 0, y + 1) => (0, 1), // South
            (-1, 0) when Probe(detector, x - 1, y - 1) && Probe(detector, x + 0, y - 1) => (0, -1), // North

            // East
            (1, 0) when Probe(detector, x + 1, y - 1) && Probe(detector, x + 0, y - 1) => (0, -1), // North
            (1, 0) when Probe(detector, x + 1, y + 1) && Probe(detector, x + 0, y + 1) => (0, 1), // South

            // North
            (0, -1) when Probe(detector, x - 1, y - 1) && Probe(detector, x - 1, y + 0) => (-1, 0), // West
            (0, -1) when Probe(detector, x + 1, y - 1) && Probe(detector, x + 1, y + 0) => (1, 0), // East

            // South
            (0, 1) when Probe(detector, x + 1, y + 1) && Probe(detector, x + 1, y + 0) => (1, 0), // East
            (0, 1) when Probe(detector, x - 1, y + 1) && Probe(detector, x - 1, y + 0) => (-1, 0), // West

            _ => (0, 0)
        };
    }
}