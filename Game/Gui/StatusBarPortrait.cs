using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class StatusBarPortrait : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<StatusBarPortrait, PartyChangedEvent>((x, _) => x.LoadSprite()),
            H<StatusBarPortrait, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<StatusBarPortrait, UiBlurEvent>((x, _) =>
            {
                x._portrait.Highlighted = false;
                x.Raise(new HoverTextEvent(""));
            }),
            H<StatusBarPortrait, UiLeftClickEvent>((x,e) => x.OnClick(e)),
            H<StatusBarPortrait, TimerElapsedEvent>((x,e) => x.OnTimer())
        );

        void OnClick(UiLeftClickEvent e)
        {
            if (_isClickTimerPending)
            {
                var stateManager = Resolve<IStateManager>();
                var memberId = stateManager.State?.Party.Players.ElementAtOrDefault(_order)?.Id;
                if(memberId.HasValue)
                    Raise(new OpenCharacterInventoryEvent(memberId.Value));
                _isClickTimerPending = false;
            }
            else
            {
                Raise(new StartTimerEvent("StatusBarPortrait.ClickTimer", 300, this));
                _isClickTimerPending = true;
            }
        }
        void OnTimer()
        {
            if (!_isClickTimerPending) // They've already double-clicked
                return;

            var stateManager = Resolve<IStateManager>();
            var memberId = stateManager.State?.Party.Players.ElementAtOrDefault(_order)?.Id;
            if (memberId.HasValue)
                Raise(new SetActiveMemberEvent(memberId.Value));
            _isClickTimerPending = false;
        }

        void Hover()
        {
            _portrait.Highlighted = true;
            var stateManager = Resolve<IStateManager>();
            var memberId = stateManager.State?.Party.Players.ElementAtOrDefault(_order)?.Id;
            if (!memberId.HasValue)
                return;

            var member = stateManager.State.GetPartyMember(memberId.Value);
            var settings = Resolve<ISettings>();
            var assets = Resolve<IAssetManager>();
            var template = assets.LoadString(SystemTextId.PartyPortrait_XLifeMana, settings.Language);
            var text = new TextFormatter(assets, settings.Language).Format(
                template, // %s (LP:%d, SP:%d)
                member.GetName(settings.Language),
                member.LifePoints,
                member.Magic.SpellPoints);

            Raise(new HoverTextEvent(text.First().Text));
        }

        readonly UiSprite<SmallPortraitId> _portrait;
        readonly StatusBarHealthBar _health;
        readonly StatusBarHealthBar _mana;
        readonly int _order;
        bool _isClickTimerPending;

        public StatusBarPortrait(int order) : base(Handlers)
        {
            _order = order;
            _portrait = new UiSprite<SmallPortraitId>(SmallPortraitId.Tom);
            _health = new StatusBarHealthBar(order, true);
            _mana = new StatusBarHealthBar(order, false);
            Children.Add(_portrait);
            Children.Add(_health);
            Children.Add(_mana);
        }

        void LoadSprite()
        {
            var stateManager = Resolve<IStateManager>();
            var memberId = stateManager.State?.Party.Players.ElementAtOrDefault(_order)?.Id;
            if (!memberId.HasValue)
                return;

            var member = stateManager.State.GetPartyMember(memberId.Value);
            if (member.PortraitId.HasValue)
                _portrait.Id = member.PortraitId.Value;
        }

        protected override void Subscribed() { LoadSprite(); base.Subscribed(); }
        public override Vector2 GetSize() => _portrait.GetSize() + new Vector2(0,6); // Add room for health + mana bars

        int DoLayout(Rectangle extents, int order, Func<Rectangle, int, IUiElement, int> func)
        {
            var stateManager = Resolve<IStateManager>();
            var member = stateManager.State.Party.Players.ElementAt(_order);
            var leader = stateManager.State.Party.Leader;
            var sheet = stateManager.State.GetPartyMember(member.Id);
            bool highlighted = member.Id == leader;

            int maxOrder = order;
            var portraitExtents = new Rectangle(extents.X, extents.Y + (highlighted ? 0 : 3), extents.Width, extents.Height - 6);

            maxOrder = Math.Max(maxOrder, func(portraitExtents, order, _portrait));
            maxOrder = Math.Max(maxOrder, func(new Rectangle(
                    extents.X + 5,
                    extents.Y + extents.Height - 7,
                    extents.Width - 12,
                    4),
                order, _health));

            if (sheet.Magic.SpellPointsMax > 0)
            {
                maxOrder = Math.Max(maxOrder, func(new Rectangle(
                        extents.X + 5,
                        extents.Y + extents.Height - 4,
                        extents.Width - 12,
                        4),
                    order, _mana));
            }

            return maxOrder;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => 
            DoLayout(extents, order, (elementExtents, elementOrder, element) => element.Render(elementExtents, elementOrder, addFunc));

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int) uiPosition.X, (int) uiPosition.Y))
                return order;

            int maxOrder = DoLayout(extents, order, (elementExtents, elementOrder, element) =>
                    element.Select(uiPosition, elementExtents, elementOrder, registerHitFunc));

            registerHitFunc(order, this);
            return maxOrder;
        }
    }
}
