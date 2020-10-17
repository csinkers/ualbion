using System;
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
        MapId MapId { get; }
        ICharacterSheet GetSheet(CharacterId id);
        IInventory GetInventory(InventoryId id);
        short GetTicker(TickerId id);
        bool GetSwitch(SwitchId id);
        MapChangeCollection TemporaryMapChanges { get; }
        MapChangeCollection PermanentMapChanges { get; }
    }
}
