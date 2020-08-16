using System;
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
        ICharacterSheet GetPartyMember(PartyCharacterId id);
        IInventory GetInventory(InventoryId id);
        short GetTicker(TickerId id);
        bool GetSwitch(SwitchId id);
        MapChangeCollection TemporaryMapChanges { get; }
        MapChangeCollection PermanentMapChanges { get; }
    }
}
