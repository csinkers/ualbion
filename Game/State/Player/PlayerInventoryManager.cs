using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.State.Player
{
    /*
     * Move apparent & effective stuff to Player
     * Update player's effective sheet on InventoryUpdated event
     * How to provide RW access to inventories to inv manager?
        * Make inv manager child of game state, de/re-register on save/load/new
     * Make InventoryManager a singleton service class
     * Merge InventoryScreenState into InventoryManager
     * Specify full inventory context for all operations
     */

    public class InventoryManager : Component
    {
        readonly Func<AssetType, int, Inventory> _getInventory;
        const int MaxSlotAmount = 99; // TODO: Verify
        const int MaxGold = 32767;
        const int MaxRations = 32767;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryManager, InventoryChangedEvent>((x, e) => { x.Update(e.InventoryType, e.InventoryId); }),
            H<InventoryManager, InventoryPickupItemEvent>((x,e) => x.OnPickupItem(e)),
            H<InventoryManager, InventoryGiveItemEvent>((x,e) => x.OnGiveItem(e)),
            H<InventoryManager, ClearInventoryItemInHandEvent>((x, e) =>
            {
                x.ItemInHand = null;
                x.ReturnItemInHandEvent = null;
            }),
            H<InventoryManager, SetInventoryItemInHandEvent>((x, e) =>
            {
                x.ItemInHand = e.ItemInHand;
                x.ReturnItemInHandEvent = new InventoryPickupItemEvent(e.InventoryType, e.Id, e.SlotId);
            })
        );

        public IHoldable ItemInHand { get; private set; }
        public InventoryPickupItemEvent ReturnItemInHandEvent { get; private set; }
        public InventoryManager(Func<AssetType, int, Inventory> getInventory) : base(Handlers) => _getInventory = getInventory;
        public override void Subscribed() => Update();

        public InventoryAction GetInventoryAction(AssetType type, int id, ItemSlotId slotId)
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

        void Update(AssetType inventoryType, int inventoryId)
        {
            Raise(new InventoryChangedEvent(inventoryType, inventoryId));
            Raise(new SetCursorEvent(ItemInHand == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
        }

        bool DoesSlotAcceptItem(ItemSlotId slotId, ItemData item)
        {
            switch (_base.Gender)
            {
                case Gender.Male: if (!item.AllowedGender.HasFlag(GenderMask.Male)) return false; break;
                case Gender.Female: if (!item.AllowedGender.HasFlag(GenderMask.Female)) return false; break;
                case Gender.Neuter: if (!item.AllowedGender.HasFlag(GenderMask.Neutral)) return false; break;
            }

            if (!item.Class.IsAllowed(_base.Class))
                return false;

            // if (!item.Race.IsAllowed(_base.Race)) // Apparently never implemented in original game?
            //     return false;

            ItemData rightHandItem = null;
            if (inventory.RightHand?.Id != null)
            {
                var assets = Resolve<IAssetManager>();
                rightHandItem = assets.LoadItem(inventory.RightHand.Id.Value);
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

        bool DoesSlotAcceptItemInHand(AssetType type, int id, ItemSlotId slotId)
        {
            if (ItemInHand == null)
                return true;

            if(ItemInHand is GoldInHand)
                return slotId == ItemSlotId.Gold;

            if(ItemInHand is RationsInHand)
                return slotId == ItemSlotId.Rations;

            if(ItemInHand is ItemSlot itemInHand && itemInHand.Id.HasValue)
            {
                if (slotId >= ItemSlotId.Slot0)
                    return true;

                var assets = Resolve<IAssetManager>();
                var item = assets.LoadItem(itemInHand.Id.Value);
                return DoesSlotAcceptItem(slotId, item);
            }

            throw new InvalidOperationException($"Unexpected item type in hand: {ItemInHand.GetType()}");
        }

        ItemSlotId GetBestSlot(ItemSlotId slotId)
        {
            if (!(ItemInHand is ItemSlot itemInHand)) // gold, rations etc
                return slotId;

            if (itemInHand.Id == null)
                return ItemSlotId.None;

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(itemInHand.Id.Value);
            if (slotId < ItemSlotId.Slot0)
            {
                if (DoesSlotAcceptItem(slotId, item)) return slotId;
                if (DoesSlotAcceptItem(ItemSlotId.Head, item)) return ItemSlotId.Head;
                if (DoesSlotAcceptItem(ItemSlotId.Neck, item)) return ItemSlotId.Neck;
                if (DoesSlotAcceptItem(ItemSlotId.Tail, item)) return ItemSlotId.Tail;
                if (DoesSlotAcceptItem(ItemSlotId.RightHand, item)) return ItemSlotId.RightHand;
                if (DoesSlotAcceptItem(ItemSlotId.LeftHand, item)) return ItemSlotId.LeftHand;
                if (DoesSlotAcceptItem(ItemSlotId.Chest, item)) return ItemSlotId.Chest;
                if (DoesSlotAcceptItem(ItemSlotId.RightFinger, item)) return ItemSlotId.RightFinger;
                if (DoesSlotAcceptItem(ItemSlotId.LeftFinger, item)) return ItemSlotId.LeftFinger;
                if (DoesSlotAcceptItem(ItemSlotId.Feet, item)) return ItemSlotId.Feet;
            }

            return ItemSlotId.None;
        }

        bool CanItemBeTaken(ItemSlotId slotId)
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
            if (ItemInHand is GoldInHand)
                Drop(ItemSlotId.Gold);
            if (ItemInHand is RationsInHand)
                Drop(ItemSlotId.Rations);
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
                Drop(slotId);

            Update();
        }

        void OnPickupItem(InventoryPickupItemEvent e)
        {
            var slotId = e.SlotId;
            if (!DoesSlotAcceptItemInHand(e.SlotId))
                slotId = GetBestSlot(slotId);

            switch(GetInventoryAction(slotId))
            {
                case InventoryAction.Pickup:   PickupItem(slotId, e.Quantity); break;
                case InventoryAction.Drop:     Drop(slotId);          break;
                case InventoryAction.Swap:     SwapItems(slotId);     break;
                case InventoryAction.Coalesce: CoalesceItems(slotId); break;
                case InventoryAction.NoCoalesceFullStack: return;
            }

            Update();
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

        void PickupItem(ItemSlotId slotId, int? quantity)
        {
            // Check if the item can be taken
            if (!CanItemBeTaken(slotId))
                return; // TODO: Message

            if (slotId == ItemSlotId.Gold)
            {
                ushort amount = (ushort)Math.Min(inventory.Gold, quantity ?? inventory.Gold);
                if (amount == 0)
                    return;

                Raise(new SetInventoryItemInHandEvent(new GoldInHand { Amount = amount }, _id, slotId));
            }
            else if (slotId == ItemSlotId.Rations)
            {
                ushort amount = (ushort)Math.Min(inventory.Rations, quantity ?? inventory.Rations);
                if (amount == 0)
                    return;

                Raise(new SetInventoryItemInHandEvent(new RationsInHand { Amount = amount }, _id, slotId));
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
                Raise(new SetInventoryItemInHandEvent(itemInHand, _id, slotId));

                slot.Amount -= amount;
                if(slot.Amount == 0)
                    inventory.SetSlot(slotId, null);
            }
        }

        void CoalesceItems(ItemSlotId slotId)
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
                Raise(new ClearInventoryItemInHandEvent());
        }

        void SwapItems(Inventory inventory, ItemSlotId slotId)
        {
            var slot = inventory.GetSlot(slotId);
            var itemInHand = (ItemSlot)ItemInHand;
            ApiUtil.Assert(slot.Id != itemInHand.Id);

            // Check if the item can be taken
            if (!CanItemBeTaken(slotId))
                return; // TODO: Message

            // FIXME: could take a lightweight object from player A (who is at their max carry weight), swap it with a heavy one carried by player B
            // and then close the inventory screen. The return event will fire and drop the heavier object in player A's inventory, taking them above their
            // max carry weight.
            Raise(new SetInventoryItemInHandEvent(slot, ReturnItemInHandEvent.InventoryType, ReturnItemInHandEvent.InventoryId, ReturnItemInHandEvent.SlotId));
            inventory.SetSlot(slotId, itemInHand);
        }

        void Drop(Inventory inventory, ItemSlotId slotId)
        {
            ApiUtil.Assert(inventory.GetSlot(slotId)?.Id == null);

            if (ItemInHand is GoldInHand gold)
            {
                inventory.Gold += gold.Amount;
                Raise(new ClearInventoryItemInHandEvent());
            }

            if (ItemInHand is RationsInHand rations)
            {
                inventory.Rations += rations.Amount;
                Raise(new ClearInventoryItemInHandEvent());
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

            Raise(new ClearInventoryItemInHandEvent());
        }

        void RaiseStatusMessage(SystemTextId textId)
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var text = assets.LoadString(textId, settings.Gameplay.Language);
            Raise(new DescriptionTextEvent(text));
        }

        public bool TryChangeInventory(ItemId itemId, QuantityChangeOperation operation, int amount, EventContext context)
        {
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

            Update();

            if(context.Source is EventSource.Map mapEventSource)
            {
                var assets = Resolve<IAssetManager>();
                var scene = Resolve<ISceneManager>()?.ActiveScene;
                var map = Resolve<IMapManager>()?.Current;
                var window = Resolve<IWindowManager>();

                if (scene == null || map == null)
                    return true;

                var worldPosition = new Vector3(mapEventSource.X, mapEventSource.Y, 0) * map.TileSize;
                var normPosition = scene.Camera.ProjectWorldToNorm(worldPosition);
                var destPosition = window.UiToNorm(23, 204); // Tom's portrait, hardcoded for now.

                var item = assets.LoadItem(itemId);
                var icon = assets.LoadTexture(item.Icon).GetSubImageDetails((int)item.Icon);
                var size = window.UiToNormRelative(icon.Size);

                var transition = new ItemTransition<ItemSpriteId>(
                    item.Icon, (int)item.Icon,
                    new Vector2(normPosition.X, normPosition.Y),
                    destPosition,
                    0.3f, size);

                Exchange.Attach(transition); // No need to attach as child as transitions clean themselves up.
            }

            return true;
        }

        public bool TryChangeGold(QuantityChangeOperation operation, int amount, EventContext context)
        {
            // TODO: Ensure weight limit is not exceeded
            ushort newValue = (ushort)operation.Apply(inventory.Gold, amount, 0, 32767);
            inventory.Gold = newValue;
            Update();
            return true;
        }

        public bool TryChangeRations(QuantityChangeOperation operation, int amount, EventContext context)
        {
            // TODO: Ensure weight limit is not exceeded
            ushort newValue = (ushort)operation.Apply(inventory.Rations, amount, 0, 32767);
            inventory.Rations = newValue;
            Update();
            return true;
        }
    }
}
