using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Status
{
    public static class ManaIndicatorTheme
    {
        public static ButtonFrame.ColorScheme Get(ButtonState state)
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
