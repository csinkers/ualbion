using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.State.Player;
using Xunit;

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

        public InventoryTests()
        {
            _sword = new ItemData(ItemId.Sword) { TypeId = ItemType.CloseRangeWeapon };
            _torch = new ItemData(ItemId.Torch)
            {
                TypeId = ItemType.Misc
            };

            _tom = new Inventory(new InventoryId(PartyCharacterId.Tom));
            _rainer = new Inventory(new InventoryId(PartyCharacterId.Rainer));
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
            Assert.Null(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, 0));
            Assert.NotNull(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);
        }

        [Fact]
        public void PutDownItemTest()
        {
            _tom.Slots[0].Set(_torch, 1);

            Assert.Null(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, 0));
            Assert.NotNull(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, (ItemSlotId)1));
            Assert.Null(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Torch, _tom.Slots[1].ItemId);
            Assert.Equal(1, _tom.Slots[1].Amount);
        }

        [Fact]
        public void CoalesceTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            _tom.Slots[1].Set(_torch, 1);

            Assert.Null(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, 0));
            Assert.NotNull(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, (ItemSlotId)1));
            Assert.Null(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Torch, _tom.Slots[1].ItemId);
            Assert.Equal(2, _tom.Slots[1].Amount);
        }

        [Fact]
        public void SwapTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            _tom.Slots[1].Set(_sword, 1);

            Assert.Null(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, 0));
            Assert.NotNull(_im.ItemInHand.ItemId);
            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, (ItemSlotId)1));
            Assert.NotNull(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Sword, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);
            Assert.Equal(ItemId.Torch, _tom.Slots[1].ItemId);
            Assert.Equal(1, _tom.Slots[1].Amount);
        }

        [Fact]
        public void PickupAllTest()
        {
            _tom.Slots[0].Set(_torch, 5);

            Assert.Null(_im.ItemInHand.ItemId);
            Raise(new InventoryPickupAllEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, 0));
            Assert.Equal(ItemId.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(5, _im.ItemInHand.Amount);
            Assert.Null(_tom.Slots[0].ItemId);
        }

        [Fact]
        public void GiveItemTest()
        {
            _tom.Slots[0].Set(_torch, 1);
            _tom.Slots[1].Set(_torch, 1);
            _tom.Slots[2].Set(_sword, 1);
            _tom.Slots[3].Set(_sword, 1);

            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, 0));
            Assert.Equal(ItemId.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(PartyCharacterId.Rainer));
            Assert.Null(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Torch, _rainer.Slots[0].ItemId);
            Assert.Equal(1, _rainer.Slots[0].Amount);
            Assert.Null(_tom.Slots[0].ItemId);
            Assert.Equal(0, _tom.Slots[0].Amount);

            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, (ItemSlotId)1));
            Assert.Equal(ItemId.Torch, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(PartyCharacterId.Rainer));
            Assert.Null(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Torch, _rainer.Slots[0].ItemId);
            Assert.Equal(2, _rainer.Slots[0].Amount);

            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, (ItemSlotId)2));
            Assert.Equal(ItemId.Sword, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(PartyCharacterId.Rainer));
            Assert.Null(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Sword, _rainer.Slots[1].ItemId);
            Assert.Equal(1, _rainer.Slots[1].Amount);

            Raise(new InventorySwapEvent(InventoryType.Player, (ushort)PartyCharacterId.Tom, (ItemSlotId)3));
            Assert.Equal(ItemId.Sword, _im.ItemInHand.ItemId);
            Assert.Equal(1, _im.ItemInHand.Amount);

            Raise(new InventoryGiveItemEvent(PartyCharacterId.Rainer));
            Assert.Null(_im.ItemInHand.ItemId);
            Assert.Equal(ItemId.Sword, _rainer.Slots[1].ItemId);
            Assert.Equal(1, _rainer.Slots[1].Amount);
            Assert.Equal(ItemId.Sword, _rainer.Slots[2].ItemId);
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
