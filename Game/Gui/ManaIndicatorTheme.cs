using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public class ManaIndicatorTheme : ButtonFrame.ITheme
    {
        public ButtonFrame.ColorScheme GetColors(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme { Alpha = 1.0f };
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