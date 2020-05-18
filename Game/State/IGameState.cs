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
        ICharacterSheet GetPartyMember(PartyCharacterId id);
        IInventory GetInventory(InventoryType type, int id);
        short GetTicker(int id);
        bool GetSwitch(int id);
        IList<MapChange> TemporaryMapChanges { get; }
        IList<MapChange> PermanentMapChanges { get; }
    }
}
