using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryMiscPage : UiElement
{
    public InventoryMiscPage()
    {
        var stack = new VerticalStack(
            new Header(Base.SystemText.Inv3_Conditions),
            new Spacing(0, 64),
            new Header(Base.SystemText.Inv3_Languages),
            new Spacing(0, 23),
            new Header(Base.SystemText.Inv3_TemporarySpells),
            new Spacing(0, 45),
            new Button(Base.SystemText.Inv3_CombatPositions) // TODO: Make functional
        );
        AttachChild(stack);
    }
}