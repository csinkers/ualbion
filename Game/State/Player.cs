using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public class Player : IPlayer
    {
        public PartyCharacterId Id { get; set; }
        public Vector2 Position { get; set; }
        public int CombatPosition { get; set; }
    }
}
