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
            })
        );

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

            Raise(new HoverTextEvent(text));
        }

        readonly UiSprite<SmallPortraitId> _portrait;
        readonly StatusBarHealthBar _health;
        readonly StatusBarHealthBar _mana;
        readonly int _order;

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

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            var stateManager = Resolve<IStateManager>();
            var member = stateManager.State.Party.Players.ElementAt(_order);
            var leader = stateManager.State.Party.Leader;
            var sheet = stateManager.State.GetPartyMember(member.Id);
            bool highlighted = member.Id == leader;

            int maxOrder = order;
            var portraitExtents = new Rectangle(extents.X, extents.Y + (highlighted ? 0 : 3), extents.Width, extents.Height - 6);

            maxOrder = Math.Max(maxOrder, _portrait.Render(portraitExtents, order, addFunc));
            maxOrder = Math.Max(maxOrder, _health.Render(new Rectangle(
                    extents.X + 5,
                    extents.Y + extents.Height - 7,
                    extents.Width - 12,
                    4),
                order, addFunc));

            if (sheet.Magic.SpellPointsMax > 0)
            {
                maxOrder = Math.Max(maxOrder, _mana.Render(new Rectangle(
                        extents.X + 5,
                        extents.Y + extents.Height - 4,
                        extents.Width - 12,
                        4),
                    order, addFunc));
            }

            return maxOrder;
        }
    }
}
