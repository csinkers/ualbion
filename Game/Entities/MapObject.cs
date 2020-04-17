using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class MapObject : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapObject, SlowClockEvent>((x, e) =>
            {
                if (x._sprite.FrameCount == 1)
                    x.Exchange.Unsubscribe<SlowClockEvent>(x);
                x._sprite.Frame += e.Delta;
            }));

        readonly Vector3 _initialPosition;
        readonly MapSprite<DungeonObjectId> _sprite;

        public MapObject(DungeonObjectId id, Vector3 initialPosition, Vector2 size, bool onFloor) : base(Handlers)
        {
            _initialPosition = initialPosition;
            _sprite = AttachChild(new MapSprite<DungeonObjectId>(
                id,
                DrawLayer.Underlay,
                SpriteKeyFlags.UseCylindrical,
                SpriteFlags.FlipVertical |
                (onFloor
                    ? SpriteFlags.Floor | SpriteFlags.MidAligned
                    : SpriteFlags.Billboard)));
            _sprite.Size = size;
        }

        public override void Subscribed()
        {
            _sprite.TilePosition = _initialPosition;
            base.Subscribed();
        }

        public override string ToString() => $"MapObjSprite {_sprite.Id} @ {_sprite.TilePosition} {_sprite.Size.X}x{_sprite.Size.Y}";
    }
}
