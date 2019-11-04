using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryChestPane : UiElement
    {
        readonly bool _isChest;

        // readonly Header _chestHeader = new Header("Chest");
        InventorySlot[] _inventory = new InventorySlot[24]; // 6x4
        Button _money;
        Button _food;
        //TODO: Button _takeAll;

        public InventoryChestPane(bool isChest)
        {
            _isChest = isChest;
        }
    }
}