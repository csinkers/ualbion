using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class StateManager : Component, IStateManager
    {
        GameState _state;
        Party _party;

        public IGameState State => _state;
        public int FrameCount { get; private set; }
        public Vector3 TileSize { get; private set; } = Vector3.One;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<StateManager, UpdateEvent>((x, e) => { x.FrameCount += e.Frames; }),
            H<StateManager, SetTileSizeEvent>((x, e) => { x.TileSize = e.TileSize; }),
            H<StateManager, SetActiveMemberEvent>((x, e) => { x._party.Leader = e.MemberId; x.Raise(e); }),
            H<StateManager, NewGameEvent>((x,e) => x.NewGame())
        );

        void SetupTestState()
        {
            var tom = _state.PartyMembers[PartyCharacterId.Tom];
            var inv = tom.Inventory;
            inv.Slots[0] = new ItemSlot { Amount = 1, Id = ItemId.Knife };
            inv.Slots[1] = new ItemSlot { Amount = 1, Id = ItemId.Shoes1 };
            inv.Slots[2] = new ItemSlot { Amount = 1, Id = ItemId.LeatherArmour };
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
        }

        void NewGame()
        {
            var assets = Resolve<IAssetManager>();
            _state = new GameState();

            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                _state.PartyMembers.Add(charId, assets.LoadCharacter(AssetType.PartyMember, charId));

            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                _state.Npcs.Add(charId, assets.LoadCharacter(AssetType.Npc, charId));

            foreach(ChestId id in Enum.GetValues(typeof(ChestId)))
                _state.Chests.Add(id, assets.LoadChest(id));

            foreach(MerchantId id in Enum.GetValues(typeof(MerchantId)))
                _state.Merchants.Add(id, assets.LoadMerchant(id));

            _party = new Party { Leader = PartyCharacterId.Tom };
            _party.Players.Add(new Player { Id = PartyCharacterId.Tom,      Position = Vector2.Zero });
            _party.Players.Add(new Player { Id = PartyCharacterId.Rainer,   Position = Vector2.Zero });
            _party.Players.Add(new Player { Id = PartyCharacterId.Drirr,    Position = Vector2.Zero });
            _party.Players.Add(new Player { Id = PartyCharacterId.Sira,     Position = Vector2.Zero });
            _party.Players.Add(new Player { Id = PartyCharacterId.Mellthas, Position = Vector2.Zero });
            _party.Players.Add(new Player { Id = PartyCharacterId.Khunag,   Position = Vector2.Zero });

            _state.Party = _party;
            SetupTestState();
            Raise(new ReloadAssetsEvent()); // No need to keep character info cached once we've loaded it. New game is also a good point to clear out state.
            Raise(new PartyChangedEvent());
            Raise(new LoadMapEvent(MapDataId.Toronto2DGesamtkarteSpielbeginn));
            Raise(new StartClockEvent());
        }

        public StateManager() : base(Handlers) { }
    }
}