using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public interface IGameState
    {
        IParty Party { get; }
        IInventoryScreenState InventoryScreenState { get; }
        DateTime Time { get; }
        Func<PartyCharacterId, IPlayer> GetPartyMember { get; }
        Func<NpcCharacterId, ICharacterSheet> GetNpc { get; }

        Func<ChestId, IChest> GetChest { get; }
        Func<MerchantId, IChest> GetMerchant { get; }
    }
}