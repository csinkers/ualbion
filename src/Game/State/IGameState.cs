using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.State;

public interface IGameState
{
    bool Loaded { get; }
    int TickCount { get; }
    DateTime Time { get; }
    float PaletteBlend { get; }
    IParty Party { get; }
    MapId MapId { get; }
    ICharacterSheet GetSheet(CharacterId id);
    IInventory GetInventory(InventoryId id);
    short GetTicker(TickerId id);
    bool GetSwitch(SwitchId id);
    MapChangeCollection TemporaryMapChanges { get; }
    MapChangeCollection PermanentMapChanges { get; }
    ActiveItems ActiveItems { get; }
    IList<NpcState> Npcs { get; }
    bool IsChainDisabled(AssetId chainSource, ushort chain);
}