using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;

namespace UAlbion.Game.State.Player
{
    public class InventoryManager : ServiceComponent<IInventoryManager>, IInventoryManager
    {
        readonly Func<InventoryType, int, Inventory> _getInventory;
        const int MaxSlotAmount = 99; // TODO: Verify
        const int MaxGold = 32767;
        const int MaxRations = 32767;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryManager, InventoryPickupDropItemEvent>((x,e) => x.OnPickupItem(e)),
            H<InventoryManager, InventoryGiveItemEvent>((x,e) => x.OnGiveItem(e))
        );

        public IHoldable ItemInHand { get; private set; }
        public InventoryPickupDropItemEvent ReturnItemInHandEvent { get; private set; }
        public InventoryManager(Func<InventoryType, int, Inventory> getInventory) : base(Handlers) => _getInventory = getInventory;

        public InventoryAction GetInventoryAction(InventoryType type, int id, ItemSlotId slotId)
        {
            if (slotId == ItemSlotId.None)
                return InventoryAction.Nothing;

            var inventory = _getInventory(type, id);
            var itemInHand = ItemInHand;
            if (itemInHand == null)
            {
                if (slotId == ItemSlotId.Gold || slotId == ItemSlotId.Rations)
                    return InventoryAction.Pickup;

                var contents = inventory.GetSlot(slotId);
                if (contents?.Id == null)
                    return InventoryAction.Nothing;

                return InventoryAction.Pickup;
            }
            else
            {
                if (itemInHand is GoldInHand || itemInHand is RationsInHand)
                    return InventoryAction.Drop;

                if (!(itemInHand is ItemSlot))
                    throw new InvalidOperationException($"Unexpected item in hand of type: {ItemInHand.GetType()}");

                var contents = inventory.GetSlot(slotId);
                if (contents?.Id == null)
                    return InventoryAction.Drop;

                if (CanCoalesce(slotId, contents, (ItemSlot)itemInHand))
                {
                    return contents.Amount >= MaxSlotAmount
                        ? InventoryAction.NoCoalesceFullStack
                        : InventoryAction.Coalesce;
                }

                return InventoryAction.Swap;
            }
        }

        void Update(Inventory inventory) => Raise(new InventoryChangedEvent(inventory.InventoryType, inventory.InventoryId));

        void SetItemInHand(IInventory inventory, ItemSlotId slotId, IHoldable itemInHand)
        {
            ItemInHand = itemInHand;
            ReturnItemInHandEvent = 
                itemInHand == null 
                ? null 
                : new InventoryPickupDropItemEvent(inventory.InventoryType, inventory.InventoryId, slotId);
            Raise(new SetCursorEvent(ItemInHand == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
        }

        bool DoesSlotAcceptItem(ICharacterSheet sheet, ItemSlotId slotId, ItemData item)
        {
            switch (sheet.Gender)
            {
                case Gender.Male: if (!item.AllowedGender.HasFlag(GenderMask.Male)) return false; break;
                case Gender.Female: if (!item.AllowedGender.HasFlag(GenderMask.Female)) return false; break;
                case Gender.Neuter: if (!item.AllowedGender.HasFlag(GenderMask.Neutral)) return false; break;
            }

            if (!item.Class.IsAllowed(sheet.Class))
                return false;

            // if (!item.Race.IsAllowed(sheet.Race)) // Apparently never implemented in original game?
            //     return false;

            ItemData rightHandItem = null;
            if (sheet.Inventory.RightHand?.Id != null)
            {
                var assets = Resolve<IAssetManager>();
                rightHandItem = assets.LoadItem(sheet.Inventory.RightHand.Id.Value);
            }

            if (item.SlotType != slotId)
                return false;

            switch (slotId)
            {
                case ItemSlotId.LeftHand:
                {
                    if (rightHandItem != null && rightHandItem.Flags.HasFlag(ItemFlags.TwoHanded))
                        return false;
                    return true;
                }
                case ItemSlotId.Tail:
                    return
                        item.TypeId == ItemType.CloseRangeWeapon
                        && item.Flags.HasFlag(ItemFlags.TailWieldable);
                default:
                    return true;
            }
        }

        bool DoesSlotAcceptItemInHand(InventoryType type, int id, ItemSlotId slotId)
        {
            if (ItemInHand == null)
                return true;

            if(ItemInHand is GoldInHand)
                return slotId == ItemSlotId.Gold;

            if(ItemInHand is RationsInHand)
                return slotId == ItemSlotId.Rations;

            if(ItemInHand is ItemSlot itemInHand && itemInHand.Id.HasValue)
            {
                if (slotId < ItemSlotId.NormalSlotCount)
                    return true;

                if (type != InventoryType.Player)
                    return false;

                var assets = Resolve<IAssetManager>();
                var item = assets.LoadItem(itemInHand.Id.Value);
                var state = Resolve<IGameState>();
                var sheet = state.GetPartyMember((PartyCharacterId) id);
                return DoesSlotAcceptItem(sheet, slotId, item);
            }

            throw new InvalidOperationException($"Unexpected item type in hand: {ItemInHand.GetType()}");
        }

        ItemSlotId GetBestSlot(InventoryType type, int id, ItemSlotId slotId)
        {
            if (!(ItemInHand is ItemSlot itemInHand)) // gold, rations etc
                return slotId;

            if (itemInHand.Id == null)
                return ItemSlotId.None;

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(itemInHand.Id.Value);
            if (type != InventoryType.Player || !slotId.IsBodyPart()) 
                return ItemSlotId.None;

            var state = Resolve<IGameState>();
            var sheet = state.GetPartyMember((PartyCharacterId)id);
            if (DoesSlotAcceptItem(sheet, slotId, item)) return slotId;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.Head, item)) return ItemSlotId.Head;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.Neck, item)) return ItemSlotId.Neck;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.Tail, item)) return ItemSlotId.Tail;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.RightHand, item)) return ItemSlotId.RightHand;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.LeftHand, item)) return ItemSlotId.LeftHand;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.Chest, item)) return ItemSlotId.Chest;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.RightFinger, item)) return ItemSlotId.RightFinger;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.LeftFinger, item)) return ItemSlotId.LeftFinger;
            if (DoesSlotAcceptItem(sheet, ItemSlotId.Feet, item)) return ItemSlotId.Feet;

            return ItemSlotId.None;
        }

        bool CanItemBeTaken(IInventory inventory, ItemSlotId slotId)
        {
            if (slotId == ItemSlotId.Gold || slotId == ItemSlotId.Rations)
                return true;

            var itemSlot = inventory.GetSlot(slotId);
            if (itemSlot != null)
            {
                // var item = assets.LoadItem(itemSlot.Id);
                if (slotId < ItemSlotId.Slot0 && itemSlot.Flags.HasFlag(ItemSlotFlags.Cursed))
                    return false;

                // TODO: Goddess' amulet etc
            }

            return true;
        }

        void OnGiveItem(InventoryGiveItemEvent e)
        {
            var inventory = _getInventory(InventoryType.Player, (int) e.MemberId);

            if (ItemInHand is GoldInHand)
                Drop(inventory, ItemSlotId.Gold);
            if (ItemInHand is RationsInHand)
                Drop(inventory, ItemSlotId.Rations);
            if (!(ItemInHand is ItemSlot itemInHand))
                return; // Unknown or null

            if (itemInHand.Id == null)
                return;

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(itemInHand.Id.Value);

            ItemSlotId slotId = ItemSlotId.None;
            if (item.IsStackable)
            {
                for (int i = 0; i < inventory.Slots.Length; i++)
                {
                    if (inventory.Slots[i] == null)
                        continue;

                    if (inventory.Slots[i].Id == item.Id)
                        slotId = (ItemSlotId)((int)ItemSlotId.Slot0 + i);
                }
            }

            for (int i = 0; i < inventory.Slots.Length; i++)
            {
                if (inventory.Slots[i] == null)
                    slotId = (ItemSlotId)((int)ItemSlotId.Slot0 + i);
            }

            if(slotId != ItemSlotId.None)
                Drop(inventory, slotId);
        }

        void OnPickupItem(InventoryPickupDropItemEvent e)
        {
            var slotId = e.SlotId;
            if (!DoesSlotAcceptItemInHand(e.InventoryType, e.InventoryId, e.SlotId))
                slotId = GetBestSlot(e.InventoryType, e.InventoryId, slotId);

            var inventory = _getInventory(e.InventoryType, e.InventoryId);
            switch (GetInventoryAction(e.InventoryType, e.InventoryId, slotId))
            {
                case InventoryAction.Pickup:   PickupItem(e.InventoryType, e.InventoryId, slotId, e.Quantity); break;
                case InventoryAction.Drop:     Drop(inventory, slotId);          break;
                case InventoryAction.Swap:     SwapItems(inventory, slotId);     break;
                case InventoryAction.Coalesce: CoalesceItems(inventory, slotId); break;
                case InventoryAction.NoCoalesceFullStack: return;
            }
        }

        bool CanCoalesce(ItemSlotId slotId, ItemSlot slot, ItemSlot itemInHand)
        {
            if (slot.Id != itemInHand.Id) // Can't stack dissimilar items
                return false;

            if (slotId < ItemSlotId.Slot0) // Can't wield / wear stacks
                return false;

            if (slot.Id == null)
                return false;

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(slot.Id.Value);
            return item.IsStackable;
        }

        void PickupItem(InventoryType inventoryType, int inventoryId, ItemSlotId slotId, int? quantity)
        {
            var inventory = _getInventory(inventoryType, inventoryId);
            // Check if the item can be taken
            if (!CanItemBeTaken(inventory, slotId))
                return; // TODO: Message

            if (slotId == ItemSlotId.Gold)
            {
                ushort amount = (ushort)Math.Min(inventory.Gold, quantity ?? inventory.Gold);
                if (amount == 0)
                    return;

                SetItemInHand(inventory, slotId, new GoldInHand { Amount = amount });
                inventory.Gold -= amount;
                Update(inventory);
            }
            else if (slotId == ItemSlotId.Rations)
            {
                ushort amount = (ushort)Math.Min(inventory.Rations, quantity ?? inventory.Rations);
                if (amount == 0)
                    return;

                SetItemInHand(inventory, slotId, new RationsInHand { Amount = amount });
                inventory.Rations -= amount;
                Update(inventory);
            }
            else
            {
                var slot = inventory.GetSlot(slotId);
                byte amount = (byte)Math.Min(slot.Amount, quantity ?? slot.Amount);
                var itemInHand = new ItemSlot
                {
                    Id = slot.Id,
                    Amount = amount,
                    Charges = slot.Charges,
                    Enchantment = slot.Enchantment,
                    Flags = slot.Flags
                };
                SetItemInHand(inventory, slotId, itemInHand);

                slot.Amount -= amount;
                if(slot.Amount == 0)
                    inventory.SetSlot(slotId, null);
                Update(inventory);
            }
        }

        void CoalesceItems(Inventory inventory, ItemSlotId slotId)
        {
            var slot = inventory.GetSlot(slotId);
            var itemInHand = (ItemSlot)ItemInHand;
            ApiUtil.Assert(slot.Id == itemInHand.Id);
            ApiUtil.Assert(CanCoalesce(slotId, slot, itemInHand));
            ApiUtil.Assert(slot.Amount < MaxSlotAmount);

            byte amountToMove = (byte)Math.Min(itemInHand.Amount, MaxSlotAmount - slot.Amount);
            itemInHand.Amount -= amountToMove;
            slot.Amount += amountToMove;

            if (itemInHand.Amount == 0)
                SetItemInHand(inventory, slotId, null);

            Update(inventory);
        }

        void SwapItems(Inventory inventory, ItemSlotId slotId)
        {
            var slot = inventory.GetSlot(slotId);
            var itemInHand = (ItemSlot)ItemInHand;
            ApiUtil.Assert(slot.Id != itemInHand.Id);

            // Check if the item can be taken
            if (!CanItemBeTaken(inventory, slotId))
                return; // TODO: Message

            // FIXME: could take a lightweight object from player A (who is at their max carry weight), swap it with a heavy one carried by player B
            // and then close the inventory screen. The return event will fire and drop the heavier object in player A's inventory, taking them above their
            // max carry weight.
            var returnInventory = _getInventory(ReturnItemInHandEvent.InventoryType, ReturnItemInHandEvent.InventoryId);
            SetItemInHand(returnInventory, ReturnItemInHandEvent.SlotId, slot);
            inventory.SetSlot(slotId, itemInHand);
            Update(inventory);
        }

        void Drop(Inventory inventory, ItemSlotId slotId)
        {
            ApiUtil.Assert(inventory.GetSlot(slotId)?.Id == null);

            if (ItemInHand is GoldInHand gold)
            {
                inventory.Gold += gold.Amount;
                SetItemInHand(inventory, slotId, null);
            }

            if (ItemInHand is RationsInHand rations)
            {
                inventory.Rations += rations.Amount;
                SetItemInHand(inventory, slotId, null);
            }

            var itemInHand = (ItemSlot)ItemInHand;
            if (slotId >= ItemSlotId.Slot0)
            {
                inventory.Slots[slotId - ItemSlotId.Slot0] = itemInHand;
            }
            else // Body slot
            {
                inventory.SetSlot(slotId, itemInHand);
            }

            SetItemInHand(inventory, slotId, null);
            Update(inventory);
        }

        void RaiseStatusMessage(SystemTextId textId)
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var text = assets.LoadString(textId, settings.Gameplay.Language);
            Raise(new DescriptionTextEvent(text));
        }

        public bool TryChangeInventory(InventoryType inventoryType, int inventoryId, ItemId itemId, QuantityChangeOperation operation, int amount, EventContext context)
        {
            var inventory = _getInventory(inventoryType, inventoryId);
            // TODO: Ensure weight limit is not exceeded
            // TODO: Handle non-stacking items.
            int currentAmount = inventory.EnumerateAll().Where(x => x.Id == itemId).Sum(x => (int?)x.Amount) ?? 0;
            int newAmount = operation.Apply(currentAmount, amount, 0, int.MaxValue);
            int remainingDelta = newAmount - currentAmount;
            var pendingChanges = new List<(int, int)>(); // Tuples of (slot index, amount)

            for (int i = 0; i < inventory.Slots.Length && remainingDelta != 0; i++)
            {
                var slot = inventory.Slots[i];
                // Add items
                if (remainingDelta > 0) 
                {
                    if (slot?.Id != null && slot.Id != itemId)
                        continue;

                    int amountToChange = Math.Min(MaxSlotAmount - (slot?.Id == null ? 0 : slot.Amount), remainingDelta);
                    if(amountToChange > 0)
                    {
                        remainingDelta -= amountToChange;
                        pendingChanges.Add((i, amountToChange));
                    }
                }
                else if (slot.Id == itemId) // Remove items
                {
                    int amountToChange = Math.Max(-slot.Amount, remainingDelta);
                    remainingDelta -= amountToChange;
                    pendingChanges.Add((i, amountToChange));
                }
            }

            if (remainingDelta != 0) // Could not perform all the changes
                return false;

            foreach (var (slotNumber, amountToChange) in pendingChanges)
            {
                var slot = inventory.Slots[slotNumber];
                if (slot == null)
                {
                    slot = new ItemSlot();
                    inventory.Slots[slotNumber] = slot;
                }

                slot.Id ??= itemId;
                slot.Amount = (byte)(slot.Amount + amountToChange);
                if (slot.Amount == 0)
                    slot.Id = null;
            }

            Update(inventory);

            if(context.Source is EventSource.Map mapEventSource)
                ItemTransition<ItemSpriteId>.CreateTransitionFromTilePosition(Exchange, mapEventSource.X, mapEventSource.Y, itemId);

            return true;
        }

        public bool TryChangeGold(InventoryType inventoryType, int inventoryId, QuantityChangeOperation operation, int amount, EventContext context)
        {
            var inventory = _getInventory(inventoryType, inventoryId);
            // TODO: Ensure weight limit is not exceeded
            ushort newValue = (ushort)operation.Apply(inventory.Gold, amount, 0, 32767);
            inventory.Gold = newValue;
            Update(inventory);
            return true;
        }

        public bool TryChangeRations(InventoryType inventoryType, int inventoryId, QuantityChangeOperation operation, int amount, EventContext context)
        {
            var inventory = _getInventory(inventoryType, inventoryId);
            // TODO: Ensure weight limit is not exceeded
            ushort newValue = (ushort)operation.Apply(inventory.Rations, amount, 0, 32767);
            inventory.Rations = newValue;
            Update(inventory);
            return true;
        }
    }
}
