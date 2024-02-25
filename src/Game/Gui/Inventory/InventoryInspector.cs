using UAlbion.Game.Events.Inventory;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryInspector : GameComponent
{
    public InventoryInspector()
    {
        On<InventoryExamineEvent>(Examine);
    }

    void Examine(InventoryExamineEvent e)
    {
        var item = Assets.LoadItem(e.ItemId);
        if (item == null)
            return;
        var details = AttachChild(new InventoryDetailsDialog(item));
        details.Closed += (sender, args) => RemoveChild(details);
    }
}
