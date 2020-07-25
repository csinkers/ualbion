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
        readonly PartyCharacterId _id;
        readonly Func<(Vector3, int)> _positionFunc;
        readonly MapSprite<LargePartyGraphicsId> _sprite;
        public override string ToString() => $"LPlayer {_id}";

        public LargePlayer(PartyCharacterId charId, LargePartyGraphicsId graphicsId, Func<(Vector3, int)> positionFunc)
        {
            On<FastClockEvent>(e => Update());
            On<MapInitEvent>(e => Update());

            _id = charId;
            _positionFunc = positionFunc;
            _sprite = AttachChild(new MapSprite<LargePartyGraphicsId>(
                graphicsId,
                DrawLayer.Character,
                0, SpriteFlags.BottomAligned));
        }

        protected override void Subscribed()
        {
            Update();
        }

        void Update()
        {
            var (pos, frame) = _positionFunc();
            _sprite.TilePosition = pos + new Vector3(0.0f, 1.0f, 0.0f); // TODO: Hacky, find a better way of fixing.
            _sprite.Frame = frame;
        }
    }
}
