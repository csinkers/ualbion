using System;
using System.Numerics;
using UAlbion.Formats;

namespace UAlbion.Game.Entities.Map2D
{
    public class Movement2D
    {
        struct MoveTarget
        {
            public Vector2 From;
            public Vector2 To;
            public int StartTick;
        }

        readonly MovementSettings _settings;
        MoveTarget? _target;
        int _movementTick;
        public Movement2D(MovementSettings settings) => _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public Direction FacingDirection { get; set; }
        public bool Clipping { get; set; } = true;
        public Vector2 Position { get; set; }
        public event EventHandler<(int, int)> EnteredTile;

        public int SpriteFrame
        {
            get
            {
                var anim = FacingDirection switch
                {
                    Direction.West => SpriteAnimation.WalkW,
                    Direction.East => SpriteAnimation.WalkE,
                    Direction.North => SpriteAnimation.WalkN,
                    Direction.South => SpriteAnimation.WalkS,
                    _ => SpriteAnimation.Sleeping
                };

                var frames = _settings.Frames[anim];
                return frames[(_movementTick / _settings.TicksPerFrame) % frames.Length];
            }
        }

        public bool Update(ICollisionManager detector, Vector2 direction)
        {
            bool moved = false;
            if (_target == null && direction.LengthSquared() > float.Epsilon)
                direction = CheckForCollisions(detector, Position, direction);

            if (_target == null && direction.LengthSquared() > float.Epsilon)
            {
                Direction desiredDirection = FacingDirection;
                if (direction.X > 0) desiredDirection = Direction.East;
                else if (direction.X < 0) desiredDirection = Direction.West;
                else if (direction.Y > 0) desiredDirection = Direction.South;
                else if (direction.Y < 0) desiredDirection = Direction.North;

                var target = new MoveTarget { From = Position, To = Position + direction, StartTick = _movementTick };
                var oldDirection = FacingDirection;
                (FacingDirection, _target) = (FacingDirection, desiredDirection) switch
                {
                    ({ } a, { } b) when a == b => (a, target),
                    (Direction.West, Direction.East) => (Direction.South, (MoveTarget?)null),
                    (Direction.East, Direction.West) => (Direction.South, null),
                    (Direction.North, Direction.South) => (Direction.East, null),
                    (Direction.South, Direction.North) => (Direction.East, null),
                    _ => (desiredDirection, null)
                };

                if (_target == null) // If it's just a direction change, we should still update the active frame
                    moved = true;
                else
                    EnteredTile?.Invoke(this, ((int)_target.Value.To.X, (int)_target.Value.To.Y));

                GameTrace.Log.Move(
                    oldDirection, desiredDirection, FacingDirection,
                    target.From.X, target.From.Y, target.To.X, target.To.Y, 0);
            }

            if (_target.HasValue)
            {
                _movementTick++;
                var target = _target.Value;
                Position = Vector2.Lerp(
                    target.From,
                    target.To,
                    ((float)_movementTick - target.StartTick) / _settings.TicksPerTile);
                moved = true;
/*
                GameTrace.Log.Move(
                    FacingDirection, FacingDirection, FacingDirection,
                    _trail[_trailOffset].Item1.X, _trail[_trailOffset].Item1.Y, target.To.X, target.To.Y,
                    _trail[_trailOffset].Item2);
*/
                if (_movementTick - target.StartTick >= _settings.TicksPerTile)
                    _target = null;
            }

            return moved;
        }

        Vector2 CheckForCollisions(ICollisionManager detector, Vector2 position, Vector2 direction) // Returns direction to move in
        {
            direction = new Vector2(Math.Sign(direction.X), Math.Sign(direction.Y));

            if (!Clipping || detector == null)
                return direction;

#if false // Print passability of adjacent tiles for debugging
            {
                var testPoints = new[]
                {
                    new Vector2(-1.0f, -1.0f), new Vector2(0.0f, -1.0f), new Vector2(1.0f, -1.0f),
                    new Vector2(-1.0f,  0.0f),                               new Vector2(1.0f,  0.0f),
                    new Vector2(-1.0f,  1.0f), new Vector2(0.0f,  1.0f), new Vector2(1.0f,  1.0f),
                }.Select(x => x + position).ToArray();

                var r = testPoints.Select(x => detector.GetPassability(x)).ToArray(); // Results

                char R(int x) => r[x] switch {
                    TilesetData.Passability.Passable      => ' ',
                    TilesetData.Passability.Passability1  => '1',
                    TilesetData.Passability.Passability2  => '2',
                    TilesetData.Passability.Passability3  => '3',
                    TilesetData.Passability.Passability4  => '4',
                    TilesetData.Passability.Passability5  => '5',
                    TilesetData.Passability.Passability6  => '6',
                    TilesetData.Passability.Blocked       => 'B',
                    TilesetData.Passability.Passability9  => '9',
                    TilesetData.Passability.Passability10 => 'A',
                    TilesetData.Passability.Passability12 => 'C',
                    TilesetData.Passability.Passability16 => 'G',
                    TilesetData.Passability.Passability24 => 'Z',
                    _ => throw new ArgumentOutOfRangeException()
                };
                Raise(new LogEvent(LogEvent.Level.Info, $"Vicinity of {position}"));
                Raise(new LogEvent(LogEvent.Level.Info, $"{R(0)}{R(1)}{R(2)}"));
                Raise(new LogEvent(LogEvent.Level.Info, $"{R(3)}@{R(4)}"));
                Raise(new LogEvent(LogEvent.Level.Info, $"{R(5)}{R(6)}{R(7)}"));
                Raise(new LogEvent(LogEvent.Level.Info, ""));
            }
#endif
            if (!detector.IsOccupied(position + direction))
                return direction;

            if ((int)direction.X != 0 && (int)direction.Y != 0) // First try and reduce diagonal movement to an axis-aligned movement
            {
                var result = CheckForCollisions(detector, position, new Vector2(direction.X, 0));
                if ((int)result.X != 0 || (int)result.Y != 0)
                    return result;

                return CheckForCollisions(detector, position, new Vector2(0, direction.Y));
            }

            static bool Probe(ICollisionManager detector, Vector2 position, int x, int y) => !detector.IsOccupied(position + new Vector2(x, y));
            return ((int)direction.X, (int)direction.Y) switch // First probe
            {
                // West
                (-1, 0)  when Probe(detector, position, -1,  1) && Probe(detector, position, 0,  1) => new Vector2(0.0f,  1.0f), // South
                (-1, 0)  when Probe(detector, position, -1, -1) && Probe(detector, position, 0, -1) => new Vector2(0.0f, -1.0f), // North

                // East
                ( 1, 0)  when Probe(detector, position,  1, -1) && Probe(detector, position, 0, -1) => new Vector2(0.0f, -1.0f), // North
                ( 1, 0)  when Probe(detector, position,  1,  1) && Probe(detector, position, 0,  1) => new Vector2(0.0f,  1.0f), // South

                // North
                ( 0, -1) when Probe(detector, position, -1, -1) && Probe(detector, position, -1, 0) => new Vector2(-1.0f, 0.0f), // West
                ( 0, -1) when Probe(detector, position,  1, -1) && Probe(detector, position,  1, 0) => new Vector2( 1.0f, 0.0f), // East

                // South
                ( 0, 1)  when Probe(detector, position,  1,  1) && Probe(detector, position,  1, 0) => new Vector2( 1.0f, 0.0f), // East
                ( 0, 1)  when Probe(detector, position, -1,  1) && Probe(detector, position, -1, 0) => new Vector2(-1.0f, 0.0f), // West

                _ => Vector2.Zero
            };
        }
    }
}
