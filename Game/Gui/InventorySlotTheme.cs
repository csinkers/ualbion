using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public class InventorySlotTheme : ButtonFrame.ITheme
    {
        public ButtonFrame.ColorScheme GetColors(ButtonState state)
        {
            var c = new ButtonFrame.ColorScheme
            {
                Alpha = 0.5f,
                Corners = CommonColor.Black2,
                TopLeft = CommonColor.Black2,
                BottomRight = CommonColor.Black2,
                Background = CommonColor.Black2
            };

            if (state == ButtonState.HoverPressed)
                c.Background = CommonColor.White;

            return c;
        }
    }
}