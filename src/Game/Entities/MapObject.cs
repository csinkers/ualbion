using System;
using System.Numerics;
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

        public override string ToString() => $"MapObjSprite {_sprite.Id} @ {_sprite.TilePosition} {_sprite.Size.X}x{_sprite.Size.Y}";

        public static MapObject Build(int tileX, int tileY, LabyrinthObject definition, SubObject subObject, in DungeonTileMapProperties properties)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (subObject == null) throw new ArgumentNullException(nameof(subObject));

            if (definition.SpriteId.IsNone)
                return null;

            bool onFloor = (definition.Properties & LabyrinthObjectFlags.FloorObject) != 0;
            bool backAndForth = (definition.Properties & LabyrinthObjectFlags.Unk0) != 0;

            // We should probably be offsetting the main tilemap by half a tile to centre the objects
            // rather than fiddling with the object positions... will need to reevaluate when working on
            // collision detection, path-finding etc.
            var objectBias = new Vector3(-1.0f, 0, -1.0f) / 2;
                /*
                (MapId == MapId.Jirinaar3D || MapId == MapId.AltesFormergebäude || MapId == MapId.FormergebäudeNachKampfGegenArgim)
                    ? new Vector3(-1.0f, 0, -1.0f) / 2
                    : new Vector3(-1.0f, 0, -1.0f); // / 2;
                */

            var tilePosition = new Vector3(tileX, 0, tileY) + objectBias;
            var offset = new Vector4(
                subObject.X,
                subObject.Y * properties.ObjectYScaling,
                subObject.Z,
                0);

            var smidgeon = onFloor
                ? new Vector4(0,subObject.ObjectInfoNumber * (offset.Y < float.Epsilon ? 0.1f : -0.1f), 0, 0)
                : Vector4.Zero;

            Vector4 position = 
                tilePosition.X * properties.HorizontalSpacing 
                + tilePosition.Z * properties.VerticalSpacing 
                + offset 
                + smidgeon;

            return new MapObject(
                definition.SpriteId,
                new Vector3(position.X, position.Y, position.Z),
                new Vector2(definition.MapWidth, definition.MapHeight),
                onFloor, backAndForth);
        }
    }
}
