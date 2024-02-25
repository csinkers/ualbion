using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D;

public class PartyCaterpillar : ServiceComponent<IMovement>, IMovement
{
    readonly MovementSettings _settings;
    readonly PlayerMovementState _state;

    readonly (Vector3, int)[] _trail; // Positions (tile coordinates) and frame numbers.
    readonly (int, bool)[] _playerOffsets = new (int, bool)[SavedGame.MaxPartySize]; // int = trail offset, bool = isMoving
    readonly LogicalMap2D _logicalMap;
    Vector2 _direction;
    int _trailOffset;
    int TrailLength => SavedGame.MaxPartySize * _settings.MaxTrailDistance; // Number of past positions to store

    public PartyCaterpillar(Vector2 initialPosition, Direction initialDirection, MovementSettings settings, LogicalMap2D logicalMap)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));

        _state = new PlayerMovementState(settings)
        {
            X = (ushort)initialPosition.X,
            Y = (ushort)initialPosition.Y,
            FacingDirection = initialDirection
        };

        _trail = new (Vector3, int)[TrailLength];
        _trailOffset = _trail.Length - 1;
        for (int i = 0; i < _playerOffsets.Length; i++)
            _playerOffsets[i] = (_trailOffset - i * _settings.MinTrailDistance, false);

        var offset = initialDirection switch
        {
            Direction.West => new Vector2(1.0f, 0.0f),
            Direction.East => new Vector2(-1.0f, 0.0f),
            Direction.North => new Vector2(0.0f, -1.0f),
            Direction.South => new Vector2(0.0f, 1.0f),
            _ => Vector2.Zero
        } / _settings.TicksPerTile;

        for (int i = 0; i < _trail.Length; i++)
        {
            var position = initialPosition + offset * i;
            _trail[_trailOffset - i] = (To3D(position), settings.GetSpriteFrame(_state, GetSitMode));
        }

        var oldTicksPerTile = _settings.TicksPerTile;
        var oldTicksPerFrame = _settings.TicksPerFrame;
        After<DebugFlagEvent>(_ =>
        {
            var debugFlags = ReadVar(V.User.Debug.DebugFlags);
            if ((debugFlags & DebugFlags.FastMovement) != 0)
            {
                _settings.TicksPerTile = (oldTicksPerTile + 3) / 4;
                _settings.TicksPerTile = (oldTicksPerFrame + 3) / 4;
            }
            else
            {
                _settings.TicksPerTile = oldTicksPerTile;
                _settings.TicksPerTile = oldTicksPerFrame;
            }

        });

        On<FastClockEvent>(_ => Update());
        On<PartyMoveEvent>(e => _direction += new Vector2(e.X, e.Y));
        On<PartyJumpEvent>(e =>
        {
            var position = new Vector2(e.X, e.Y);
            for (int i = 0; i < _trail.Length; i++)
                _trail[i] = (To3D(position), _settings.GetSpriteFrame(_state, GetSitMode));

            _state.X = (ushort)e.X;
            _state.Y = (ushort)e.Y;
        });
        On<PartyTurnEvent>(e =>
        {
            var (position3d, _) = _trail[_trailOffset];
            var position = new Vector2(position3d.X, position3d.Y);
            _state.FacingDirection = e.Direction;
            MoveLeader(position);
        });
        On<NoClipEvent>(_ =>
        {
            _state.NoClip = !_state.NoClip;
            Info($"Clipping {(_state.NoClip ? "off" : "on")}");
        });
    }

    int OffsetAge(int offset) => offset > _trailOffset ? _trailOffset - (offset - _trail.Length) : _trailOffset - offset;

    void Update()
    {
        var detector = Resolve<ICollisionManager>();
        if (Movement2D.Instance.Update(_state,
                _settings,
                detector,
                (int)_direction.X,
                (int)_direction.Y,
                OnEnteredTile))
        {
            MoveLeader(new Vector2(
                _state.PixelX / _state.Settings.TileWidth,
                _state.PixelY / _state.Settings.TileHeight));
        }

        _direction = Vector2.Zero;
        MoveFollowers();
    }

    void OnEnteredTile(int x, int y) => Raise(new PlayerEnteredTileEvent(x, y));

    SitMode GetSitMode(int x, int y)
    {
        var underlay = _logicalMap.GetUnderlay(x, y)?.SitMode ?? 0;
        var overlay = _logicalMap.GetOverlay(x, y)?.SitMode ?? 0;
        return overlay == 0 ? underlay : overlay;
    }

    void MoveLeader(Vector2 position)
    {
        _trailOffset++;
        if (_trailOffset >= _trail.Length)
            _trailOffset = 0;

        _trail[_trailOffset] = (To3D(position), _settings.GetSpriteFrame(_state, GetSitMode));
        _playerOffsets[0] = (_trailOffset, true);
    }

    void MoveFollowers()
    {
        for (int i = 1; i < _playerOffsets.Length; i++)
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