using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public class SliderThumbTheme : ButtonFrame.ITheme
    {
        public ButtonFrame.ColorScheme GetColors(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme { Alpha = 1.0f, Corners = CommonColor.BlueGrey4 };
            switch (state)
            {
                case ButtonState.Normal:
                    c.TopLeft = CommonColor.BlueGrey6;
                    c.BottomRight = CommonColor.BlueGrey3;
                    c.Background = CommonColor.BlueGrey4;
                    break;
                case ButtonState.Hover:
                    c.TopLeft = CommonColor.Teal4;
                    c.BottomRight = CommonColor.Teal1;
                    c.Background = CommonColor.Teal3;
                    break;
                case ButtonState.Clicked:
                case ButtonState.ClickedBlurred:
                case ButtonState.Pressed:
                    c.TopLeft = CommonColor.Teal4;
                    c.BottomRight = CommonColor.Teal1;
                    c.Background = CommonColor.Teal3;
                    break;
                case ButtonState.HoverPressed:
                    c.TopLeft = CommonColor.Teal4;
                    c.BottomRight = CommonColor.Teal1;
                    c.Background = CommonColor.Teal3;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            return c;
        }
    }
}