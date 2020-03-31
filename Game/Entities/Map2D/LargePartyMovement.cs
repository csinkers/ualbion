using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class LargePartyMovement : ServiceComponent<IMovement>, IMovement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<LargePartyMovement, UpdateEvent>((x,e) => x.Update()),
            H<LargePartyMovement, PartyJumpEvent>((x, e) =>
                {
                    var position = new Vector2(e.X, e.Y);
                    for (int i = 0; i < x._trail.Length; i++)
                        x._trail[i] = (To3D(position), 0);

                    x._target = null;
                }),
            H<LargePartyMovement, BeginFrameEvent>((x, e) => x._direction = Vector2.Zero),
            H<LargePartyMovement, PartyMoveEvent>((x, e) => x._direction += new Vector2(e.X, e.Y)),
            H<LargePartyMovement, PartyTurnEvent>((x, e) =>
            {
                var (position3d, _) = x._trail[x._trailOffset];
                var position = new Vector2(position3d.X, position3d.Y);
                x._facingDirection = e.Direction switch
                {
                    TeleportDirection.Up => MovementDirection.Up,
                    TeleportDirection.Right => MovementDirection.Right,
                    TeleportDirection.Down => MovementDirection.Down,
                    TeleportDirection.Left => MovementDirection.Left,
                    _ => x._facingDirection
                };
                x.MoveLeader(position);
            }),
            H<LargePartyMovement, NoClipEvent>((x, e) =>
            {
                x._clipping = !x._clipping;
                x.Raise(new LogEvent(LogEvent.Level.Info, $"Clipping {(x._clipping ? "on" : "off")}"));
            })
        );

        struct MoveTarget
        {
            public Vector2 From;
            public Vector2 To;
            public int StartTick;
        }

        const int TicksPerTile = 12; // Number of game ticks it takes to move across a map tile
        const int TicksPerFrame = 9; // Number of game ticks it takes to advance to the next animation frame
        const int MinTrailDistance = 12;
        const int MaxTrailDistance = 18; // Max number of positions between each character in the party. Looks best if coprime to TicksPerPile and TicksPerFrame.
        int TrailLength => Party.MaxPartySize * MaxTrailDistance; // Number of past positions to store

        readonly (Vector3, int)[] _trail; // Positions (tile coordinates) and frame numbers.
        readonly (int, bool)[] _playerOffsets = new (int, bool)[Party.MaxPartySize]; // int = trail offset, bool = isMoving
        int _trailOffset;
        Vector2 _direction;
        MovementDirection _facingDirection;
        MoveTarget? _target;
        int _movementTick;
        bool _clipping = true;

        public LargePartyMovement(Vector2 initialPosition, MovementDirection initialDirection) : base(Handlers)
        {
            _facingDirection = initialDirection;
            _trail = new (Vector3, int)[TrailLength];

            _trailOffset = _trail.Length - 1;
            for (int i = 0; i < _playerOffsets.Length; i++)
                _playerOffsets[i] = (_trailOffset - i * MinTrailDistance, false);

            var offset = (initialDirection switch
                {
                    MovementDirection.Left  => new Vector2(1.0f, 0.0f),
                    MovementDirection.Right => new Vector2(-1.0f, 0.0f),
                    MovementDirection.Up    => new Vector2(0.0f, -1.0f),
                    MovementDirection.Down  => new Vector2(0.0f, 1.0f),
                    _ => Vector2.Zero
                }) / TicksPerTile;

            for (int i = 0; i < _trail.Length; i++)
            {
                var position = initialPosition + offset * i;
                _trail[_trailOffset - i] = (To3D(position), SpriteFrame);
            }
        }

        int OffsetAge(int offset) => offset > _trailOffset ? _trailOffset - (offset - _trail.Length) : _trailOffset - offset;

        int SpriteFrame
        {
            get
            {
                var anim = _facingDirection switch
                {
                    MovementDirection.Left => SpriteAnimation.WalkW,
                    MovementDirection.Right => SpriteAnimation.WalkE,
                    MovementDirection.Up => SpriteAnimation.WalkN,
                    MovementDirection.Down => SpriteAnimation.WalkS,
                    _ => SpriteAnimation.Sleeping
                };

                var frames = LargeSpriteAnimations.Frames[anim];
                return frames[(_movementTick / TicksPerFrame) % frames.Length];
            }
        }

        void Update()
        {
            var (position3d, _) = _trail[_trailOffset];
            var position = new Vector2(position3d.X, position3d.Y);

            if (_target == null && _direction.LengthSquared() > float.Epsilon)
                _direction = CheckForCollisions(position, _direction);

            if (_target == null && _direction.LengthSquared() > float.Epsilon)
            {
                MovementDirection desiredDirection = _facingDirection;
                if (_direction.X > 0) desiredDirection = MovementDirection.Right;
                else if (_direction.X < 0) desiredDirection = MovementDirection.Left;
                else if (_direction.Y > 0) desiredDirection = MovementDirection.Down;
                else if (_direction.Y < 0) desiredDirection = MovementDirection.Up;

                var target = new MoveTarget { From = position, To = position + _direction, StartTick = _movementTick };
                var oldDirection = _facingDirection;
                (_facingDirection, _target) = (_facingDirection, desiredDirection) switch
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
                    _ => (_facingDirection, null)
                };

                if (_target == null) // If it's just a direction change, we should still update the active frame
                    MoveLeader(position);
                else
                    Raise(new PlayerEnteredTileEvent((int)_target.Value.To.X, (int)_target.Value.To.Y));

                GameTrace.Log.Move(
                    oldDirection, desiredDirection, _facingDirection,
                    target.From.X, target.From.Y, target.To.X, target.To.Y,
                    _trail[_trailOffset].Item2);
            }

            if (_target.HasValue)
            {
                _movementTick++;
                var target = _target.Value;
                position = Vector2.Lerp(target.From, target.To, ((float)_movementTick - target.StartTick) / TicksPerTile);
                MoveLeader(position);

                GameTrace.Log.Move(
                    _facingDirection, _facingDirection, _facingDirection,
                    _trail[_trailOffset].Item1.X, _trail[_trailOffset].Item1.Y, target.To.X, target.To.Y,
                    _trail[_trailOffset].Item2);

                if (_movementTick - target.StartTick >= TicksPerTile)
                    _target = null;
            }

            MoveFollowers();
        }

        Vector2 CheckForCollisions(Vector2 position, Vector2 direction) // Returns direction to move in
        {
            direction = new Vector2(Math.Sign(direction.X), Math.Sign(direction.Y));

            var detector = Resolve<ICollisionManager>();
            if (!_clipping || detector == null)
                return direction;

#if false // Print passability of adjacent tiles for debugging
            {
                var testPoints = new[]
                {
                    new Vector2(-2.0f, -1.0f), // 0123
                    new Vector2(-1.0f, -1.0f), // 4 @5
                    new Vector2(0.0f, -1.0f),  // 6789
                    new Vector2(1.0f, -1.0f),

                    new Vector2(-2.0f, 0.0f),
                    new Vector2(1.0f, 0.0f),

                    new Vector2(-2.0f, 1.0f),
                    new Vector2(-1.0f, 1.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
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
                Raise(new LogEvent(LogEvent.Level.Info, $"{R(0)}{R(1)}{R(2)}{R(3)}"));
                Raise(new LogEvent(LogEvent.Level.Info, $"{R(4)}@@{R(5)}"));
                Raise(new LogEvent(LogEvent.Level.Info, $"{R(6)}{R(7)}{R(8)}{R(9)}"));
                Raise(new LogEvent(LogEvent.Level.Info, ""));
            }
#endif
            bool Probe(Vector2 x) => !detector.IsOccupied(position + x) && !detector.IsOccupied(position + x + new Vector2(-1.0f, 0.0f));
            if (Probe(direction))
                return direction;

            if((int)direction.X != 0 && (int)direction.Y != 0) // First try and reduce diagonal movement to an axis-aligned movement
            {
                var result = CheckForCollisions(position, new Vector2(direction.X, 0));
                if ((int)result.X != 0 || (int)result.Y != 0)
                    return result;

                return CheckForCollisions(position, new Vector2(0, direction.Y));
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

        void MoveLeader(Vector2 position)
        {
            _trailOffset++;
            if (_trailOffset >= _trail.Length)
                _trailOffset = 0;

            _trail[_trailOffset] = (To3D(position), SpriteFrame);
            _playerOffsets[0] = (_trailOffset, true);
        }

        void MoveFollowers()
        {
            for(int i = 1; i < _playerOffsets.Length; i++)
            {
                int predecessorAge = OffsetAge(_playerOffsets[i - 1].Item1);
                int myAge = OffsetAge(_playerOffsets[i].Item1);

                if (_playerOffsets[i].Item2 || myAge - predecessorAge > MaxTrailDistance)
                {
                    int newOffset = _playerOffsets[i].Item1 + 1;
                    if (newOffset >= _trail.Length)
                        newOffset = 0;

                    bool keepMoving = (OffsetAge(newOffset) - predecessorAge) > MinTrailDistance;
                    _playerOffsets[i] = (newOffset, keepMoving);
                }
            }
        }

        public (Vector3, int) GetPositionHistory(PartyCharacterId partyMember)
        {
            var players = Resolve<IParty>().WalkOrder;
            int index = 0;
            for (; index < players.Count && players[index].Id != partyMember; index++) { }

            if (index == players.Count)
                return (Vector3.Zero, 0);

            var (pos, frame) = _trail[_playerOffsets[index].Item1];
            return (pos, frame);
        }

        static Vector3 To3D(Vector2 position) => new Vector3(position, DrawLayer.Characters1.ToZCoordinate(position.Y - 1));
    }
}
