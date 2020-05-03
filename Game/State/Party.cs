using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State.Player;

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
            On<AddPartyMemberEvent>(e => e.Context.LastEventResult = AddMember(e.PartyMemberId));
            On<RemovePartyMemberEvent>(e => e.Context.LastEventResult = RemoveMember(e.PartyMemberId));
            On<SetPartyLeaderEvent>(e => { Leader = e.PartyMemberId; Raise(e); });
            On<ChangePartyGoldEvent>(e => e.Context.LastEventResult = ChangePartyGold(e.Operation, e.Amount, e.Context));
            On<ChangePartyRationsEvent>(e => e.Context.LastEventResult = ChangePartyRations(e.Operation, e.Amount, e.Context));
            On<AddRemoveInventoryItemEvent>(e => e.Context.LastEventResult = ChangePartyInventory(e.ItemId, e.Operation, e.Amount, e.Context));
            On<SimpleChestEvent>(e =>
            {
                e.Context.LastEventResult = e.ChestType switch
                {
                    SimpleChestEvent.SimpleChestItemType.Item => ChangePartyInventory(e.ItemId, QuantityChangeOperation.AddAmount, e.Amount, e.Context),
                    SimpleChestEvent.SimpleChestItemType.Gold => ChangePartyGold(QuantityChangeOperation.AddAmount, e.Amount, e.Context),
                    SimpleChestEvent.SimpleChestItemType.Rations => ChangePartyRations(QuantityChangeOperation.AddAmount, e.Amount, e.Context),
                    _ => false
                };
            });

            _characterSheets = characterSheets;
            _readOnlyStatusBarOrder = new ReadOnlyCollection<Player.Player>(_statusBarOrder);
            _readOnlyWalkOrder = new ReadOnlyCollection<Player.Player>(_walkOrder);

            foreach (var member in statusBarOrder)
                if (member.HasValue)
                    AddMember(member.Value);
        }

        public IPlayer this[PartyCharacterId id] => _statusBarOrder.FirstOrDefault(x => x.Id == id);
        public IReadOnlyList<IPlayer> StatusBarOrder => _readOnlyStatusBarOrder;
        public IReadOnlyList<IPlayer> WalkOrder => _readOnlyWalkOrder;
        public int TotalGold => _statusBarOrder.Sum(x => x.Effective.Inventory.Gold);
        public int GetItemCount(ItemId itemId) =>
            _statusBarOrder
                .SelectMany(x => x.Effective.Inventory.EnumerateAll())
                .Where(x => x.Id == itemId)
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
            player.Detach();
            Raise(new PartyChangedEvent());
            return true;
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

        bool ChangePartyInventory(ItemId itemId, QuantityChangeOperation operation, int amount, EventContext context) 
            => TryEachMember((im, x) =>
                im.TryChangeInventory(InventoryType.Player, (int)x, itemId, operation, amount, context));

        bool ChangePartyGold(QuantityChangeOperation operation, int amount, EventContext context)
            => TryEachMember((im, x) =>
                im.TryChangeGold(InventoryType.Player, (int)x, operation, amount, context));

        bool ChangePartyRations(QuantityChangeOperation operation, int amount, EventContext context)
            => TryEachMember((im, x) =>
                im.TryChangeRations(InventoryType.Player, (int)x, operation, amount, context));
    }
}

