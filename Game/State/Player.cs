using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public class Player : IPlayer
    {
        readonly CharacterInventory _tweeningInventory;

        public PartyCharacterId Id { get; set; }
        public Vector2 Position { get; set; }
        public int CombatPosition { get; set; }
        public ICharacterSheet Sheet { get; }
    }
}
