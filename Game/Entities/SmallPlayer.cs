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
        static readonly HandlerSet Handlers = new HandlerSet(
            H<SmallPlayer, UpdateEvent>((x, e) =>
            {
                //(x._sprite.TilePosition, x._sprite.Frame) = x._positionFunc();
                var (pos, frame) = x._positionFunc();
                x._sprite.TilePosition = pos + new Vector3(0.0f, -1.0f, 0.0f); // TODO: Hacky, find a better way of fixing.
                x._sprite.Frame = frame;
            })
        );

        readonly PartyCharacterId _id;
        readonly Func<(Vector3, int)> _positionFunc;
        readonly MapSprite<SmallPartyGraphicsId> _sprite;
        public override string ToString() => $"SPlayer {_id}";

        public SmallPlayer(PartyCharacterId charId, SmallPartyGraphicsId graphicsId, Func<(Vector3, int)> positionFunc) : base(Handlers)
        {
            _id = charId;
            _positionFunc = positionFunc;
            _sprite = AttachChild(new MapSprite<SmallPartyGraphicsId>(graphicsId, DrawLayer.Characters1, 0, SpriteFlags.LeftAligned));
        }
    }
}
