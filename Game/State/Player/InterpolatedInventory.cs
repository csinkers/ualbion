using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public class InterpolatedInventory : IInventory
    {
        readonly Func<IInventory> _b;

        public InterpolatedInventory(Func<IInventory> a, Func<IInventory> b, Func<float> getLerp)
        {
            IReadOnlyItemSlot Slot(Func<IInventory, IReadOnlyItemSlot> getSlot)
                => new InterpolatedItemSlot(() => getSlot(a()), () => getSlot(b()), getLerp);

            _b = b;
            Gold = Slot(x => x.Gold);
            Rations = Slot(x => x.Rations);
        }

        public InventoryId Id => _b().Id;
        public IReadOnlyItemSlot Gold { get; }
        public IReadOnlyItemSlot Rations { get; }
        public IReadOnlyItemSlot Neck => _b().Neck;
        public IReadOnlyItemSlot Head => _b().Head;
        public IReadOnlyItemSlot Tail => _b().Tail;
        public IReadOnlyItemSlot LeftHand => _b().LeftHand;
        public IReadOnlyItemSlot Chest => _b().Chest;
        public IReadOnlyItemSlot RightHand => _b().RightHand;
        public IReadOnlyItemSlot LeftFinger => _b().LeftFinger;
        public IReadOnlyItemSlot Feet => _b().Feet;
        public IReadOnlyItemSlot RightFinger => _b().RightFinger;
        public IReadOnlyList<IReadOnlyItemSlot> Slots => _b().Slots;
        public IEnumerable<IReadOnlyItemSlot> EnumerateAll() => _b().EnumerateAll();
        public IEnumerable<IReadOnlyItemSlot> EnumerateBodyParts() => _b().EnumerateBodyParts();
        public IReadOnlyItemSlot GetSlot(ItemSlotId itemSlotId) => _b().GetSlot(itemSlotId);
    }
}
