using System.Collections.Generic;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        IReadOnlyCollection<IPlayer> Players { get; }
        int ActivePlayer { get; }
    }
}
