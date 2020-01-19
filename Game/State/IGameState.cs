using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public interface IGameState
    {
        int FrameCount { get; }
        IParty Party { get; }
        DateTime Time { get; }
        Func<NpcCharacterId, ICharacterSheet> GetNpc { get; }

        Func<ChestId, IChest> GetChest { get; }
        Func<MerchantId, IChest> GetMerchant { get; }
        bool Loaded { get; }
    }
}