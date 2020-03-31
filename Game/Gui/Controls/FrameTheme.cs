using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Controls
{
    public class FrameTheme : ButtonFrame.ITheme
    {
        public ButtonFrame.ColorScheme GetColors(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme { Alpha = 0.5f, Corners = CommonColor.Grey8 };
            switch (state)
            {
                case ButtonState.Normal:
                case ButtonState.ClickedBlurred:
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.White;
                    c.Background = null;
                    break;
                case ButtonState.Hover:
                    c.TopLeft = CommonColor.White;
                    c.BottomRight = CommonColor.Black2;
                    c.Background = CommonColor.White;
                    break;
                case ButtonState.Clicked:
                case ButtonState.Pressed:
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.White;
                    c.Background = CommonColor.Black2;
                    break;
                case ButtonState.HoverPressed:
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.White;
                    c.Background = null;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return c;
        }
    }
}
