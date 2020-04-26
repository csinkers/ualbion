using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.State
{
    public interface IGameState
    {
        int TickCount { get; }
        IParty Party { get; }
        DateTime Time { get; }
        bool Loaded { get; }
        MapDataId MapId { get; }
        Func<NpcCharacterId, ICharacterSheet> GetNpc { get; }
        Func<ChestId, IInventory> GetChest { get; }
        Func<MerchantId, IInventory> GetMerchant { get; }
        Func<PartyCharacterId, ICharacterSheet> GetPartyMember { get; }
        Func<int, short> GetTicker { get; }
        Func<int, bool> GetSwitch { get; }
        IList<MapChange> TemporaryMapChanges { get; }
        IList<MapChange> PermanentMapChanges { get; }
    }
}
