using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public interface IGameState
    {
        int TickCount { get; }
        IParty Party { get; }
        DateTime Time { get; }
        Func<NpcCharacterId, ICharacterSheet> GetNpc { get; }

        Func<ChestId, IChest> GetChest { get; }
        Func<MerchantId, IChest> GetMerchant { get; }
        bool Loaded { get; }
        IReadOnlyDictionary<int, int> Tickers { get; }
    }
}