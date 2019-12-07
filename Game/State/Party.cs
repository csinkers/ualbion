using System.Collections.Generic;
using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public class Party : IParty
    {
        public const int MaxPartySize = 6;
        readonly List<Player.Player> _players = new List<Player.Player>();
        public IList<Player.Player> Players => _players;  // Max of 6

        // The current party leader (shown with a white outline on
        // health bar and slightly raised in the status bar)
        public PartyCharacterId Leader { get; set; }
        IReadOnlyCollection<IPlayer> IParty.Players => _players;  // Max of 6
        public Vector2 GetPastPosition(int index) => Vector2.Zero;
    }
}
