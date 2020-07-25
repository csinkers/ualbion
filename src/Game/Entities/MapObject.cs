using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class MapObject : Component
    {
        readonly MapSprite<DungeonObjectId> _sprite;

        public MapObject(DungeonObjectId id, Vector3 initialPosition, Vector2 size, bool onFloor)
        {
            _sprite = AttachChild(new MapSprite<DungeonObjectId>(
                id,
                DrawLayer.Underlay,
                SpriteKeyFlags.UseCylindrical,
                SpriteFlags.FlipVertical |
                (onFloor
                    ? SpriteFlags.Floor | SpriteFlags.MidAligned
                    : SpriteFlags.Billboard))
            {
                Size = size,
                Position = initialPosition
            });
            _sprite.Selected += (sender, args) => args.RegisterHit(this);

            On<SlowClockEvent>(e =>
            {
                if (_sprite.FrameCount == 1)
                    Exchange.Unsubscribe<SlowClockEvent>(this);
                _sprite.Frame += e.Delta;
            });
        }

        public override string ToString() => $"MapObjSprite {_sprite.Id} @ {_sprite.TilePosition} {_sprite.Size.X}x{_sprite.Size.Y}";
    }
}
