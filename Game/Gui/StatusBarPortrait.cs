using System;
using System.Linq;
using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class StatusBarPortrait : UiElement
    {
        const string TimerName = "StatusBarPortrait.ClickTimer";
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
            H<StatusBarPortrait, UiLeftClickEvent>((x, _) => x.OnClick()),
            H<StatusBarPortrait, TimerElapsedEvent>((x, e) => { if (e.Id == TimerName) x.OnTimer(); })
        );

        void OnClick()
        {
            if (_isClickTimerPending) // If they double-clicked...
            {
                var stateManager = Resolve<IStateManager>();
                var memberId = stateManager.State?.Party.Players.ElementAtOrDefault(_order)?.Id;
                if(memberId.HasValue)
                    Raise(new OpenCharacterInventoryEvent(memberId.Value));
                _isClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
            }
            else // For the first click, just start the double-click timer.
            {
                Raise(new StartTimerEvent(TimerName, 300, this));
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
            var template = assets.LoadString(SystemTextId.PartyPortrait_XLifeMana, settings.Gameplay.Language);
            var (text, _) = new TextFormatter(assets, settings.Gameplay.Language).Format(
                template, // %s (LP:%d, SP:%d)
                member.Apparent.GetName(settings.Gameplay.Language),
                member.Apparent.Combat.LifePoints,
                member.Apparent.Magic.SpellPoints);

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
            if (member.Apparent.PortraitId.HasValue)
                _portrait.Id = member.Apparent.PortraitId.Value;
        }

        protected override void Subscribed() => LoadSprite();
        public override Vector2 GetSize() => _portrait.GetSize() + new Vector2(0,6); // Add room for health + mana bars

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var stateManager = Resolve<IStateManager>();
            var member = stateManager.State.Party.Players.ElementAt(_order);
            var leader = stateManager.State.Party.Leader;
            var sheet = stateManager.State.GetPartyMember(member.Id);
            bool highlighted = member.Id == leader;

            int maxOrder = order;
            var portraitExtents = new Rectangle(extents.X, extents.Y + (highlighted ? 0 : 3), extents.Width, extents.Height - 6);

            maxOrder = Math.Max(maxOrder, func(_portrait, portraitExtents, order));
            maxOrder = Math.Max(maxOrder, func(_health, new Rectangle(
                    extents.X + 5,
                    extents.Y + extents.Height - 7,
                    extents.Width - 12,
                    4),
                order));

            if (sheet.Apparent.Magic.SpellPointsMax > 0)
            {
                maxOrder = Math.Max(maxOrder, func(_mana, new Rectangle(
                        extents.X + 5,
                        extents.Y + extents.Height - 4,
                        extents.Width - 12,
                        4),
                    order));
            }

            return maxOrder;
        }
    }
}
