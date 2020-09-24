using System.Linq;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor
{
    public class InventoryEditor : AssetEditor
    {
        readonly ItemSlotEditor[] _slots;

        public InventoryEditor(Inventory inventory) : base(inventory)
        {
            _slots = inventory.Slots.Select(x => new ItemSlotEditor(x)).ToArray();
            foreach (var slot in _slots)
                AttachChild(slot);
        }

        public override void Render()
        {
            foreach(var slot in _slots)
                slot.Render();
        }
    }
}