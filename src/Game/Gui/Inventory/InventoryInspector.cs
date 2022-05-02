using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Game.Events.Inventory;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryInspector : Component
{
    public InventoryInspector()
    {
        On<InventoryExamineEvent>(Examine);
    }

    void Examine(InventoryExamineEvent e)
    {
        var assets = Resolve<IAssetManager>();
        var item = assets.LoadItem(e.ItemId);
        if (item == null)
            return;
        var details = AttachChild(new InventoryDetailsDialog(item));
        details.Closed += (sender, args) => RemoveChild(details);
    }
}