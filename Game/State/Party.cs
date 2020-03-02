using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

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

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Party, AddPartyMemberEvent>((x,e) => x.AddMember(e.PartyMemberId)),
            H<Party, RemovePartyMemberEvent>((x,e) => x.RemoveMember(e.PartyMemberId)),
            H<Party, SetPartyLeaderEvent>((x, e) => { x.Leader = e.PartyMemberId; x.Raise(e); }),
            H<Party, ChangePartyGoldEvent>((x, e) => {}),
            H<Party, ChangePartyRationsEvent>((x, e) => {}),
            H<Party, AddRemoveInventoryItemEvent>((x, e) => {})
        );

        public Party(IDictionary<PartyCharacterId, CharacterSheet> characterSheets) : base(Handlers)
        {
            _characterSheets = characterSheets;
            _readOnlyStatusBarOrder = new ReadOnlyCollection<Player.Player>(_statusBarOrder);
            _readOnlyWalkOrder = new ReadOnlyCollection<Player.Player>(_walkOrder);
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

        void AddMember(PartyCharacterId id)
        {
            if (_statusBarOrder.Any(x => x.Id == id))
                return;

            var player = new Player.Player(id, _characterSheets[id]);
            for (int i = 0; i < MaxPartySize; i++)
            {
                if (_statusBarOrder.Count == i || _statusBarOrder[i].Id > id)
                {
                    _statusBarOrder.Insert(i, player);
                    break;
                }
            }

            _walkOrder.Add(player);
            AttachChild(player);

            Raise(new PartyChangedEvent());
        }

        void RemoveMember(PartyCharacterId id)
        {
            var player = _statusBarOrder.FirstOrDefault(x => x.Id == id);
            if (player == null)
                return;

            _walkOrder.Remove(player);
            _statusBarOrder.Remove(player);
            player.Detach();
            Raise(new PartyChangedEvent());
        }

        public void Clear()
        {
            foreach(var id in _statusBarOrder.Select(x => x.Id).ToList())
                RemoveMember(id);
        }
    }

    public interface IMovement : IComponent
    {
        (Vector3, int) GetPositionHistory(PartyCharacterId partyMember);
    }
}

