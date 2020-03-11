using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

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

        Func<ChestId, IChest> GetChest { get; }
        Func<MerchantId, IChest> GetMerchant { get; }
        Func<int, int> GetTicker { get; }
        Func<int, int> GetSwitch { get; }
    }
}
