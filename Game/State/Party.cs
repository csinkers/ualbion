using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        IReadOnlyCollection<IPlayer> Players { get; } // Max of 6
        PartyCharacterId Leader { get; } // The current party leader (shown with a white outline on health bar and slightly raised in the status bar)
    }

    public class Party : IParty
    {
        readonly List<Player> _players = new List<Player>();
        public IList<Player> Players => _players;  // Max of 6
        public PartyCharacterId Leader { get; set; } // The current party leader (shown with a white outline on health bar and slightly raised in the status bar)
        IReadOnlyCollection<IPlayer> IParty.Players => _players;  // Max of 6
    }
}
