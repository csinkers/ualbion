using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryMiscPage : UiElement
{
    public InventoryMiscPage()
    {
        var stack = new VerticalStack(
            new Spacing(0, 1),
            new Header(Base.SystemText.Inv3_Conditions, 4),
            new Spacing(0, 63),
            new Header(Base.SystemText.Inv3_Languages, 3),
            new Spacing(0, 23),
            new Header(Base.SystemText.Inv3_TemporarySpells, 3),
            new Spacing(0, 45),
            new Button(Base.SystemText.Inv3_CombatPositions)
            {
                DoubleFrame = true
            }.OnClick(() => Raise(new ShowCombatPositionsDialogEvent()))
        );
        AttachChild(stack);
    }
}