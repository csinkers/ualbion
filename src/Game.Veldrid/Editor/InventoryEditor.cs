using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Editor
{
    public class InventoryEditor : AssetEditor
    {
        readonly Inventory _inventory;

        public InventoryEditor(Inventory inventory) : base(inventory)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public override void Render()
        {
            /*
                ItemSlot[] Slots
                IContents Item
                Gold
                    Rations
                ItemData
                    ItemProxy
                ItemId? ItemId
                ushort Amount
                byte Charges
                byte Enchantment
                ItemSlotFlags Flags
            */
        }
    }
}