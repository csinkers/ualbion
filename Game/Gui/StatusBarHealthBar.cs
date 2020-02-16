using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui
{
    public class StatusBarHealthBar : UiElement
    {
        static readonly ButtonFrame.ITheme HealthTheme = new HealthIndicatorTheme();
        static readonly ButtonFrame.ITheme ManaTheme = new ManaIndicatorTheme();
        readonly int _order;
        readonly bool _isHealth;
        readonly ButtonFrame _frame;
        readonly UiRectangle _bar;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<StatusBarHealthBar, PostUpdateEvent>((x,e) => x.Update()));

        void Update()
        {
            var party = Resolve<IParty>();
            if (party == null || party.StatusBarOrder.Count <= _order)
                return;

            var playerId = party.StatusBarOrder[_order].Id;
            var highlighted = playerId == party.Leader;
            var sheet = party[playerId];
            var value = _isHealth ? sheet.Apparent.Combat.LifePoints : sheet.Apparent.Magic.SpellPoints;
            var valueMax = _isHealth ? sheet.Apparent.Combat.LifePointsMax : sheet.Apparent.Magic.SpellPointsMax;
            if (valueMax == 0) valueMax = 1;
            _bar.DrawSize = new Vector2(_bar.MeasureSize.X * value / valueMax, _bar.MeasureSize.Y);
            _frame.State = highlighted ? ButtonState.Hover : ButtonState.Normal;
        }

        public StatusBarHealthBar(int order, bool isHealth) : base(Handlers)
        {
            _order = order;
            _isHealth = isHealth;
            _bar = new UiRectangle(isHealth ? CommonColor.Green5 : CommonColor.Teal3)
            {
                MeasureSize = new Vector2(20, 2)
            };

            _frame = AttachChild(new ButtonFrame(_bar) { Theme = isHealth ? HealthTheme : ManaTheme, Padding = 0 });
        }
    }
}
