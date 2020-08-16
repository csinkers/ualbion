using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public interface IParty
    {
        int TotalGold { get; }

        int GetItemCount(ItemId item);
        IReadOnlyList<IPlayer> StatusBarOrder { get; } // Max of 6
        IReadOnlyList<IPlayer> WalkOrder { get; } // Max of 6
        PartyCharacterId Leader { get; }

        [SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers", Justification = "<Pending>")]
        IPlayer this[PartyCharacterId id] { get; }
    }
}
