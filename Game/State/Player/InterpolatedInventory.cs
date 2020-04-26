using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public class InterpolatedInventory : IInventory
    {
        readonly Func<IInventory> _a;
        readonly Func<IInventory> _b;
        readonly Func<float> _getLerp;

        public InterpolatedInventory(Func<IInventory> a, Func<IInventory> b, Func<float> getLerp)
        {
            _a = a;
            _b = b;
            _getLerp = getLerp;
        }

        public InventoryType InventoryType => _b().InventoryType;
        public int InventoryId => _b().InventoryId;
        public ushort Gold => (ushort)ApiUtil.Lerp(_a().Gold, _b().Gold, _getLerp());
        public ushort Rations => (ushort)ApiUtil.Lerp(_a().Rations, _b().Rations, _getLerp());
        public ItemSlot Neck => _b().Neck;
        public ItemSlot Head => _b().Head;
        public ItemSlot Tail => _b().Tail;
        public ItemSlot LeftHand => _b().LeftHand;
        public ItemSlot Chest => _b().Chest;
        public ItemSlot RightHand => _b().RightHand;
        public ItemSlot LeftFinger => _b().LeftFinger;
        public ItemSlot Feet => _b().Feet;
        public ItemSlot RightFinger => _b().RightFinger;
        public ItemSlot[] Slots => _b().Slots;
        public IEnumerable<ItemSlot> EnumerateAll() => _b().EnumerateAll();
        public IEnumerable<ItemSlot> EnumerateBodyParts() => _b().EnumerateBodyParts();
        public ItemSlot GetSlot(ItemSlotId itemSlotId) => _b().GetSlot(itemSlotId);
    }
}
