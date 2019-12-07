using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        IReadOnlyCollection<IPlayer> Players { get; } // Max of 6

        // The current party leader (shown with a white outline on
        // health bar and slightly raised in the status bar)
        PartyCharacterId Leader { get; }
    }
}