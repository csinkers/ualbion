using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class PartyMovement : Component, IMovement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<PartyMovement, UpdateEvent>((x,e) => x.Update()),
            H<PartyMovement, PartyJumpEvent>((x, e) =>
                {
                    var position = new Vector2(e.X, e.Y);
                    for(int i = 0; i < x._trail.Length; i++)
                        x._trail[i] = (position, 0);
                    x._target = null;
                }),
            H<PartyMovement, BeginFrameEvent>((x, e) => x._direction = Vector2.Zero),
            H<PartyMovement, PartyMoveEvent>((x, e) => x._direction += new Vector2(e.X, e.Y)),
            H<PartyMovement, PartyTurnEvent>((x, e) => { })
        );

        public enum Direction
        {
            Left, Right, Up, Down,
        }

        struct MoveTarget
        {
            public Vector2 From;
            public Vector2 To;
            public int StartTick;
        }

        const int TicksPerTile = 12; // Number of game ticks it takes to move across a map tile
        const int TicksPerFrame = 9; // Number of game ticks it takes to advance to the next animation frame
        int MinTrailDistance => _useSmallSprites ? 6 : 12; 
        int MaxTrailDistance => _useSmallSprites ? 12 : 18; // Max number of positions between each character in the party. Looks best if coprime to TicksPerPile and TicksPerFrame.
        int TrailLength => Party.MaxPartySize * MaxTrailDistance; // Number of past positions to store

        readonly (Vector2, int)[] _trail;
        readonly (int, bool)[] _playerOffsets = new (int, bool)[Party.MaxPartySize]; // int = trail offset, bool = isMoving
        readonly bool _useSmallSprites;
        int _trailOffset;
        Vector2 _direction;
        Direction _facingDirection;
        MoveTarget? _target;
        int _movementTick;

        public PartyMovement(bool useSmallSprites, Vector2 initialPosition, Direction initialDirection) : base(Handlers)
        {
            _useSmallSprites = useSmallSprites;
            _facingDirection = initialDirection;
            _trail = new (Vector2, int)[TrailLength];

            _trailOffset = _trail.Length - 1;
            for (int i = 0; i < _playerOffsets.Length; i++)
                _playerOffsets[i] = (_trailOffset - i * MinTrailDistance, false);

            var offset = initialDirection switch
                {
                    Direction.Left  => new Vector2(1.0f, 0.0f), 
                    Direction.Right => new Vector2(-1.0f, 0.0f), 
                    Direction.Up    => new Vector2(0.0f, -1.0f), 
                    Direction.Down  => new Vector2(0.0f, 1.0f),
                    _ => Vector2.Zero
                } / TicksPerTile;

            for (int i = 0; i < _trail.Length; i++)
                _trail[_trailOffset - i] = (initialPosition + offset * i, SpriteFrame);
        }

        int OffsetAge(int offset) => offset > _trailOffset ? _trailOffset - (offset - _trail.Length) : _trailOffset - offset;

        int SpriteFrame
        {
            get
            {
                var anim = _facingDirection switch
                {
                    Direction.Left => SpriteAnimation.WalkW,
                    Direction.Right => SpriteAnimation.WalkE,
                    Direction.Up => SpriteAnimation.WalkN,
                    Direction.Down => SpriteAnimation.WalkS,
                    _ => SpriteAnimation.Sleeping
                };

                var frames = _useSmallSprites ? SmallSpriteAnimations.Frames[anim] : LargeSpriteAnimations.Frames[anim];
                return frames[(_movementTick / TicksPerFrame) % frames.Length];
            }
        }

        void Update()
        {
            var (position, _) = _trail[_trailOffset];

            if (_target == null && _direction.LengthSquared() > float.Epsilon)
            {
                Direction desiredDirection = _facingDirection;
                if (_direction.X > 0) desiredDirection = Direction.Right;
                else if (_direction.X < 0) desiredDirection = Direction.Left;
                else if (_direction.Y > 0) desiredDirection = Direction.Down;
                else if (_direction.Y < 0) desiredDirection = Direction.Up;

                var target = new MoveTarget { From = position, To = position + _direction, StartTick = _movementTick };
                var oldDirection = _facingDirection;
                (_facingDirection, _target) = (_facingDirection, desiredDirection) switch
                {
                    (Direction.Left, Direction.Left)  => (Direction.Left, target),
                    (Direction.Left, Direction.Right) => (Direction.Down, (MoveTarget?)null),
                    (Direction.Left, Direction.Up)    => (Direction.Up, null),
                    (Direction.Left, Direction.Down)  => (Direction.Down, null),

                    (Direction.Right, Direction.Left)  => (Direction.Down, null),
                    (Direction.Right, Direction.Right) => (Direction.Right, target),
                    (Direction.Right, Direction.Up)    => (Direction.Up, null),
                    (Direction.Right, Direction.Down)  => (Direction.Down, null),

                    (Direction.Up, Direction.Left)  => (Direction.Left, null),
                    (Direction.Up, Direction.Right) => (Direction.Right, null),
                    (Direction.Up, Direction.Up)    => (Direction.Up, target),
                    (Direction.Up, Direction.Down)  => (Direction.Right, null),

                    (Direction.Down, Direction.Left)  => (Direction.Left, null),
                    (Direction.Down, Direction.Right) => (Direction.Right, null),
                    (Direction.Down, Direction.Up)    => (Direction.Right, null),
                    (Direction.Down, Direction.Down)  => (Direction.Down, target),
                    _ => (_facingDirection, null)
                };

                if (_target == null) // If it's just a direction change, we should still update the active frame
                    MoveLeader(position);

                GameTrace.Log.Move(
                    oldDirection, desiredDirection, _facingDirection,
                    target.From.X, target.From.Y, target.To.X, target.To.Y,
                    _trail[_trailOffset].Item2);
            }

            if(_target.HasValue)
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

        void MoveLeader(Vector2 position)
        {
            _trailOffset++;
            if (_trailOffset >= _trail.Length)
                _trailOffset = 0;

            _trail[_trailOffset] = (position, SpriteFrame);
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

        public (Vector2, int) GetPositionHistory(PartyCharacterId partyMember)
        {
            var players = Resolve<IParty>().WalkOrder;
            int index = 0;
            for (; index < players.Count && players[index].Id != partyMember; index++) { }

            if (index == players.Count)
                return (Vector2.Zero, 0);

            var (pos, frame) = _trail[_playerOffsets[index].Item1];
            return (pos, frame);
        }
    }
}
