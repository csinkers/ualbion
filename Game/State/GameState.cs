using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class GameState : IGameState
    {
        public IParty Party { get; set; }
        public DateTime Time { get; set; }
        public InventoryScreenState InventoryScreenState { get; } = new InventoryScreenState();

        public IDictionary<PartyCharacterId, CharacterSheet> PartyMembers { get; } = new Dictionary<PartyCharacterId, CharacterSheet>();
        public IDictionary<NpcCharacterId, CharacterSheet> Npcs { get; } = new Dictionary<NpcCharacterId, CharacterSheet>();
        public IDictionary<ChestId, Chest> Chests { get; } = new Dictionary<ChestId, Chest>();
        public IDictionary<MerchantId, Chest> Merchants { get; } = new Dictionary<MerchantId, Chest>();

        IInventoryScreenState IGameState.InventoryScreenState => InventoryScreenState;
        Func<PartyCharacterId, IPlayer> IGameState.GetPartyMember => id => Party.Players.Single(x => x.Id == id);
        Func<NpcCharacterId, ICharacterSheet> IGameState.GetNpc => x => Npcs[x];
        Func<ChestId, IChest> IGameState.GetChest => x => Chests[x];
        Func<MerchantId, IChest> IGameState.GetMerchant => x => Merchants[x];
    }
}