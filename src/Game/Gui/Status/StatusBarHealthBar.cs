using System.Numerics;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Status
{
    public class StatusBarHealthBar : UiElement
    {
        readonly int _order;
        readonly bool _isHealth;
        readonly ButtonFrame _frame;
        readonly UiRectangle _bar;

        public StatusBarHealthBar(int order, bool isHealth)
        {
            On<PostUpdateEvent>(e => Update());

            _order = order;
            _isHealth = isHealth;
            _bar = new UiRectangle(isHealth ? CommonColor.Green5 : CommonColor.Teal3)
            {
                MeasureSize = new Vector2(20, 2)
            };

            _frame = AttachChild(new ButtonFrame(_bar)
            {
                Theme = isHealth 
                    ? (ButtonFrame.ThemeFunction)HealthIndicatorTheme
                    : ManaIndicatorTheme,
                Padding = 0
            });
        }

        void Update()
        {
            var party = Resolve<IParty>();
            bool visible = party != null && _order < party.StatusBarOrder.Count;
            _bar.IsActive = visible;
            _frame.IsActive = visible;
            if (!visible)
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
        static ButtonColorScheme HealthIndicatorTheme(ButtonState state)
        {
            var c = new ButtonColorScheme { Alpha = 1.0f };
            switch (state)
            {
                case ButtonState.Normal:
                    c.Corners = CommonColor.Black2;
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.Black2;
                    c.Background = CommonColor.Green3;
                    break;
                default:
                    c.Corners = CommonColor.BlueGrey5;
                    c.TopLeft = CommonColor.BlueGrey5;
                    c.BottomRight = CommonColor.BlueGrey5;
                    c.Background = CommonColor.Green3;
                    break;
            }
            return c;
        }

        static ButtonColorScheme ManaIndicatorTheme(ButtonState state)
        {
            var c = new ButtonColorScheme { Alpha = 1.0f };
            switch (state)
            {
                case ButtonState.Normal:
                    c.Corners = CommonColor.Black2;
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.Black2;
                    c.Background = CommonColor.Teal1;
                    break;
                default:
                    c.Corners = CommonColor.BlueGrey5;
                    c.TopLeft = CommonColor.BlueGrey5;
                    c.BottomRight = CommonColor.BlueGrey5;
                    c.Background = CommonColor.Teal1;
                    break;
            }
            return c;
        }
    }
}
