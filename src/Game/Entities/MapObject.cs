using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Game.Entities
{
    public class MapObject : Component
    {
        readonly MapSprite _sprite;
        int _frame;

        public MapObject(SpriteId id, Vector3 initialPosition, Vector2 size, bool onFloor, bool backAndForth)
        {
            _sprite = AttachChild(new MapSprite(
                id,
                DrawLayer.Underlay,
                0,
                SpriteFlags.FlipVertical |
                (onFloor
                    ? SpriteFlags.Floor | SpriteFlags.MidAligned
                    : SpriteFlags.Billboard))
            {
                Size = size,
                Position = initialPosition
            });
            _sprite.Selected += (_, args) => args.RegisterHit(this);

            On<SlowClockEvent>(e =>
            {
                if (_sprite.FrameCount == 1)
                    Exchange.Unsubscribe<SlowClockEvent>(this);

                _frame += e.Delta;
                if (backAndForth && _sprite.FrameCount > 2)
                {
                    int maxFrame = _sprite.FrameCount - 1;
                    int frame = _frame % (2 * maxFrame) - maxFrame;
                    _sprite.Frame = Math.Abs(frame);
                }
                else _sprite.Frame = _frame;
            });
        }

        public Vector3 Position { get => _sprite.Position; set => _sprite.Position = value; }
        public SpriteId SpriteId => (SpriteId)_sprite.Id;

        public override string ToString() => $"MapObjSprite {_sprite.Id} @ {_sprite.TilePosition} {_sprite.Size.X}x{_sprite.Size.Y}";

        static Vector4 GetRelativeObjectPosition(LabyrinthData labyrinth, SubObject subObject, float objectYScaling)
        {
            var offset = new Vector4(
                (float)subObject.X / 512 /*labyrinth.EffectiveWallWidth */,
                (float)subObject.Y * objectYScaling / labyrinth.WallHeight,
                (float)subObject.Z / 512 /*labyrinth.EffectiveWallWidth*/,
                0);

            return offset - new Vector4(0.5f, 0, 0.5f, 0);
        }

        public static MapObject Build(int tileX, int tileY, LabyrinthData labyrinth, SubObject subObject, TilemapRequest properties)
        {
            if (labyrinth == null) throw new ArgumentNullException(nameof(labyrinth));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (subObject == null) return null;
            if (subObject.ObjectInfoNumber >= labyrinth.Objects.Count)
            {
                ApiUtil.Assert($"Tried to build object {subObject.ObjectInfoNumber} in {labyrinth.Id}, but there are only {labyrinth.Objects.Count} objects");
                return null;
            }

            if (!(labyrinth.Objects[subObject.ObjectInfoNumber] is { } definition)) return null;
            if (definition.SpriteId.IsNone) return null;

            bool onFloor = (definition.Properties & LabyrinthObjectFlags.FloorObject) != 0;
            bool backAndForth = (definition.Properties & LabyrinthObjectFlags.Unk0) != 0;

            var offset = GetRelativeObjectPosition(labyrinth, subObject, properties.ObjectYScaling);

            // Cut down on z-fighting. TODO: Find a better solution, still happens sometimes.
            var tweak = onFloor ? subObject.ObjectInfoNumber * (offset.Y < float.Epsilon ? 0.0001f : -0.0001f) : 0;
            offset.Y += tweak;

            Vector3 pos3 =
                tileX * properties.HorizontalSpacing
                + tileY * properties.VerticalSpacing
                + new Vector3(offset.X, offset.Y, offset.Z) * properties.Scale;

            var size = 
                    new Vector2(definition.MapWidth, definition.MapHeight)
                    / new Vector2(labyrinth.EffectiveWallWidth, labyrinth.WallHeight)
                ;

            size *= new Vector2(properties.Scale.X, properties.Scale.Y);

            return new MapObject(definition.SpriteId, pos3, size, onFloor, backAndForth);
        }
    }
}
