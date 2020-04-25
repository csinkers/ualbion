using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Assets
{
    public class Inventory : IInventory
    {
        readonly FileFormat _format;

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
        public Inventory(FileFormat format)
        {
            _format = format;
            Slots = new ItemSlot[(int) (format == FileFormat.PlayerInventory
                ? ItemSlotId.CharacterSlotCount
                : ItemSlotId.NormalSlotCount)];
        }

        public static Inventory SerdesChest(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, FileFormat.ChestInventory);
        public static Inventory SerdesMerchant(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, FileFormat.MerchantInventory);
        public static Inventory SerdesCharacter(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, FileFormat.PlayerInventory);
        public IEnumerable<ItemSlot> EnumerateAll() => Slots.Where(x => x != null);

        static Inventory Serdes(int n, Inventory inv, ISerializer s, FileFormat format)
        {
            inv ??= new Inventory(format);
            if (format == FileFormat.PlayerInventory)
            {
                inv.Neck = s.Meta(nameof(inv.Neck), inv.Neck, ItemSlot.Serdes);
                inv.Head = s.Meta(nameof(inv.Head), inv.Head, ItemSlot.Serdes);
                inv.Tail = s.Meta(nameof(inv.Tail), inv.Tail, ItemSlot.Serdes);
                inv.LeftHand = s.Meta(nameof(inv.LeftHand), inv.LeftHand, ItemSlot.Serdes);
                inv.Chest = s.Meta(nameof(inv.Chest), inv.Chest, ItemSlot.Serdes);
                inv.RightHand = s.Meta(nameof(inv.RightHand), inv.RightHand, ItemSlot.Serdes);
                inv.LeftFinger = s.Meta(nameof(inv.LeftFinger), inv.LeftFinger, ItemSlot.Serdes);
                inv.Feet = s.Meta(nameof(inv.Feet), inv.Feet, ItemSlot.Serdes);
                inv.RightFinger = s.Meta(nameof(inv.RightFinger), inv.RightFinger, ItemSlot.Serdes);
            }

            for (int i = 0; i < (int)ItemSlotId.NormalSlotCount; i++)
                inv.Slots[i] = s.Meta($"Slot{i}", inv.Slots[i], ItemSlot.Serdes);

            if (format == FileFormat.ChestInventory)
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
                throw new ArgumentOutOfRangeException($"Unexpected slot id: {itemSlotId}");
            return Slots[slotNumber];
        }

        public void SetSlot(ItemSlotId slotId, ItemSlot slot) => Slots[(int)slotId] = slot;

        public Inventory DeepClone()
        {
            var clone = new Inventory(_format)
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
