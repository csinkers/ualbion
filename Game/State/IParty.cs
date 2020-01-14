using System.Collections.Generic;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        IReadOnlyList<IPlayer> Players { get; } // Max of 6
    }
}