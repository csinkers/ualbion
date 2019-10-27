using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public interface IGameState
    {
        IParty Party { get; }
        DateTime Time { get; }
        Func<PartyCharacterId, ICharacterSheet> GetPartyMember { get; }
        Func<NpcCharacterId, ICharacterSheet> GetNpc { get; }

        Func<ChestId, IChest> GetChest { get; }
        Func<MerchantId, IChest> GetMerchant { get; }
    }
}