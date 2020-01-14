using System;
using System.Numerics;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class PlayerSprite : LargeCharacterSprite<LargePartyGraphicsId>
    {
        readonly Func<(Vector2, int)> _positionFunc;
        public override string ToString() => $"NpcSprite {Id} {Animation}";

        public PlayerSprite(PartyCharacterId charId, LargePartyGraphicsId graphicsId, Func<(Vector2, int)> positionFunc) : base(graphicsId, positionFunc().Item1)
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
