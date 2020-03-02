using System.Collections.Generic;
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
        IPlayer this[PartyCharacterId id] { get; }
    }
}