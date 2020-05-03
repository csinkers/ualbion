using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class SmallPlayer : Component
    {
        readonly PartyCharacterId _id;
        readonly Func<(Vector3, int)> _positionFunc;
        readonly MapSprite<SmallPartyGraphicsId> _sprite;

        public SmallPlayer(PartyCharacterId charId, SmallPartyGraphicsId graphicsId, Func<(Vector3, int)> positionFunc)
        {
            On<FastClockEvent>(e =>
            {
                //(_sprite.TilePosition, _sprite.Frame) = _positionFunc();
                var (pos, frame) = _positionFunc();
                _sprite.TilePosition = pos + new Vector3(0.0f, -1.0f, 0.0f); // TODO: Hacky, find a better way of fixing.
                _sprite.Frame = frame;
            });

            _id = charId;
            _positionFunc = positionFunc;
            _sprite = AttachChild(new MapSprite<SmallPartyGraphicsId>(
                graphicsId,
                DrawLayer.Character,
                0,
                SpriteFlags.LeftAligned));
        }

        public override string ToString() => $"SPlayer {_id}";
    }
}
