using System.Collections.Generic;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        IReadOnlyCollection<IPlayer> Players { get; }
        IPlayer PartyLeader { get; }
    }
}
