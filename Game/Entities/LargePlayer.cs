using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class LargePlayer : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<LargePlayer, UpdateEvent>((x, e) =>
            {
                var (pos, frame) = x._positionFunc();
                x._sprite.TilePosition = pos + new Vector3(0.0f, 1.0f, 0.0f); // TODO: Hacky, find a better way of fixing.
                x._sprite.Frame = frame;
            })
        );

        readonly PartyCharacterId _id;
        readonly Func<(Vector3, int)> _positionFunc;
        readonly MapSprite<LargePartyGraphicsId> _sprite;
        public override string ToString() => $"LPlayer {_id}";

        public LargePlayer(PartyCharacterId charId, LargePartyGraphicsId graphicsId, Func<(Vector3, int)> positionFunc) : base(Handlers)
        {
            _id = charId;
            _positionFunc = positionFunc;
            _sprite = AttachChild(new MapSprite<LargePartyGraphicsId>(graphicsId, DrawLayer.Characters2 + 1, 0, SpriteFlags.BottomAligned)); // TODO: Hack, fix.
        }
    }
}
