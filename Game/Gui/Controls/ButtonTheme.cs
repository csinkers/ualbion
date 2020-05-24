using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Controls
{
    public static class ButtonTheme
    {
        public static ButtonFrame.ColorScheme Default(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme { Alpha = 0.4f, Corners = CommonColor.Grey8 };
            switch (state)
            {
                case ButtonState.Normal:
                case ButtonState.ClickedBlurred:
                    c.TopLeft = CommonColor.White;
                    c.BottomRight = CommonColor.Black2;
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
                    c.Background = CommonColor.White;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return c;
        }

        public static ButtonFrame.ColorScheme SliderThumb(ButtonState state)
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

        public static ButtonFrame.ColorScheme Frameless(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme { Alpha = 0.4f, Corners = CommonColor.Grey8 };
            switch (state)
            {
                case ButtonState.Normal:
                case ButtonState.ClickedBlurred:
                    c.TopLeft = null;
                    c.BottomRight = null;
                    c.Background = null;
                    break;
                case ButtonState.Hover:
                    c.TopLeft = null;
                    c.BottomRight = null;
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
                    c.Background = CommonColor.White;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return c;
        }

        public static ButtonFrame.ColorScheme InventorySlot(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme
            {
                Alpha = 0.5f,
                Corners = CommonColor.Black2,
                TopLeft = CommonColor.Black2,
                BottomRight = CommonColor.Black2,
                Background = CommonColor.Black2
            };

            if (state == ButtonState.Hover)
                c.Background = CommonColor.White;

            return c;
        }
    }
}
