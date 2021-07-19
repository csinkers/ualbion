using System;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryScreenManager : Component
    {
        IEvent _modeEvent = new InventoryOpenEvent(PartyMemberId.None); // Should never be null.
        InventoryPage _page;
        PartyMemberId _activeCharacter;
        Action<bool> _continuation;
        InventoryScreen _screen;

        public InventoryScreenManager()
        {
            OnAsync<ChestEvent, bool>(OpenChest);
            OnAsync<DoorEvent, bool>(OpenDoor);
            OnAsync<MerchantEvent>(TalkToMerchant);
            On<InventoryOpenEvent>(e => SetDisplayedPartyMember(e.PartyMemberId));
            On<InventoryOpenPositionEvent>(e =>
            {
                var party = Resolve<IParty>();
                if (party?.StatusBarOrder.Count > e.Position)
                    SetDisplayedPartyMember(party.StatusBarOrder[e.Position].Id);
            });
            On<InventoryCloseEvent>(_ => InventoryClosed());
            On<LockOpenedEvent>(_ => LockOpened());
        }

        bool TalkToMerchant(MerchantEvent e, Action continuation)
        {
            _continuation?.Invoke(false);
            SetMode(e);
            _continuation = _ => continuation();
            return true;
        }

        bool OpenDoor(DoorEvent e, Action<bool> continuation)
        {
            Raise(new PushSceneEvent(SceneId.Inventory));
            _continuation?.Invoke(false);
            SetMode(e);
            _continuation = continuation;
            return true;
        }

        bool OpenChest(ChestEvent e, Action<bool> continuation)
        {
            Raise(new PushSceneEvent(SceneId.Inventory));
            _continuation?.Invoke(false);
            SetMode(e);
            _continuation = continuation;
            return true;
        }

        void SetMode(IEvent e)
        {
            _modeEvent = e;
            SetDisplayedPartyMember(null);

            if (e is ILockedInventoryEvent locked && locked.OpenedText != 255)
            {
                var state = Resolve<IGameState>();
                Raise(new TextEvent(state.MapId.ToMapText(), locked.OpenedText, TextLocation.NoPortrait, NpcId.None));
            }
        }

        void SetDisplayedPartyMember(PartyMemberId? member)
        {
            if (Resolve<ISceneManager>().ActiveSceneId != SceneId.Inventory)
            {
                Raise(new PushSceneEvent(SceneId.Inventory));
                Raise(new SetClearColourEvent(0, 0, 0, 1));
            }

            var party = Resolve<IParty>();
            member ??= party.Leader.Id;
            if (party.WalkOrder.All(x => x.Id != member.Value))
                member = party.Leader.Id;

            _activeCharacter = member.Value;

            Raise(new SetContextEvent(ContextType.Inventory, member.Value));
            Rebuild();
        }

        void Rebuild()
        {
            _screen?.Remove();
            var scene = Resolve<ISceneManager>().GetScene(SceneId.Inventory);
            _screen = new InventoryScreen(
                _modeEvent,
                _activeCharacter,
                () => _page,
                x => _page = x);
            scene.Add(_screen);
        }

        void LockOpened()
        {
            var em = Resolve<IEventManager>();
            em.LastEventResult = true;
            InventoryClosed();
        }

        void InventoryClosed()
        {
            _modeEvent = new InventoryOpenEvent(PartyMemberId.None);
            _screen?.Remove();
            _screen = null;
            Raise(new PopSceneEvent());

            var continuation = _continuation;
            _continuation = null;
            continuation?.Invoke(true); // TODO: Test with trapped chests / doors
        }
    }
}
