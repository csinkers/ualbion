using System;
using System.Numerics;

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
        Vector2 _direction;
        MoveTarget? _target;
        int _movementTick;
        public Movement2D(MovementSettings settings) => _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public MovementDirection FacingDirection { get; set; }
        public bool Clipping { get; set; } = true;
        public Vector2 Position { get; set; }
        public void AddDirection(Vector2 direction) => _direction += direction;
        public event EventHandler<(int, int)> EnteredTile;

        public int SpriteFrame
        {
            get
            {
                var anim = FacingDirection switch
                {
                    MovementDirection.Left => SpriteAnimation.WalkW,
                    MovementDirection.Right => SpriteAnimation.WalkE,
                    MovementDirection.Up => SpriteAnimation.WalkN,
                    MovementDirection.Down => SpriteAnimation.WalkS,
                    _ => SpriteAnimation.Sleeping
                };

                var frames = _settings.Frames[anim];
                return frames[(_movementTick / _settings.TicksPerFrame) % frames.Length];
            }
        }

        public bool Update(ICollisionManager detector, Vector2 position)
        {
            bool moved = false;
            if (_target == null && _direction.LengthSquared() > float.Epsilon)
                _direction = CheckForCollisions(detector, position, _direction);

            if (_target == null && _direction.LengthSquared() > float.Epsilon)
            {
                MovementDirection desiredDirection = FacingDirection;
                if (_direction.X > 0) desiredDirection = MovementDirection.Right;
                else if (_direction.X < 0) desiredDirection = MovementDirection.Left;
                else if (_direction.Y > 0) desiredDirection = MovementDirection.Down;
                else if (_direction.Y < 0) desiredDirection = MovementDirection.Up;

                var target = new MoveTarget { From = position, To = position + _direction, StartTick = _movementTick };
                var oldDirection = FacingDirection;
                (FacingDirection, _target) = (FacingDirection, desiredDirection) switch
                {
                    (MovementDirection.Left, MovementDirection.Left) => (MovementDirection.Left, target),
                    (MovementDirection.Left, MovementDirection.Right) => (MovementDirection.Down, (MoveTarget?)null),
                    (MovementDirection.Left, MovementDirection.Up) => (MovementDirection.Up, null),
                    (MovementDirection.Left, MovementDirection.Down) => (MovementDirection.Down, null),

                    (MovementDirection.Right, MovementDirection.Left) => (MovementDirection.Down, null),
                    (MovementDirection.Right, MovementDirection.Right) => (MovementDirection.Right, target),
                    (MovementDirection.Right, MovementDirection.Up) => (MovementDirection.Up, null),
                    (MovementDirection.Right, MovementDirection.Down) => (MovementDirection.Down, null),

                    (MovementDirection.Up, MovementDirection.Left) => (MovementDirection.Left, null),
                    (MovementDirection.Up, MovementDirection.Right) => (MovementDirection.Right, null),
                    (MovementDirection.Up, MovementDirection.Up) => (MovementDirection.Up, target),
                    (MovementDirection.Up, MovementDirection.Down) => (MovementDirection.Right, null),

                    (MovementDirection.Down, MovementDirection.Left) => (MovementDirection.Left, null),
                    (MovementDirection.Down, MovementDirection.Right) => (MovementDirection.Right, null),
                    (MovementDirection.Down, MovementDirection.Up) => (MovementDirection.Right, null),
                    (MovementDirection.Down, MovementDirection.Down) => (MovementDirection.Down, target),
                    _ => (FacingDirection, null)
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

            _direction = Vector2.Zero;
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
            bool Probe(Vector2 x) => !detector.IsOccupied(position + x);
            if (Probe(direction))
                return direction;

            if((int)direction.X != 0 && (int)direction.Y != 0) // First try and reduce diagonal movement to an axis-aligned movement
            {
                var result = CheckForCollisions(detector, position, new Vector2(direction.X, 0));
                if ((int)result.X != 0 || (int)result.Y != 0)
                    return result;

                return CheckForCollisions(detector, position, new Vector2(0, direction.Y));
            }

            return ((int)direction.X, (int)direction.Y) switch // First probe
            {
                // Left
                (-1, 0)  when Probe(new Vector2(-1.0f,  1.0f)) && Probe(new Vector2(0.0f, 1.0f)) => new Vector2(0.0f,  1.0f),  // Down
                (-1, 0)  when Probe(new Vector2(-1.0f, -1.0f)) && Probe(new Vector2(0.0f, -1.0f)) => new Vector2(0.0f, -1.0f), // Up

                // Right
                ( 1, 0)  when Probe(new Vector2( 1.0f, -1.0f)) && Probe(new Vector2(0.0f, -1.0f)) => new Vector2(0.0f, -1.0f), // Up
                ( 1, 0)  when Probe(new Vector2( 1.0f,  1.0f)) && Probe(new Vector2(0.0f,  1.0f)) => new Vector2(0.0f,  1.0f), // Down

                // Up
                ( 0, -1) when Probe(new Vector2(-1.0f, -1.0f)) && Probe(new Vector2(-1.0f, 0.0f)) => new Vector2(-1.0f, 0.0f), // Left
                ( 0, -1) when Probe(new Vector2( 1.0f, -1.0f)) && Probe(new Vector2( 1.0f, 0.0f)) => new Vector2( 1.0f, 0.0f), // Right

                // Down
                ( 0, 1)  when Probe(new Vector2( 1.0f,  1.0f)) && Probe(new Vector2( 1.0f, 0.0f)) => new Vector2( 1.0f, 0.0f), // Right
                ( 0, 1)  when Probe(new Vector2(-1.0f,  1.0f)) && Probe(new Vector2(-1.0f, 0.0f)) => new Vector2(-1.0f, 0.0f), // Left

                _ => Vector2.Zero
            };
        }
    }
}
