using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class GameState : ServiceComponent<IGameState>, IGameState
    {
        readonly Party _party;
        public DateTime Time { get; private set; }

        public IDictionary<PartyCharacterId, CharacterSheet> PartyMembers { get; } = new Dictionary<PartyCharacterId, CharacterSheet>();
        public IDictionary<NpcCharacterId, CharacterSheet> Npcs { get; } = new Dictionary<NpcCharacterId, CharacterSheet>();
        public IDictionary<ChestId, Chest> Chests { get; } = new Dictionary<ChestId, Chest>();
        public IDictionary<MerchantId, Chest> Merchants { get; } = new Dictionary<MerchantId, Chest>();

        IParty IGameState.Party => _party;
        Func<NpcCharacterId, ICharacterSheet> IGameState.GetNpc => x => Npcs[x];
        Func<ChestId, IChest> IGameState.GetChest => x => Chests[x];
        Func<MerchantId, IChest> IGameState.GetMerchant => x => Merchants[x];

        static readonly HandlerSet Handlers = new HandlerSet(
            H<GameState, NewGameEvent>((x,e) => x.NewGame()),
            H<GameState, UpdateEvent>((x, e) => { x.TickCount += e.Frames; }),
            H<GameState, SetActiveMemberEvent>((x, e) => { x._party.Leader = e.MemberId; x.Raise(e); })
        );

        public GameState() : base(Handlers)
        {
            _party = AttachChild(new Party(PartyMembers));
            AttachChild(new InventoryScreenState());
        }

        public int TickCount { get; private set; }
        public bool Loaded { get; private set; }

        void SetupTestState()
        {
            var tom = PartyMembers[PartyCharacterId.Tom];
            var inv = tom.Inventory;
            inv.Gold = 256;
            inv.Rations = 72;
            inv.Slots[0] = new ItemSlot { Amount = 1, Id = ItemId.Knife };
            inv.Slots[1] = new ItemSlot { Amount = 1, Id = ItemId.Shoes };
            inv.Slots[2] = new ItemSlot { Amount = 1, Id = ItemId.LeatherArmor };
            inv.Slots[3] = new ItemSlot { Amount = 1, Id = ItemId.Dagger };
            inv.Slots[4] = new ItemSlot { Amount = 1, Id = ItemId.LeatherCap };
            inv.Slots[5] = new ItemSlot { Amount = 12, Id = ItemId.Canister };
            inv.Slots[6] = new ItemSlot { Amount = 1, Id = ItemId.Pistol };
            inv.Slots[7] = new ItemSlot { Amount = 6, Id = ItemId.Torch };
            inv.Slots[8] = new ItemSlot { Amount = 1, Id = ItemId.Compass };
            inv.Slots[9] = new ItemSlot { Amount = 1, Id = ItemId.MonsterEye };
            inv.Slots[10] = new ItemSlot { Amount = 1, Id = ItemId.Clock };
            inv.Slots[11] = new ItemSlot { Amount = 1, Id = ItemId.RingOfWrath };
            inv.Slots[12] = new ItemSlot { Amount = 1, Id = ItemId.StrengthAmulet };
            inv.Slots[13] = new ItemSlot { Amount = 5, Id = ItemId.Torch };
            inv.Slots[14] = new ItemSlot { Amount = 1, Id = ItemId.TorchBurning };
            inv.Slots[15] = new ItemSlot { Amount = 99, Id = ItemId.TurqHealingPotion };
            inv.Slots[16] = new ItemSlot { Amount = 1, Id = ItemId.Sword, Flags = ItemSlotFlags.Broken };
            Raise(new InventoryChangedEvent(PartyCharacterId.Tom));
        }

        void NewGame()
        {
            var assets = Resolve<IAssetManager>();

            PartyMembers.Clear();
            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                PartyMembers.Add(charId, assets.LoadCharacter(AssetType.PartyMember, charId));

            Npcs.Clear();
            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                Npcs.Add(charId, assets.LoadCharacter(AssetType.Npc, charId));

            Chests.Clear();
            foreach(ChestId id in Enum.GetValues(typeof(ChestId)))
                Chests.Add(id, assets.LoadChest(id));

            Merchants.Clear();
            foreach(MerchantId id in Enum.GetValues(typeof(MerchantId)))
                Merchants.Add(id, assets.LoadMerchant(id));

            _party.Clear();

            Raise(new AddPartyMemberEvent(PartyCharacterId.Tom));
            Raise(new AddPartyMemberEvent(PartyCharacterId.Rainer));
            Raise(new AddPartyMemberEvent(PartyCharacterId.Drirr));
            Raise(new AddPartyMemberEvent(PartyCharacterId.Sira));
            Raise(new AddPartyMemberEvent(PartyCharacterId.Mellthas));
            Raise(new AddPartyMemberEvent(PartyCharacterId.Khunag));

            SetupTestState();
            Raise(new ReloadAssetsEvent()); // No need to keep character info cached once we've loaded it. New game is also a good point to clear out state.
            Raise(new PartyChangedEvent());
            Raise(new LoadMapEvent(MapDataId.Toronto2DGesamtkarteSpielbeginn));
            Raise(new StartClockEvent());
            Raise(new PartyJumpEvent(30, 75));
            Loaded = true;
        }
    }
}
