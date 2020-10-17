using System.Collections.Generic;
using System.Numerics;
using Xunit;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.State.Player;
using UAlbion.TestCommon;

namespace UAlbion.Game.Tests
{
    public class InventoryTests : Component
    {
        readonly ItemData _sword; // A non-stackable item
        readonly ItemData _torch; // A stackable item
        readonly Inventory _tom;
        readonly Inventory _rainer;
        readonly Dictionary<InventoryId, Inventory> _inventories;
        readonly EventExchange _exchange;
        readonly InventoryManager _im;
        readonly InventoryId _tomInv;

        public InventoryTests()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.Clear()
                .RegisterAssetType(typeof(Base.PartyMember), AssetType.PartyMember)
                .RegisterAssetType(typeof(Base.CoreSprite), AssetType.CoreGraphics)
                .RegisterAssetType(typeof(Base.Item), AssetType.Item)
                ;

            _tomInv = new InventoryId((CharacterId)Base.PartyMember.Tom);
            _sword = new ItemData(Base.Item.Sword) { TypeId = ItemType.CloseRangeWeapon };
            _torch = new ItemData(Base.Item.Torch)
            {
                TypeId = ItemType.Misc,
                Flags = ItemFlags.Stackable
            };

            _tom = new Inventory(_tomInv);
            _rainer = new Inventory(new InventoryId((CharacterId)Base.PartyMember.Rainer));
            _inventories = new Dictionary<InventoryId, Inventory>
            {
                [_tom.Id] = _tom,
                [_rainer.Id] = _rainer
            };

            _exchange = new EventExchange(new LogExchange());
            _im = new InventoryManager(x => _inventories[x]);
            var wm = new WindowManager { Window = new MockWindow(1920, 1080) };
            var cm = new MockCursorManager { Position = new Vector2(1, 1) };

            _exchange
                .Register<IInventoryManager>(_im)
                .Attach(wm)
                .Attach(cm)
                .Attach(this);
        }

        [Fact]
        public void PickupItemTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, 0));
            Assert.False(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);
        }

        [Fact]
        public void PutDownItemTest()
        {
            _tom.Slots[0].Set(_torch, 1);

            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, 0));
            Assert.False(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, (ItemSlotId)1));
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Torch, _tom.Slots[1].ItemId);
            Assert.Equal(1, _tom.Slots[1].Amount);
        }

        [Fact]
        public void CoalesceTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            _tom.Slots[1].Set(_torch, 1);

            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, 0));
            Assert.False(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, (ItemSlotId)1));
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Torch, _tom.Slots[1].ItemId);
            Assert.Equal(2, _tom.Slots[1].Amount);
        }

        [Fact]
        public void SwapTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            _tom.Slots[1].Set(_sword, 1);

            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, 0));
            Assert.False(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventorySwapEvent(_tomInv, (ItemSlotId)1));
            Assert.False(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Sword, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);
            Assert.Equal(Base.Item.Torch, _tom.Slots[1].ItemId);
            Assert.Equal(1, _tom.Slots[1].Amount);
        }

        [Fact]
        public void PickupAllTest()
        {
            _tom.Slots[0].Set(_torch, 5);

            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Raise(new InventoryPickupEvent(null, _tomInv, 0));
            Assert.Equal(Base.Item.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(5, _im.ItemInHand.Amount);
            Assert.True(_tom.Slots[0].ItemId.IsNone);
        }

        [Fact]
        public void GiveItemTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            _tom.Slots[1].Set(_torch, 1);
            _tom.Slots[2].Set(_sword, 1);
            _tom.Slots[3].Set(_sword, 1);

            Raise(new InventorySwapEvent(_tomInv, 0));
            Assert.Equal(Base.Item.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(Base.PartyMember.Rainer));
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Torch, _rainer.Slots[0].ItemId);
            Assert.Equal(1, _rainer.Slots[0].Amount);
            Assert.True(_tom.Slots[0].ItemId.IsNone);
            Assert.Equal(0, _tom.Slots[0].Amount);

            Raise(new InventorySwapEvent(_tomInv, (ItemSlotId)1));
            Assert.Equal(Base.Item.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(Base.PartyMember.Rainer));
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Torch, _rainer.Slots[0].ItemId);
            Assert.Equal(2, _rainer.Slots[0].Amount);

            Raise(new InventorySwapEvent(_tomInv, (ItemSlotId)2));
            Assert.Equal(Base.Item.Sword, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(Base.PartyMember.Rainer));
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Sword, _rainer.Slots[1].ItemId);
            Assert.Equal(1, _rainer.Slots[1].Amount);

            Raise(new InventorySwapEvent(_tomInv, (ItemSlotId)3));
            Assert.Equal(Base.Item.Sword, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(Base.PartyMember.Rainer));
            Assert.True(_im.ItemInHand.ItemId.IsNone);
            Assert.Equal(Base.Item.Sword, _rainer.Slots[1].ItemId);
            Assert.Equal(1, _rainer.Slots[1].Amount);
            Assert.Equal(Base.Item.Sword, _rainer.Slots[2].ItemId);
            Assert.Equal(1, _rainer.Slots[2].Amount);
        }

        [Fact] public void PartialPickupTest() { }
        [Fact] public void CoalesceWouldOverfillTest() { }
        [Fact] public void TakeAllFromChestTest() { }
        [Fact] public void TakeAllFromChestInsufficientRoomTest() { }
        [Fact] public void TakeGoldTest() { }
        [Fact] public void TakeRationsTest() { }
        [Fact] public void GiveGoldTest() { }
        [Fact] public void GiveRationsTest() { }
        [Fact] public void DropItemTest() { }
        [Fact] public void DropGoldTest() { }
        [Fact] public void DropRationsTest() { }
        [Fact] public void ContextMenuTest() { }
        [Fact] public void ExamineItemTest() { }
        [Fact] public void ReturnItemsTest() { }


        /*
        Hand      Slot
        (Nothing, A): Pickup A
        (A, Nothing): Drop A
        (A, A): Coalesce into slot if A stackable, else nothing.
        (A, B): Swap hand and slot

        Pickup part of a stack
        Coalesce to an almost-full stack
        Take all
        Take all without enough room
        Take gold & rations, coalesce gold & rations, give gold & rations to another party member
        Pickup events for items with a pickup script
        Activation events
        Hover & blur actions
        Right click menu items
        */
    }
}
