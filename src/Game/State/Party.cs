using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.State
{
    public class Party : ServiceComponent<IParty>, IParty
    {
        public const int MaxPartySize = 6;

        readonly IDictionary<CharacterId, CharacterSheet> _characterSheets;
        readonly List<Player.PartyMember> _statusBarOrder = new List<Player.PartyMember>();
        readonly List<Player.PartyMember> _walkOrder = new List<Player.PartyMember>();
        readonly IReadOnlyList<Player.PartyMember> _readOnlyStatusBarOrder;
        readonly IReadOnlyList<Player.PartyMember> _readOnlyWalkOrder;

        public Party(
            IDictionary<CharacterId, CharacterSheet> characterSheets,
            IList<PartyMemberId> statusBarOrder,
            Func<InventoryId, Inventory> getInventory)
        {
            if (statusBarOrder == null) throw new ArgumentNullException(nameof(statusBarOrder));
            On<AddPartyMemberEvent>(e => SetLastResult(AddMember(e.PartyMemberId)));
            On<RemovePartyMemberEvent>(e => SetLastResult(RemoveMember(e.PartyMemberId)));
            On<SetPartyLeaderEvent>(e =>
            {
                Leader = e.PartyMemberId;
                Raise(new SetContextEvent(ContextType.Leader, e.PartyMemberId));
                Raise(e);
            });

            _characterSheets = characterSheets;
            _readOnlyStatusBarOrder = _statusBarOrder.AsReadOnly();
            _readOnlyWalkOrder = _walkOrder.AsReadOnly();
            AttachChild(new PartyInventory(getInventory));

            foreach (var member in statusBarOrder)
                if (!member.IsNone)
                    AddMember(member);
        }

        [SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers", Justification = "<Pending>")]
        public IPlayer this[PartyMemberId id] => _statusBarOrder.FirstOrDefault(x => x.Id == id);
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
        public PartyMemberId Leader
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

        bool AddMember(PartyMemberId id)
        {
            bool InsertMember(Player.PartyMember newPlayer)
            {
                for (int i = 0; i < MaxPartySize; i++)
                {
                    if (_statusBarOrder.Count == i || _statusBarOrder[i].Id.Id > id.Id)
                    {
                        _statusBarOrder.Insert(i, newPlayer);
                        return true;
                    }
                }
                return false;
            }

            if (_statusBarOrder.Any(x => x.Id == id))
                return false;

            var player = new Player.PartyMember(id, _characterSheets[id]);
            if (!InsertMember(player)) 
                return false;

            _walkOrder.Add(player);
            AttachChild(player);
            Raise(new PartyChangedEvent());
            return true;
        }

        bool RemoveMember(PartyMemberId id)
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

        void SetLastResult(bool result) => Resolve<IEventManager>().LastEventResult = result;

        public void Clear()
        {
            foreach(var id in _statusBarOrder.Select(x => x.Id).ToList())
                RemoveMember(id);
        }
    }
}

