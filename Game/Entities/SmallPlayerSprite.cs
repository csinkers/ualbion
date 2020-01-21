using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class SmallPlayerSprite : SmallCharacterSprite<SmallPartyGraphicsId>
    {
        readonly Func<(Vector2, int)> _positionFunc;
        public override string ToString() => $"SPlayerSprite {Id} {Animation} Z: {DrawLayer.Characters2.ToDebugZCoordinate(_position.Y)}";
        public SmallPlayerSprite(PartyCharacterId charId, SmallPartyGraphicsId graphicsId, Func<(Vector2, int)> positionFunc) : base(graphicsId, positionFunc().Item1)
        {
            _positionFunc = positionFunc;
        }

        protected override void Render(RenderEvent e)
        {
            (_position, _frame) = _positionFunc();
            base.Render(e);
        }
    }
}