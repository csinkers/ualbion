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
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Text;

namespace UAlbion.Game.State.Player
{
    public class InventoryManager : ServiceComponent<IInventoryManager>, IInventoryManager
    {
        readonly Func<InventoryId, Inventory> _getInventory;
        readonly ItemSlot _hand = new ItemSlot(new InventorySlotId(InventoryType.Cursor, 0, ItemSlotId.None));
        const int MaxSlotAmount = 99; // TODO: Verify
        const int MaxGold = 32767;
        const int MaxRations = 32767;

        ItemSlot GetSlot(InventorySlotId id) => _getInventory(id.Inventory).GetSlot(id.Slot);
        public ReadOnlyItemSlot ItemInHand { get; }
        public InventoryMode ActiveMode { get; private set; }
        public InventoryPickupDropEvent ReturnItemInHandEvent { get; private set; }
        public InventoryManager(Func<InventoryId, Inventory> getInventory)
        {
            On<InventoryPickupDropEvent>(OnPickupItem);
            On<InventoryPickupAllEvent>(OnPickupItem);
            On<InventoryGiveItemEvent>(OnGiveItem);
            _getInventory = getInventory;
            ItemInHand = new ReadOnlyItemSlot(_hand);
        }

        public InventoryAction GetInventoryAction(InventorySlotId slotId)
        {
            var slot = GetSlot(slotId);
            if (slot.Id.Slot == ItemSlotId.None)
                return InventoryAction.Nothing;

            switch (_hand.Item, slot.Item)
            {
                case (null, null): return InventoryAction.Nothing;
                case (null, _): return InventoryAction.Pickup;

                case (Gold _, Gold _):
                case (Rations _, Rations _): return InventoryAction.Coalesce;

                case (ItemData _, null): return InventoryAction.PutDown;
                case (ItemData _, ItemData _) when slot.CanCoalesce(_hand):
                    return slot.Amount >= MaxSlotAmount
                        ? InventoryAction.NoCoalesceFullStack
                        : InventoryAction.Coalesce;

                case (ItemData _, ItemData _):
                    return InventoryAction.Swap;

                default:
                    return InventoryAction.Nothing;
            }
        }

        void Update(InventoryId id) => Raise(new InventoryChangedEvent(id.Type, id.Id));

        void PickupItem(ItemSlot slot, ushort? quantity)
        {
            if (!CanItemBeTaken(slot))
                return; // TODO: Message

            _hand.TransferFrom(slot, quantity);
            ReturnItemInHandEvent = 
                _hand.Item == null 
                ? null 
                : new InventoryPickupDropEvent(slot.Id.Type, slot.Id.Id, slot.Id.Slot);
            Raise(new SetCursorEvent(_hand.Item == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
        }

        static bool DoesSlotAcceptItem(ICharacterSheet sheet, ItemSlotId slotId, ItemData item)
        {
            switch (sheet.Gender)
            {
                case Gender.Male:   if (!item.AllowedGender.HasFlag(GenderMask.Male)) return false; break;
                case Gender.Female: if (!item.AllowedGender.HasFlag(GenderMask.Female)) return false; break;
                case Gender.Neuter: if (!item.AllowedGender.HasFlag(GenderMask.Neutral)) return false; break;
            }

            if (!item.Class.IsAllowed(sheet.Class))
                return false;

            // if (!item.Race.IsAllowed(sheet.Race)) // Apparently never implemented in original game?
            //     return false;


            if (item.SlotType != slotId)
                return false;

            switch (slotId)
            {
                case ItemSlotId.LeftHand:
                {
                    return !(sheet.Inventory.RightHand.Item is ItemData rightHandItem)
                        || !rightHandItem.Flags.HasFlag(ItemFlags.TwoHanded);
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
            switch (_hand?.Item)
            {
                case null: return true;
                case Gold _: return slotId == ItemSlotId.Gold;
                case Rations _: return slotId == ItemSlotId.Rations;
                case ItemData _ when slotId < ItemSlotId.NormalSlotCount: return true;
                case ItemData _ when type != InventoryType.Player: return false;
                case ItemData item:
                {
                    var state = Resolve<IGameState>();
                    var sheet = state.GetPartyMember((PartyCharacterId)id);
                    return DoesSlotAcceptItem(sheet, slotId, item);
                }

                default:
                    throw new InvalidOperationException($"Unexpected item type in hand: {_hand.GetType()}");
            }
        }

        ItemSlotId GetBestSlot(InventorySlotId id)
        {
            if (_hand.Item is Gold _) return ItemSlotId.Gold;
            if (_hand.Item is Rations _) return ItemSlotId.Rations;
            if (!(_hand.Item is ItemData item)) return id.Slot; // Shouldn't be possible

            if (id.Type != InventoryType.Player || !id.Slot.IsBodyPart()) 
                return ItemSlotId.None;

            var state = Resolve<IGameState>();
            var sheet = state.GetPartyMember((PartyCharacterId)id.Id);
            if (DoesSlotAcceptItem(sheet, id.Slot, item)) return id.Slot;
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

        static bool CanItemBeTaken(ItemSlot slot)
        {
            // TODO: Goddess' amulet etc
            switch (slot.Item)
            {
                case Gold _:
                case Rations _: return true;
                case ItemData item when 
                    slot.Id.Slot < ItemSlotId.Slot0 && 
                    item.Flags.HasFlag(ItemSlotFlags.Cursed): return false;
                default: return true;
            }
        }

        void OnGiveItem(InventoryGiveItemEvent e)
        {
            var inventory = _getInventory((InventoryId)e.MemberId);

            if (_hand.Item is Gold)
                PutDown(inventory.Gold);
            if (_hand.Item is Rations)
                PutDown(inventory.Rations);
            if (!(_hand.Item is ItemData item))
                return; // Unknown or null

            ItemSlot slot = null;
            if (item.IsStackable)
            {
                slot = inventory.Slots.FirstOrDefault(x =>
                    x.Item is ItemData existing &&
                    existing.Id == item.Id);
            }

            slot ??= inventory.Slots.FirstOrDefault(x => x.Item == null);

            if(slot != null)
                PutDown(slot);

            Update(inventory.Id);
        }

        void GetQuantity(bool discard, IInventory inventory, ItemSlotId slotId, Action<int> continuation)
        {
            var slot = inventory.GetSlot(slotId);
            var (maxQuantity, text, icon) = slot.Item switch
            {
                Gold _ => (
                    slot.Amount,
                    discard
                        ? SystemTextId.Gold_ThrowHowMuchGoldAway
                        : SystemTextId.Gold_TakeHowMuchGold,
                    CoreSpriteId.UiGold.ToAssetId()),

                Rations _ => (
                    slot.Amount,
                    discard
                        ? SystemTextId.Gold_ThrowHowManyRationsAway
                        : SystemTextId.Gold_TakeHowManyRations,
                    CoreSpriteId.UiFood.ToAssetId()),

                ItemData item => (
                    slot.Amount,
                    discard
                        ? SystemTextId.InvMsg_ThrowHowManyItemsAway
                        : SystemTextId.InvMsg_TakeHowManyItems,
                    item.Icon.ToAssetId()
                ),
                { } x => throw new InvalidOperationException($"Unexpected item contents {x}")
            };

            if (maxQuantity == 1)
                continuation(1);
            else
            {
                var formatFunc = slotId == ItemSlotId.Gold 
                    ? x => $"{x / 10}.{x % 10}" 
                    : (Func<int, string>)null;

                Exchange.Attach(new ItemQuantityDialog(
                    text,
                    icon,
                    maxQuantity,
                    continuation,
                    formatFunc));
            }
        }


        void OnPickupItem(InventorySlotEvent e)
        {
            var slotId = new InventorySlotId(e.InventoryType, e.InventoryId, e.SlotId);
            if (!DoesSlotAcceptItemInHand(e.InventoryType, e.InventoryId, e.SlotId))
                slotId = new InventorySlotId(slotId.Type, slotId.Id, GetBestSlot(slotId));

            if (slotId.Slot == ItemSlotId.None)
                return;

            var inventory = _getInventory(slotId.Inventory);
            var slot = inventory.GetSlot(slotId.Slot);
            switch (GetInventoryAction(slotId))
            {
                case InventoryAction.Pickup:
                    if (e is InventoryPickupAllEvent)
                        PickupItem(slot, null);
                    else
                    {
                        GetQuantity(false, inventory, e.SlotId, quantity =>
                        {
                            if (quantity > 0)
                                PickupItem(slot, (ushort)quantity);
                        });
                    }

                    break;
                case InventoryAction.PutDown:  PutDown(slot);          break;
                case InventoryAction.Swap:     SwapItems(slot);     break;
                case InventoryAction.Coalesce: CoalesceItems(slot); break;
                case InventoryAction.NoCoalesceFullStack: return;
            }

            Update(slotId.Inventory);
        }

        void CoalesceItems(ItemSlot slot)
        {
            ApiUtil.Assert(slot.CanCoalesce(_hand));
            ApiUtil.Assert(slot.Amount < MaxSlotAmount);

            slot.TransferFrom(_hand, null);
        }

        void SwapItems(ItemSlot slot)
        {
            // Check if the item can be taken
            if (!CanItemBeTaken(slot))
                return; // TODO: Message

            _hand.Swap(slot);

            // FIXME: could take a lightweight object from player A (who is at their max carry weight), swap it with a heavy one carried by player B
            // and then close the inventory screen. The return event will fire and drop the heavier object in player A's inventory, taking them above their
            // max carry weight.
            Raise(new SetCursorEvent(_hand.Item == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
        }

        void PutDown(ItemSlot slot)
        {
            ApiUtil.Assert(slot.Item == null);
            slot.TransferFrom(_hand, null);
        }

        void RaiseStatusMessage(SystemTextId textId) 
            => Raise(new DescriptionTextEvent(Resolve<ITextFormatter>().Format(textId)));

        public bool TryChangeInventory(
            InventoryId id,
            IContents contents,
            QuantityChangeOperation operation,
            int amount)
        {
            var context = Resolve<IEventManager>().Context;
            var inventory = _getInventory(id);
            // TODO: Ensure weight limit is not exceeded
            // TODO: Handle non-stacking items.
            int currentAmount = inventory.EnumerateAll().Where(x => contents.Equals(x.Item)).Sum(x => (int?)x.Amount) ?? 0;
            int newAmount = operation.Apply(currentAmount, amount, 0, int.MaxValue);
            int remainingDelta = newAmount - currentAmount;
            var pendingChanges = new List<(int, int)>(); // Tuples of (slot index, amount)

            for (int i = 0; i < inventory.Slots.Length && remainingDelta != 0; i++)
            {
                var slot = inventory.Slots[i];
                // Add items
                if (remainingDelta > 0) 
                {
                    if (contents.Equals(slot?.Item))
                        continue;

                    int amountToChange = Math.Min(MaxSlotAmount - (slot?.Id == null ? 0 : slot.Amount), remainingDelta);
                    if(amountToChange > 0)
                    {
                        remainingDelta -= amountToChange;
                        pendingChanges.Add((i, amountToChange));
                    }
                }
                else if (contents.Equals(slot?.Item)) // Remove items
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
                slot.Item ??= contents;
                slot.Amount = (ushort)(slot.Amount + amountToChange);
                if (slot.Amount == 0 && slot.Item is ItemData)
                    slot.Item = null;
            }

            Update(id);

            if(context.Source is EventSource.Map mapEventSource && contents is IItem item)
                ItemTransition.CreateTransitionFromTilePosition(Exchange, mapEventSource.X, mapEventSource.Y, item.Id);

            return true;
        }
    }
}
