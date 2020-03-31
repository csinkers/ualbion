using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory
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

            if (state == ButtonState.Hover)
                c.Background = CommonColor.White;

            return c;
        }
    }
}
