using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class PartyCaterpillar : ServiceComponent<IMovement>, IMovement
    {
        readonly MovementSettings _settings;
        readonly Movement _movement;

        readonly (Vector3, int)[] _trail; // Positions (tile coordinates) and frame numbers.
        readonly (int, bool)[] _playerOffsets = new (int, bool)[Party.MaxPartySize]; // int = trail offset, bool = isMoving
        int _trailOffset;
        int TrailLength => Party.MaxPartySize * _settings.MaxTrailDistance; // Number of past positions to store

        static readonly HandlerSet Handlers = new HandlerSet(
            H<PartyCaterpillar, FastClockEvent>((x,e) => x.Update()),
            H<PartyCaterpillar, PartyJumpEvent>((x, e) =>
            {
                var position = new Vector2(e.X, e.Y);
                for (int i = 0; i < x._trail.Length; i++)
                    x._trail[i] = (x.To3D(position), 0);

                x._movement.Position = position;
            }),
            H<PartyCaterpillar, PartyMoveEvent>((x, e) => x._movement.AddDirection(new Vector2(e.X, e.Y))),
            H<PartyCaterpillar, PartyTurnEvent>((x, e) =>
            {
                var (position3d, _) = x._trail[x._trailOffset];
                var position = new Vector2(position3d.X, position3d.Y);
                x._movement.FacingDirection = e.Direction switch
                {
                    TeleportDirection.Up => MovementDirection.Up,
                    TeleportDirection.Right => MovementDirection.Right,
                    TeleportDirection.Down => MovementDirection.Down,
                    TeleportDirection.Left => MovementDirection.Left,
                    _ => x._movement.FacingDirection
                };
                x.MoveLeader(position);
            }),
            H<PartyCaterpillar, NoClipEvent>((x, e) =>
            {
                x._movement.Clipping = !x._movement.Clipping;
                x.Raise(new LogEvent(LogEvent.Level.Info, $"Clipping {(x._movement.Clipping ? "on" : "off")}"));
            })
        );

        public PartyCaterpillar(Vector2 initialPosition, MovementDirection initialDirection, MovementSettings settings) : base(Handlers)
        {
            _settings = settings;
            _movement = new Movement(settings);
            _movement.EnteredTile += (sender, coords) => Raise(new PlayerEnteredTileEvent(coords.Item1, coords.Item2));

            _trail = new (Vector3, int)[TrailLength];
            _trailOffset = _trail.Length - 1;
            for (int i = 0; i < _playerOffsets.Length; i++)
                _playerOffsets[i] = (_trailOffset - i * _settings.MinTrailDistance, false);

            var offset = (initialDirection switch
            {
                MovementDirection.Left  => new Vector2(1.0f, 0.0f),
                MovementDirection.Right => new Vector2(-1.0f, 0.0f),
                MovementDirection.Up    => new Vector2(0.0f, -1.0f),
                MovementDirection.Down  => new Vector2(0.0f, 1.0f),
                _ => Vector2.Zero
            }) / _settings.TicksPerTile;

            for (int i = 0; i < _trail.Length; i++)
            {
                var position = initialPosition + offset * i;
                _trail[_trailOffset - i] = (To3D(position), _movement.SpriteFrame);
            }
        }

        int OffsetAge(int offset) => offset > _trailOffset ? _trailOffset - (offset - _trail.Length) : _trailOffset - offset;

        void Update()
        {
            var (position3d, _) = _trail[_trailOffset];
            var position = new Vector2(position3d.X, position3d.Y);

            var detector = Resolve<ICollisionManager>();
            if (_movement.Update(detector, position))
            {
                MoveLeader(_movement.Position);
                MoveFollowers();
            }
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

        Vector3 To3D(Vector2 position) => new Vector3(position, _settings.GetDepth(position.Y));
    }
}