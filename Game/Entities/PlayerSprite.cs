using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class PlayerSprite : LargeCharacterSprite<LargePartyGraphicsId>
    {
        public override string ToString() => $"NpcSprite {Id} {Animation}";

        public PlayerSprite(LargePartyGraphicsId id, Vector2 position) : base(id, position)
        {
        }
    }
}
