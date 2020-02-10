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
            H<LargePlayer, UpdateEvent>((x, e) => (x._sprite.TilePosition, x._sprite.Frame) = x._positionFunc())
        );

        readonly PartyCharacterId _id;
        readonly Func<(Vector3, int)> _positionFunc;
        readonly MapSprite<LargePartyGraphicsId> _sprite;
        public override string ToString() => $"LPlayer {_id}";

        public LargePlayer(PartyCharacterId charId, LargePartyGraphicsId graphicsId, Func<(Vector3, int)> positionFunc) : base(Handlers)
        {
            _id = charId;
            _positionFunc = positionFunc;
            _sprite = new MapSprite<LargePartyGraphicsId>(graphicsId, DrawLayer.Characters1, 0, SpriteFlags.BottomAligned);
            Children.Add(_sprite);
        }
    }
}