using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.State
{
    public class Party : ServiceComponent<IParty>, IParty
    {
        public const int MaxPartySize = 6;

        readonly IDictionary<PartyCharacterId, CharacterSheet> _characterSheets;
        readonly List<Player.Player> _statusBarOrder = new List<Player.Player>();
        readonly List<Player.Player> _walkOrder = new List<Player.Player>();
        readonly IReadOnlyList<Player.Player> _readOnlyStatusBarOrder;
        readonly IReadOnlyList<Player.Player> _readOnlyWalkOrder;

        public Party(IDictionary<PartyCharacterId, CharacterSheet> characterSheets, PartyCharacterId?[] statusBarOrder)
        {
            On<AddPartyMemberEvent>(e => SetLastResult(AddMember(e.PartyMemberId)));
            On<RemovePartyMemberEvent>(e => SetLastResult(RemoveMember(e.PartyMemberId)));
            On<ChangePartyGoldEvent>(e => SetLastResult(ChangePartyInventory(new Gold(),  e.Operation, e.Amount)));
            On<ChangePartyRationsEvent>(e => SetLastResult(ChangePartyInventory(new Rations(), e.Operation, e.Amount)));
            On<AddRemoveInventoryItemEvent>(e => SetLastResult(ChangePartyInventory(Resolve<IAssetManager>().LoadItem(e.ItemId), e.Operation, e.Amount)));
            On<SetPartyLeaderEvent>(e =>
            {
                Leader = e.PartyMemberId;
                Raise(new SetContextEvent(ContextType.Leader, AssetType.PartyMember, (int)e.PartyMemberId));
                Raise(e);
            });
            On<SimpleChestEvent>(e =>
            {
                SetLastResult(e.ChestType switch
                {
                    SimpleChestEvent.SimpleChestItemType.Item => ChangePartyInventory(Resolve<IAssetManager>().LoadItem(e.ItemId), QuantityChangeOperation.AddAmount, e.Amount),
                    SimpleChestEvent.SimpleChestItemType.Gold => ChangePartyInventory(new Gold(), QuantityChangeOperation.AddAmount, e.Amount),
                    SimpleChestEvent.SimpleChestItemType.Rations => ChangePartyInventory(new Rations(), QuantityChangeOperation.AddAmount, e.Amount),
                    _ => false
                });
            });
            On<InventoryTakeAllEvent>(e =>
            {
                var state = Resolve<IGameState>();
                var inventoryManager = Resolve<IInventoryManager>();
                var chestId = new InventoryId(e.ChestId);
                var chest = state.GetInventory(chestId);

                if (ChangePartyInventory(new Gold(), QuantityChangeOperation.AddAmount, chest.Gold.Amount))
                    inventoryManager.TryChangeInventory(chestId, new Gold(), QuantityChangeOperation.SubtractAmount, chest.Gold.Amount);

                if (ChangePartyInventory(new Rations(), QuantityChangeOperation.AddAmount, chest.Rations.Amount))
                    inventoryManager.TryChangeInventory(chestId, new Rations(), QuantityChangeOperation.SubtractAmount, chest.Rations.Amount);

                foreach (var slot in chest.Slots.Where(x => x?.Item != null && x.Amount > 0))
                {
                    if (ChangePartyInventory(slot.Item, QuantityChangeOperation.AddAmount, slot.Amount))
                        inventoryManager.TryChangeInventory(chestId, slot.Item, QuantityChangeOperation.SubtractAmount, slot.Amount);
                }
            });

            _characterSheets = characterSheets;
            _readOnlyStatusBarOrder = _statusBarOrder.AsReadOnly();
            _readOnlyWalkOrder = _walkOrder.AsReadOnly();

            foreach (var member in statusBarOrder)
                if (member.HasValue)
                    AddMember(member.Value);
        }

        public IPlayer this[PartyCharacterId id] => _statusBarOrder.FirstOrDefault(x => x.Id == id);
        public IReadOnlyList<IPlayer> StatusBarOrder => _readOnlyStatusBarOrder;
        public IReadOnlyList<IPlayer> WalkOrder => _readOnlyWalkOrder;
        public int TotalGold => _statusBarOrder.Sum(x => x.Effective.Inventory.Gold.Amount);
        public int GetItemCount(ItemId itemId) =>
            _statusBarOrder
            .SelectMany(x => x.Effective.Inventory.EnumerateAll())
            .Where(x => x.Item is ItemData item && item.Id == itemId)
            .Sum(x => x.Amount);

        // The current party leader (shown with a white outline on
        // health bar and slightly raised in the status bar)
        public PartyCharacterId Leader
        {
            get => _walkOrder[0].Id;
            private set
            {
                int index = _walkOrder.FindIndex(x => x.Id == value);
                if (index == -1)
                    return;

                var player = _walkOrder[index];
                _walkOrder.RemoveAt(index);
                _walkOrder.Insert(0, player);
            }
        }

        bool AddMember(PartyCharacterId id)
        {
            bool InsertMember(Player.Player newPlayer)
            {
                for (int i = 0; i < MaxPartySize; i++)
                {
                    if (_statusBarOrder.Count == i || _statusBarOrder[i].Id > id)
                    {
                        _statusBarOrder.Insert(i, newPlayer);
                        return true;
                    }
                }
                return false;
            }

            if (_statusBarOrder.Any(x => x.Id == id))
                return false;

            var player = new Player.Player(id, _characterSheets[id]);
            if (!InsertMember(player)) 
                return false;

            _walkOrder.Add(player);
            AttachChild(player);
            Raise(new PartyChangedEvent());
            return true;
        }

        bool RemoveMember(PartyCharacterId id)
        {
            var player = _statusBarOrder.FirstOrDefault(x => x.Id == id);
            if (player == null)
                return false;

            _walkOrder.Remove(player);
            _statusBarOrder.Remove(player);
            player.Remove();
            Raise(new PartyChangedEvent());
            return true;
        }

        void SetLastResult(bool result)
        {
            var em = Resolve<IEventManager>();
            em.Context.LastEventResult = result;
        }

        public void Clear()
        {
            foreach(var id in _statusBarOrder.Select(x => x.Id).ToList())
                RemoveMember(id);
        }

        bool TryEachMember(Func<IInventoryManager, PartyCharacterId, bool> func)
        {
            var inventoryManager = Resolve<IInventoryManager>();
            return _walkOrder.Any(x => func(inventoryManager, x.Id));
        }

        bool ChangePartyInventory(IContents item, QuantityChangeOperation operation, int amount) 
            => TryEachMember((im, x) =>
                im.TryChangeInventory(new InventoryId(x), item, operation, amount));
    }
}

