using Xunit;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Tests;

public class ItemSlotTests
{
    public ItemSlotTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            ;
    }

    [Fact]
    void ItemSlotTransferTest()
    {
        var s1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0));
        var s2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0 + 1));
        var dagger = new ItemData(Base.Item.Dagger);

        s1.Set(Base.Item.Dagger, 1);
        Assert.Equal(Base.Item.Dagger, s1.Item);
        Assert.Equal(1, s1.Amount);
        s1.Charges = 2;
        s1.Enchantment = 3;

        s2.TransferFrom(s1, null, GetItem);
        Assert.True(s1.Item.IsNone);
        Assert.Equal(0, s1.Amount);
        Assert.Equal(Base.Item.Dagger, s2.Item);
        Assert.Equal(1, s2.Amount);
        Assert.Equal(2, s2.Charges);
        Assert.Equal(3, s2.Enchantment);
        return;

        ItemData GetItem(ItemId id)
        {
            if (id != Base.Item.Dagger)
                throw new AssetNotFoundException("This test only includes the dagger object", id);
            return dagger;
        }
    }

    [Fact]
    void ItemSlotDontCoalesceNonStackable()
    {
        var s1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0));
        var s2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.Slot0 + 1));
        var dagger = new ItemData(Base.Item.Dagger);

        s1.Set(Base.Item.Dagger, 1);
        Assert.Equal(Base.Item.Dagger, s1.Item);
        Assert.Equal(1, s1.Amount);

        s2.TransferFrom(s1, null, GetItem);
        Assert.True(s1.Item.IsNone);
        Assert.Equal(0, s1.Amount);
        Assert.Equal(Base.Item.Dagger, s2.Item);
        Assert.Equal(1, s2.Amount);

        s1.Set(Base.Item.Dagger, 1);
        s2.TransferFrom(s1, null, GetItem);
        Assert.Equal(Base.Item.Dagger, s1.Item);
        Assert.Equal(1, s1.Amount);
        Assert.Equal(Base.Item.Dagger, s2.Item);
        Assert.Equal(1, s2.Amount);
        return;

        ItemData GetItem(ItemId id)
        {
            if (id != Base.Item.Dagger)
                throw new AssetNotFoundException("This test only includes the dagger object", id);
            return dagger;
        }
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
        var torch = new ItemData(Base.Item.Torch) { TypeId = ItemType.LightSource, Flags = ItemFlags.Stackable };

        s1.Set(Base.Item.Torch, 1);
        Assert.Equal(Base.Item.Torch, s1.Item);
        Assert.Equal(1, s1.Amount);

        s2.TransferFrom(s1, null, GetItem);
        Assert.True(s1.Item.IsNone);
        Assert.Equal(0, s1.Amount);
        Assert.Equal(Base.Item.Torch, s2.Item);
        Assert.Equal(1, s2.Amount);

        s1.Set(Base.Item.Torch, 5);
        s2.TransferFrom(s1, null, GetItem);
        Assert.True(s1.Item.IsNone);
        Assert.Equal(0, s1.Amount);
        Assert.Equal(Base.Item.Torch, s2.Item);
        Assert.Equal(6, s2.Amount);

        s1.Set(Base.Item.Torch, ItemSlot.Unlimited);
        s2.TransferFrom(s1, null, GetItem);
        Assert.Equal(Base.Item.Torch, s1.Item);
        Assert.Equal(ItemSlot.Unlimited, s1.Amount);
        Assert.Equal(Base.Item.Torch, s2.Item);
        Assert.Equal(ItemSlot.MaxItemCount, s2.Amount);
        return;

        ItemData GetItem(ItemId id)
        {
            if (id != Base.Item.Torch)
                throw new AssetNotFoundException("This test only includes the torch object", id);
            return torch;
        }
    }
}