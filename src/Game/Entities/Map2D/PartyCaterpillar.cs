using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D;

public class PartyCaterpillar : ServiceComponent<IMovement>, IMovement
{
    readonly MovementSettings _settings;
    readonly Movement2D _movement;

    readonly (Vector3, int)[] _trail; // Positions (tile coordinates) and frame numbers.
    readonly (int, bool)[] _playerOffsets = new (int, bool)[Party.MaxPartySize]; // int = trail offset, bool = isMoving
    Vector2 _direction;
    int _trailOffset;
    int TrailLength => Party.MaxPartySize * _settings.MaxTrailDistance; // Number of past positions to store

    public PartyCaterpillar(Vector2 initialPosition, Direction initialDirection, MovementSettings settings)
    {
        _settings = settings;
        _movement = new Movement2D(settings);
        _movement.EnteredTile += (sender, coords) => Raise(new PlayerEnteredTileEvent(coords.Item1, coords.Item2));

        _trail = new (Vector3, int)[TrailLength];
        _trailOffset = _trail.Length - 1;
        for (int i = 0; i < _playerOffsets.Length; i++)
            _playerOffsets[i] = (_trailOffset - i * _settings.MinTrailDistance, false);

        var offset = (initialDirection switch
        {
            Direction.West  => new Vector2(1.0f, 0.0f),
            Direction.East => new Vector2(-1.0f, 0.0f),
            Direction.North    => new Vector2(0.0f, -1.0f),
            Direction.South  => new Vector2(0.0f, 1.0f),
            _ => Vector2.Zero
        }) / _settings.TicksPerTile;

        for (int i = 0; i < _trail.Length; i++)
        {
            var position = initialPosition + offset * i;
            _trail[_trailOffset - i] = (To3D(position), _movement.SpriteFrame);
        }

        On<FastClockEvent>(_ => Update());
        On<PartyMoveEvent>(e => _direction += new Vector2(e.X, e.Y));
        On<PartyJumpEvent>(e =>
        {
            var position = new Vector2(e.X, e.Y);
            for (int i = 0; i < _trail.Length; i++)
                _trail[i] = (To3D(position), 0);

            _movement.Position = position;
        });
        On<PartyTurnEvent>(e =>
        {
            var (position3d, _) = _trail[_trailOffset];
            var position = new Vector2(position3d.X, position3d.Y);
            _movement.FacingDirection = e.Direction;
            MoveLeader(position);
        });
        On<NoClipEvent>(_ =>
        {
            _movement.Clipping = !_movement.Clipping;
            Info($"Clipping {(_movement.Clipping ? "on" : "off")}");
        });
    }

    int OffsetAge(int offset) => offset > _trailOffset ? _trailOffset - (offset - _trail.Length) : _trailOffset - offset;

    void Update()
    {
        var detector = Resolve<ICollisionManager>();
        if (_movement.Update(detector, _direction))
            MoveLeader(_movement.Position);

        _direction = Vector2.Zero;
        MoveFollowers();
    }

    void MoveLeader(Vector2 position)
    {
        _trailOffset++;
        if (_trailOffset >= _trail.Length)
            _trailOffset = 0;

        _trail[_trailOffset] = (To3D(position), _movement.SpriteFrame);
        _playerOffsets[0] = (_trailOffset, true);
    }

    void MoveFollowers()
    {
        for(int i = 1; i < _playerOffsets.Length; i++)
        {
            int predecessorAge = OffsetAge(_playerOffsets[i - 1].Item1);
            int myAge = OffsetAge(_playerOffsets[i].Item1);

            if (_playerOffsets[i].Item2 || myAge - predecessorAge > _settings.MaxTrailDistance)
            {
                int newOffset = _playerOffsets[i].Item1 + 1;
                if (newOffset >= _trail.Length)
                    newOffset = 0;

                bool keepMoving = (OffsetAge(newOffset) - predecessorAge) > _settings.MinTrailDistance;
                _playerOffsets[i] = (newOffset, keepMoving);
            }
        }
    }

    public (Vector3, int) GetPositionHistory(int followerIndex) 
        => followerIndex >= _playerOffsets.Length 
            ? (Vector3.Zero, 0) 
            : _trail[_playerOffsets[followerIndex].Item1];

    Vector3 To3D(Vector2 position) => new(position, _settings.GetDepth(position.Y));
}