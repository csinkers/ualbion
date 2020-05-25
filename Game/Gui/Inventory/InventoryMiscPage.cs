using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryMiscPage : UiElement
    {
        public InventoryMiscPage()
        {
            var stack = new VerticalStack(
                new Header(SystemTextId.Inv3_Conditions.ToId()),
                new Spacing(0, 64),
                new Header(SystemTextId.Inv3_Languages.ToId()),
                new Spacing(0, 23),
                new Header(SystemTextId.Inv3_TemporarySpells.ToId()),
                new Spacing(0, 45),
                new Button(SystemTextId.Inv3_CombatPositions.ToId()) // TODO: Make functional
            );
            AttachChild(stack);
        }
    }
}
