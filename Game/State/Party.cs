using System.Collections.Generic;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        IReadOnlyCollection<IPlayer> Players { get; } // Max of 6
        IPlayer PartyLeader { get; } // The current party leader (shown with a white outline on health bar and slightly raised in the status bar)
    }
}
