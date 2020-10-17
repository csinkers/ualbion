using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls
{
    public class GroupingFrame : ButtonFrame
    {
        public GroupingFrame(IUiElement child) : base(child)
        {
            Theme = FrameTheme;
            State = ButtonState.Pressed;
        }

        public static ButtonColorScheme FrameTheme(ButtonState state)
        {
            var c = new ButtonColorScheme { Alpha = 0.5f, Corners = CommonColor.Grey8 };
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
                default: throw new ArgumentOutOfRangeException(nameof(state), state, $"Unexpected button state \"{state}\"");
            }

            return c;
        }

        public static ButtonColorScheme FrameThemeBackgroundless(ButtonState state)
        {
            var c = new ButtonColorScheme { Alpha = 0.5f, Corners = CommonColor.Grey8 };
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
                    c.Background = null;
                    break;
                case ButtonState.Clicked:
                case ButtonState.Pressed:
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.White;
                    c.Background = null;
                    break;
                case ButtonState.HoverPressed:
                    c.TopLeft = CommonColor.Black2;
                    c.BottomRight = CommonColor.White;
                    c.Background = null;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, $"Unexpected button state \"{state}\"");
            }

            return c;
        }
    }
}
