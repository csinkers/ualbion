using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using Xunit;

namespace UAlbion.Game.Tests
{
    public class ItemSlotTests
    {
        [Fact]
        void ItemSlotTransferTest()
        {
            var s1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0));
            var s2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0 + 1));
            var dagger = new ItemData(ItemId.Dagger);

            s1.Set(dagger, 1);
            Assert.Equal(dagger, s1.Item);
            Assert.Equal(1, s1.Amount);
            s1.Charges = 2;
            s1.Enchantment = 3;

            s2.TransferFrom(s1, null);
            Assert.Null(s1.Item);
            Assert.Equal(0, s1.Amount);
            Assert.Equal(dagger, s2.Item);
            Assert.Equal(1, s2.Amount);
            Assert.Equal(2, s2.Charges);
            Assert.Equal(3, s2.Enchantment);
        }

        [Fact]
        void ItemSlotDontCoalesceNonStackable()
        {
            var s1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0));
            var s2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0 + 1));
            var dagger = new ItemData(ItemId.Dagger);

            s1.Set(dagger, 1);
            Assert.Equal(dagger, s1.Item);
            Assert.Equal(1, s1.Amount);

            s2.TransferFrom(s1, null);
            Assert.Null(s1.Item);
            Assert.Equal(0, s1.Amount);
            Assert.Equal(dagger, s2.Item);
            Assert.Equal(1, s2.Amount);

            s1.Set(dagger, 1);
            s2.TransferFrom(s1, null);
            Assert.Equal(dagger, s1.Item);
            Assert.Equal(1, s1.Amount);
            Assert.Equal(dagger, s2.Item);
            Assert.Equal(1, s2.Amount);
        }
/*
        [Fact]
        void ItemSlotCoalescingTest()
        {
            var gold = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Gold));
            gold.Item = new Gold();
            gold.Amount = 100;

            var rations = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Rations));
            rations.Item = new Rations();
            rations.Amount = 20;
        }
*/
        [Fact]
        void ItemSlotCanCoalesceTest()
        {
            var s1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0));
            var s2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0 + 1));
            var torch = new ItemData(ItemId.Torch) { TypeId = ItemType.LightSource, Flags = ItemFlags.Stackable };

            s1.Set(torch, 1);
            Assert.Equal(torch, s1.Item);
            Assert.Equal(1, s1.Amount);

            s2.TransferFrom(s1, null);
            Assert.Null(s1.Item);
            Assert.Equal(0, s1.Amount);
            Assert.Equal(torch, s2.Item);
            Assert.Equal(1, s2.Amount);

            s1.Set(torch, 5);
            s2.TransferFrom(s1, null);
            Assert.Null(s1.Item);
            Assert.Equal(0, s1.Amount);
            Assert.Equal(torch, s2.Item);
            Assert.Equal(6, s2.Amount);

            s1.Set(torch, ItemSlot.Unlimited);
            s2.TransferFrom(s1, null);
            Assert.Equal(torch, s1.Item);
            Assert.Equal(ItemSlot.Unlimited, s1.Amount);
            Assert.Equal(torch, s2.Item);
            Assert.Equal(ItemSlot.MaxItemCount, s2.Amount);
        }
    }
}
