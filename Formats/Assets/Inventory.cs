using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class Inventory : IInventory
    {
        readonly IReadOnlyList<IReadOnlyItemSlot> _readOnlyList;

        public Inventory(InventoryId id)
        {
            Id = id;
            Slots = new ItemSlot[(int)(id.Type switch
            {
                InventoryType.Player => ItemSlotId.CharacterSlotCount,
                InventoryType.Chest => ItemSlotId.ChestSlotCount,
                _ => ItemSlotId.NormalSlotCount // e.g. merchants
            })];

            for (int i = 0; i < Slots.Length; i++)
                Slots[i] = new ItemSlot(new InventorySlotId(id, (ItemSlotId)i));

            _readOnlyList = new ReadOnlyCollection<IReadOnlyItemSlot>(Slots);
        }

        public InventoryId Id { get; }
        public ItemSlot[] Slots { get; }
        public ItemSlot Gold => Slots[(int)ItemSlotId.Gold];
        public ItemSlot Rations => Slots[(int)ItemSlotId.Rations];
        public ItemSlot Neck => Slots[(int)ItemSlotId.Neck];
        public ItemSlot Head => Slots[(int)ItemSlotId.Head];
        public ItemSlot Tail => Slots[(int)ItemSlotId.Tail];
        public ItemSlot LeftHand => Slots[(int)ItemSlotId.LeftHand];
        public ItemSlot Chest => Slots[(int)ItemSlotId.Chest];
        public ItemSlot RightHand => Slots[(int)ItemSlotId.RightHand];
        public ItemSlot LeftFinger => Slots[(int)ItemSlotId.LeftFinger];
        public ItemSlot Feet => Slots[(int)ItemSlotId.Feet];
        public ItemSlot RightFinger => Slots[(int)ItemSlotId.RightFinger];
        public static Inventory SerdesChest(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, InventoryType.Chest);
        public static Inventory SerdesMerchant(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, InventoryType.Merchant);
        public static Inventory SerdesCharacter(int n, Inventory inv, ISerializer s) => Serdes(n, inv, s, InventoryType.Player);
        public IEnumerable<ItemSlot> EnumerateAll() => Slots.Where(x => x != null);

        static Inventory Serdes(int n, Inventory inv, ISerializer s, InventoryType type)
        {
            var invId = new InventoryId(type, (ushort) n);
            void S(string name, ItemSlot existing, ItemSlotId slotId)
                => s.Meta(name, existing,
                    (_, x, s2) => ItemSlot.Serdes(new InventorySlotId(invId, slotId), x, s2));

            inv ??= new Inventory(invId);
            if (type == InventoryType.Player)
            {
                S(nameof(inv.Neck), inv.Neck, ItemSlotId.Neck);
                S(nameof(inv.Head), inv.Head, ItemSlotId.Head);
                S(nameof(inv.Tail), inv.Tail, ItemSlotId.Tail);
                S(nameof(inv.RightHand), inv.RightHand, ItemSlotId.RightHand);
                S(nameof(inv.Chest), inv.Chest, ItemSlotId.Chest);
                S(nameof(inv.LeftHand), inv.LeftHand, ItemSlotId.LeftHand);
                S(nameof(inv.RightFinger), inv.RightFinger, ItemSlotId.RightFinger);
                S(nameof(inv.Feet), inv.Feet, ItemSlotId.Feet);
                S(nameof(inv.LeftFinger), inv.LeftFinger, ItemSlotId.LeftFinger);
            }

            for (int i = 0; i < (int)ItemSlotId.NormalSlotCount; i++)
                S($"Slot{i}", inv.Slots[i], (ItemSlotId)((int)ItemSlotId.Slot0 + i));

            if (type != InventoryType.Merchant)
            {
                inv.Gold.Item ??= new Gold();
                inv.Rations.Item ??= new Rations();
            }

            // Note: Gold + Rations for players are added in the sheet loader. Merchants have no gold/rations.
            if (type == InventoryType.Chest) 
            {
                inv.Gold.Amount = s.UInt16(nameof(inv.Gold), inv.Gold.Amount);
                inv.Rations.Amount = s.UInt16(nameof(inv.Rations), inv.Rations.Amount);
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

        public Inventory DeepClone()
        {
            var clone = new Inventory(Id)
            {
                Gold = { Amount = Gold.Amount },
                Rations = { Amount = Rations.Amount }
            };

            for (int i = 0; i < Slots.Length; i++)
                clone.Slots[i] = Slots[i]?.DeepClone();

            return clone;
        }

        IReadOnlyItemSlot IInventory.Gold => Gold;
        IReadOnlyItemSlot IInventory.Rations => Rations;
        IReadOnlyItemSlot IInventory.Neck => Neck;
        IReadOnlyItemSlot IInventory.Head => Head;
        IReadOnlyItemSlot IInventory.Tail => Tail;
        IReadOnlyItemSlot IInventory.LeftHand => LeftHand;
        IReadOnlyItemSlot IInventory.Chest => Chest;
        IReadOnlyItemSlot IInventory.RightHand => RightHand;
        IReadOnlyItemSlot IInventory.LeftFinger => LeftFinger;
        IReadOnlyItemSlot IInventory.Feet => Feet;
        IReadOnlyItemSlot IInventory.RightFinger => RightFinger;
        IReadOnlyList<IReadOnlyItemSlot> IInventory.Slots => _readOnlyList;
        IEnumerable<IReadOnlyItemSlot> IInventory.EnumerateAll() => EnumerateAll();
        IEnumerable<IReadOnlyItemSlot> IInventory.EnumerateBodyParts() => EnumerateBodyParts();
        IReadOnlyItemSlot IInventory.GetSlot(ItemSlotId itemSlotId) => GetSlot(itemSlotId);
    }
}
