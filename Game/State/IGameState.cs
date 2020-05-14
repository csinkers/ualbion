using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.State
{
    public interface IGameState
    {
        bool Loaded { get; }
        int TickCount { get; }
        DateTime Time { get; }
        IParty Party { get; }
        MapDataId MapId { get; }
        ICharacterSheet GetNpc(NpcCharacterId id);
        IInventory GetChest(ChestId id);
        IInventory GetMerchant(MerchantId id);
        ICharacterSheet GetPartyMember(PartyCharacterId id);
        short GetTicker(int id);
        bool GetSwitch(int id);
        IList<MapChange> TemporaryMapChanges { get; }
        IList<MapChange> PermanentMapChanges { get; }
    }
}
