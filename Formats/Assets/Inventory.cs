using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class Inventory : IInventory
    {
        public InventoryType InventoryType { get; }
        public int InventoryId { get; }
        public ushort Gold { get; set; }
        public ushort Rations { get; set; }
        public ItemSlot[] Slots { get; }

        public ItemSlot Neck { get => Slots[(int)ItemSlotId.Neck]; private set => Slots[(int)ItemSlotId.Neck] = value; } 
        public ItemSlot Head { get => Slots[(int)ItemSlotId.Head]; private set => Slots[(int)ItemSlotId.Head] = value; }
        public ItemSlot Tail { get => Slots[(int)ItemSlotId.Tail]; private set => Slots[(int)ItemSlotId.Tail] = value; }
        public ItemSlot LeftHand { get => Slots[(int)ItemSlotId.LeftHand]; private set => Slots[(int)ItemSlotId.LeftHand] = value; }
        public ItemSlot Chest { get => Slots[(int)ItemSlotId.Chest]; private set => Slots[(int)ItemSlotId.Chest] = value; }
        public ItemSlot RightHand { get => Slots[(int)ItemSlotId.RightHand]; private set => Slots[(int)ItemSlotId.RightHand] = value; }
        public ItemSlot LeftFinger { get => Slots[(int)ItemSlotId.LeftFinger]; private set => Slots[(int)ItemSlotId.LeftFinger] = value; }
        public ItemSlot Feet { get => Slots[(int)ItemSlotId.Feet]; private set => Slots[(int)ItemSlotId.Feet] = value; }
        public ItemSlot RightFinger { get => Slots[(int)ItemSlotId.RightFinger]; private set => Slots[(int)ItemSlotId.RightFinger] = value; }
        public Inventory(InventoryType inventoryType, int inventoryId)
        {
            InventoryType = inventoryType;
            InventoryId = inventoryId;
            Slots = new ItemSlot[(int) (inventoryType == InventoryType.Player
                ? ItemSlotId.CharacterSlotCount
                : ItemSlotId.NormalSlotCount)];
        }

        public static Inventory SerdesChest(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, InventoryType.Chest);
        public static Inventory SerdesMerchant(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, InventoryType.Merchant);
        public static Inventory SerdesCharacter(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, InventoryType.Player);
        public IEnumerable<ItemSlot> EnumerateAll() => Slots.Where(x => x != null);

        static Inventory Serdes(int n, Inventory inv, ISerializer s, InventoryType type)
        {
            inv ??= new Inventory(type, n);
            if (type == InventoryType.Player)
            {
                inv.Neck = s.Meta(nameof(inv.Neck), inv.Neck, ItemSlot.Serdes);
                inv.Head = s.Meta(nameof(inv.Head), inv.Head, ItemSlot.Serdes);
                inv.Tail = s.Meta(nameof(inv.Tail), inv.Tail, ItemSlot.Serdes);
                inv.RightHand = s.Meta(nameof(inv.RightHand), inv.RightHand, ItemSlot.Serdes);
                inv.Chest = s.Meta(nameof(inv.Chest), inv.Chest, ItemSlot.Serdes);
                inv.LeftHand = s.Meta(nameof(inv.LeftHand), inv.LeftHand, ItemSlot.Serdes);
                inv.RightFinger = s.Meta(nameof(inv.RightFinger), inv.RightFinger, ItemSlot.Serdes);
                inv.Feet = s.Meta(nameof(inv.Feet), inv.Feet, ItemSlot.Serdes);
                inv.LeftFinger = s.Meta(nameof(inv.LeftFinger), inv.LeftFinger, ItemSlot.Serdes);
            }

            for (int i = 0; i < (int)ItemSlotId.NormalSlotCount; i++)
                inv.Slots[i] = s.Meta($"Slot{i}", inv.Slots[i], ItemSlot.Serdes);

            if (type == InventoryType.Chest)
            {
                inv.Gold = s.UInt16(nameof(inv.Gold), inv.Gold);
                inv.Rations = s.UInt16(nameof(inv.Rations), inv.Rations);
            }

            return inv;
        }

        public IEnumerable<ItemSlot> EnumerateBodyParts()
        {
            if (Neck != null) yield return Neck;
            if (Head != null) yield return Head;
            if (Tail != null) yield return Tail;
            if (LeftHand != null) yield return LeftHand;
            if (Chest != null) yield return Chest;
            if (RightHand != null) yield return RightHand;
            if (LeftFinger != null) yield return LeftFinger;
            if (Feet != null) yield return Feet;
            if (RightFinger != null) yield return RightFinger;
        }

        public ItemSlot GetSlot(ItemSlotId itemSlotId)
        {
            int slotNumber = (int)itemSlotId;
            if (slotNumber < 0 || slotNumber >= Slots.Length)
                return null;
            return Slots[slotNumber];
        }

        public void SetSlot(ItemSlotId slotId, ItemSlot slot) => Slots[(int)slotId] = slot;

        public Inventory DeepClone()
        {
            var clone = new Inventory(InventoryType, InventoryId)
            {
                Gold = Gold,
                Rations = Rations,
            };

            for (int i = 0; i < Slots.Length; i++)
                clone.Slots[i] = Slots[i]?.DeepClone();

            return clone;
        }
    }
}
