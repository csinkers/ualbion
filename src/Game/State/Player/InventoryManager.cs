using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.Input;
using UAlbion.Game.Text;

namespace UAlbion.Game.State.Player;

public class InventoryEventManager : Component
{
}

public class InventoryManager : ServiceComponent<IInventoryManager>, IInventoryManager
{
    readonly Func<InventoryId, Inventory> _getInventory;
    readonly ItemSlot _hand = new(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.None));
    IEvent _returnItemInHandEvent;

    ItemSlot GetSlot(InventorySlotId id) => _getInventory(id.Id)?.GetSlot(id.Slot);
    public ReadOnlyItemSlot ItemInHand { get; }
    public InventoryManager(Func<InventoryId, Inventory> getInventory)
    {
        On<InventoryReturnItemInHandEvent>(_ => ReturnItemInHand());
        On<InventoryDestroyItemInHandEvent>(_ =>
        {
            if (_hand.Amount > 0)
                _hand.Amount--;
            ReturnItemInHand();
        });
        OnAsync<InventorySwapEvent>(OnSlotEvent);
        OnAsync<InventoryPickupEvent>(OnSlotEvent);
        OnAsync<InventoryGiveItemEvent>(OnGiveItem);
        OnAsync<InventoryDiscardEvent>(OnDiscard);
        On<SetInventorySlotUiPositionEvent>(OnSetSlotUiPosition);
        _getInventory = getInventory;
        ItemInHand = new ReadOnlyItemSlot(_hand);
    }

    void ReturnItemInHand()
    {
        if (_returnItemInHandEvent == null || _hand.Item == null)
            return;

        Receive(_returnItemInHandEvent, null);
    }

    void OnSetSlotUiPosition(SetInventorySlotUiPositionEvent e)
    {
        var inventory = _getInventory(e.InventorySlotId.Id);
        inventory.SetSlotUiPosition(e.InventorySlotId.Slot, new Vector2(e.X, e.Y));
    }

    public InventoryAction GetInventoryAction(InventorySlotId slotId)
    {
        var slot = GetSlot(slotId);
        if (slot == null || slot.Id.Slot == ItemSlotId.None)
            return InventoryAction.Nothing;

        switch (_hand.Item, slot.Item)
        {
            case (null, null): return InventoryAction.Nothing;
            case (null, _): return InventoryAction.Pickup;

            case (Gold, Gold):
            case (Rations, Rations): return InventoryAction.Coalesce;
            case (Gold, null): return slotId.Slot == ItemSlotId.Gold ? InventoryAction.PutDown : InventoryAction.Nothing;
            case (Rations, null): return slotId.Slot == ItemSlotId.Rations ? InventoryAction.PutDown : InventoryAction.Nothing;

            case (ItemData, null): return InventoryAction.PutDown;
            case (ItemData, ItemData) when slot.CanCoalesce(_hand):
                return slot.Amount >= ItemSlot.MaxItemCount
                    ? InventoryAction.NoCoalesceFullStack
                    : InventoryAction.Coalesce;

            case (ItemData, ItemData):
                return InventoryAction.Swap;

            default:
                return InventoryAction.Nothing;
        }
    }

    void Update(InventoryId id) => Raise(new InventoryChangedEvent(id));

    void PickupItem(ItemSlot slot, ushort? quantity)
    {
        if (!CanItemBeTaken(slot))
            return; // TODO: Message

        _hand.TransferFrom(slot, quantity);
        _returnItemInHandEvent = new InventorySwapEvent(slot.Id.Id, slot.Id.Slot);
    }

    static bool DoesSlotAcceptItem(ICharacterSheet sheet, ItemSlotId slotId, ItemData item)
    {
        switch (sheet.Gender)
        {
            case Gender.Male: if (!item.AllowedGender.HasFlag(Genders.Male)) return false; break;
            case Gender.Female: if (!item.AllowedGender.HasFlag(Genders.Female)) return false; break;
            case Gender.Neuter: if (!item.AllowedGender.HasFlag(Genders.Neutral)) return false; break;
        }

        if (!item.Class.IsAllowed(sheet.PlayerClass))
            return false;

        // if (!item.Races.IsAllowed(sheet.Races)) // Apparently never implemented in original game?
        //     return false;

        if (item.SlotType != slotId)
            return false;

        switch (slotId)
        {
            case ItemSlotId.LeftHand:
            {
                return sheet.Inventory.RightHand.Item is not ItemData { Hands: > 1 };
            }
            case ItemSlotId.Tail:
                return
                    item.TypeId == ItemType.CloseRangeWeapon
                    && item.SlotType is ItemSlotId.Tail or ItemSlotId.RightHandOrTail;
            default:
                return true;
        }
    }

    bool DoesSlotAcceptItemInHand(InventoryId id, ItemSlotId slotId)
    {
        switch (_hand?.Item)
        {
            case null: return true;
            case Gold: return slotId == ItemSlotId.Gold;
            case Rations: return slotId == ItemSlotId.Rations;
            case ItemData when slotId < ItemSlotId.NormalSlotCount: return true;
            case ItemData when id.Type != InventoryType.Player: return false;
            case ItemData item:
            {
                var state = Resolve<IGameState>();
                var sheet = state.GetSheet(id.ToAssetId());
                return DoesSlotAcceptItem(sheet, slotId, item);
            }

            default:
                throw new InvalidOperationException($"Unexpected item type in hand: {_hand.GetType()}");
        }
    }

    ItemSlotId GetBestSlot(InventorySlotId id)
    {
        if (_hand.Item is Gold) return ItemSlotId.Gold;
        if (_hand.Item is Rations) return ItemSlotId.Rations;
        if (_hand.Item is not ItemData item) return id.Slot; // Shouldn't be possible

        if (id.Id.Type != InventoryType.Player || !id.Slot.IsBodyPart())
            return ItemSlotId.None;

        var state = Resolve<IGameState>();
        var sheet = state.GetSheet(id.Id.ToAssetId());
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
            case Gold:
            case Rations: return true;
            case ItemData item when
                slot.Id.Slot < ItemSlotId.Slot0 &&
                item.Flags.HasFlag(ItemSlotFlags.Cursed):
                return false;
            default: return true;
        }
    }

    bool OnGiveItem(InventoryGiveItemEvent e, Action continuation)
    {
        var inventory = _getInventory((InventoryId)e.MemberId);
        switch (_hand.Item)
        {
            case Gold: inventory.Gold.TransferFrom(_hand, null); break;
            case Rations: inventory.Rations.TransferFrom(_hand, null); break;
            case ItemData item:
            {
                ItemSlot slot = null;
                if (item.IsStackable)
                {
                    slot = inventory.BackpackSlots.FirstOrDefault(x =>
                        x.Item is ItemData existing &&
                        existing.Id == item.Id);
                }

                slot ??= inventory.BackpackSlots.FirstOrDefault(x => x.Item == null);
                slot?.TransferFrom(_hand, null);

                break;
            }
            default: return false; // Unknown or null
        }

        Update(inventory.Id);
        SetCursor();
        continuation();
        return true;
    }

    void GetQuantity(bool discard, IInventory inventory, ItemSlotId slotId, Action<int> continuation)
    {
        var slot = inventory.GetSlot(slotId);
        if (slot.Amount == 1)
        {
            continuation(1);
            return;
        }

        var text = (slot.Item, discard) switch
        {
            (Gold, true) => Base.SystemText.Gold_ThrowHowMuchGoldAway,
            (Gold, false) => Base.SystemText.Gold_TakeHowMuchGold,
            (Rations, true) => Base.SystemText.Gold_ThrowHowManyRationsAway,
            (Rations, false) => Base.SystemText.Gold_TakeHowManyRations,
            (ItemData, true) => Base.SystemText.InvMsg_ThrowHowManyItemsAway,
            (ItemData, false) => Base.SystemText.InvMsg_TakeHowManyItems,
            var x => throw new InvalidOperationException($"Unexpected item contents {x}")
        };

        if (RaiseAsync(new ItemQuantityPromptEvent((TextId)text, slot.Item.Icon, slot.Item.IconSubId, slot.Amount, slotId == ItemSlotId.Gold), continuation) == 0)
        {
            ApiUtil.Assert("ItemManager.GetQuantity tried to open a quantity dialog, but no-one was listening for the event.");
            continuation(0);
        }
    }

    bool OnSlotEvent(InventorySlotEvent e, Action continuation)
    {
        var slotId = new InventorySlotId(e.Id, e.SlotId);
        bool redirected = false;
        bool complete = false;

        if (!DoesSlotAcceptItemInHand(e.Id, e.SlotId))
        {
            slotId = new InventorySlotId(slotId.Id, GetBestSlot(slotId));
            redirected = true;
        }

        if (slotId.Slot is ItemSlotId.None or ItemSlotId.CharacterBody)
            return false;

        Inventory inventory = _getInventory(slotId.Id);
        ItemSlot slot = inventory?.GetSlot(slotId.Slot);
        if (slot == null)
            return false;

        var config = Resolve<IGameConfigProvider>().Game;
        var cursorManager = Resolve<ICursorManager>();
        var window = Resolve<IWindowManager>();
        var cursorUiPosition = window.PixelToUi(cursorManager.Position);

        switch (GetInventoryAction(slotId))
        {
            case InventoryAction.Pickup:
            {
                if (slot.Amount == 1)
                {
                    PickupItem(slot, null);
                    complete = true;
                }
                else if (e is InventoryPickupEvent pickup)
                {
                    PickupItem(slot, pickup.Amount);
                    complete = true;
                }
                else
                {
                    GetQuantity(false, inventory, e.SlotId, quantity =>
                    {
                        if (quantity > 0)
                            PickupItem(slot, (ushort)quantity);

                        Update(slotId.Id);
                        SetCursor();
                        continuation();
                    });
                }
                break;
            }
            case InventoryAction.PutDown:
            {
                if (redirected)
                {
                    var transitionEvent = new LinearItemTransitionEvent(
                        _hand.ItemId,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        config.UI.Transitions.ItemMovementTransitionTimeSeconds);

                    ItemSlot temp = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    temp.TransferFrom(_hand, null);
                    SetCursor();
                    RaiseAsync(transitionEvent, () =>
                    {
                        slot.TransferFrom(temp, null);
                        Update(slotId.Id);
                        continuation();
                    });
                }
                else
                {
                    slot.TransferFrom(_hand, null);
                    complete = true;
                }
                break;
            }
            case InventoryAction.Swap:
            {
                if (redirected)
                {
                    // Original game didn't handle this, but doesn't hurt.
                    var transitionEvent1 = new LinearItemTransitionEvent(
                        _hand.ItemId,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        config.UI.Transitions.ItemMovementTransitionTimeSeconds);

                    var transitionEvent2 = new LinearItemTransitionEvent(
                        slot.ItemId,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        config.UI.Transitions.ItemMovementTransitionTimeSeconds);

                    ItemSlot temp1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    ItemSlot temp2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    temp1.TransferFrom(_hand, null);
                    temp2.TransferFrom(slot, null);

                    RaiseAsync(transitionEvent1, () =>
                    {
                        slot.TransferFrom(temp1, null);
                        Update(slotId.Id);
                        SetCursor();
                        continuation();
                    });

                    RaiseAsync(transitionEvent2, () =>
                    {
                        _hand.TransferFrom(temp2, null);
                        _returnItemInHandEvent = new InventorySwapEvent(slot.Id.Id, slot.Id.Slot);
                        SetCursor();
                    });

                    SetCursor();
                }
                else
                {
                    SwapItems(slot);
                    complete = true;
                }
                break;
            }

            // Shouldn't be possible for this to be a redirect as redirects only happen between body parts and they don't allow stacks.
            case InventoryAction.Coalesce:
            {
                if (e is InventoryPickupEvent pickup)
                    PickupItem(slot, pickup.Amount);
                else
                    CoalesceItems(slot);
                complete = true;
                break;
            }
            case InventoryAction.NoCoalesceFullStack: complete = true; break; // No-op
        }

        if (complete)
        {
            continuation();
            Update(slotId.Id);
            SetCursor();
        }

        return true;
    }

    bool OnDiscard(InventoryDiscardEvent e, Action continuation)
    {
        var inventory = _getInventory(e.Id);
        GetQuantity(true, inventory, e.SlotId, quantity =>
        {
            if (quantity <= 0)
            {
                continuation();
                return;
            }

            var slot = inventory.GetSlot(e.SlotId);
            ushort itemsToDrop = Math.Min((ushort)quantity, slot.Amount);

            var prompt = slot.Item switch
            {
                Gold => Base.SystemText.Gold_ReallyThrowTheGoldAway,
                Rations => Base.SystemText.Gold_ReallyThrowTheRationsAway,
                ItemData when itemsToDrop == 1 => Base.SystemText.InvMsg_ReallyThrowThisItemAway,
                _ => Base.SystemText.InvMsg_ReallyThrowTheseItemsAway,
            };

            RaiseAsync(new YesNoPromptEvent((TextId)prompt), response =>
            {
                if (!response)
                {
                    continuation();
                    return;
                }

                if (!slot.ItemId.IsNone)
                {
                    var config = Resolve<IGameConfigProvider>().Game;
                    for (int i = 0; i < itemsToDrop && i < config.UI.Transitions.MaxDiscardTransitions; i++)
                        Raise(new GravityItemTransitionEvent(slot.ItemId, e.NormX, e.NormY));
                }

                slot.Amount -= itemsToDrop;
                Update(e.Id);
                SetCursor();
                continuation();
            });
        });
        return true;
    }

    void SetCursor()
    {
        Raise(new SetCursorEvent(_hand.Item == null ? Base.CoreSprite.Cursor : Base.CoreSprite.CursorSmall));
    }

    void CoalesceItems(ItemSlot slot)
    {
        ApiUtil.Assert(slot.CanCoalesce(_hand));
        ApiUtil.Assert(slot.Amount < ItemSlot.MaxItemCount || slot.Item is Gold or Rations);
        slot.TransferFrom(_hand, null);
    }

    void SwapItems(ItemSlot slot)
    {
        // Check if the item can be taken
        if (!CanItemBeTaken(slot))
            return; // TODO: Message

        _hand.Swap(slot);
        _returnItemInHandEvent = new InventorySwapEvent(slot.Id.Id, slot.Id.Slot);
    }

    void RaiseStatusMessage(TextId textId)
        => Raise(new DescriptionTextEvent(Resolve<ITextFormatter>().Format(textId)));

    public int GetItemCount(InventoryId id, ItemId item) => _getInventory(id).EnumerateAll().Where(x => x.ItemId == item).Sum(x => (int?)x.Amount) ?? 0;
    public ushort TryGiveItems(InventoryId id, ItemSlot donor, ushort? amount)
    {
        // TODO: Ensure weight limit is not exceeded?
        ushort totalTransferred = 0;
        ushort remaining = amount ?? ushort.MaxValue;
        var inventory = _getInventory(id);

        if (donor.ItemId == AssetId.Gold)
            return inventory.Gold.TransferFrom(donor, remaining);

        if (donor.ItemId == AssetId.Rations)
            return inventory.Rations.TransferFrom(donor, remaining);

        for (int i = 0; i < (int)ItemSlotId.NormalSlotCount && amount != 0; i++)
        {
            if (!inventory.Slots[i].CanCoalesce(donor))
                continue;

            ushort transferred = inventory.Slots[i].TransferFrom(donor, remaining);
            remaining -= transferred;
            totalTransferred += transferred;
            if (remaining == 0 || donor.Item == null)
                break;
        }

        return totalTransferred;
    }

    public ushort TryTakeItems(InventoryId id, ItemSlot acceptor, ItemId item, ushort? amount)
    {
        ushort totalTransferred = 0;
        ushort remaining = amount ?? ushort.MaxValue;
        var inventory = _getInventory(id);

        if (item == AssetId.Gold)
            return acceptor.TransferFrom(inventory.Gold, remaining);

        if (item == AssetId.Rations)
            return acceptor.TransferFrom(inventory.Rations, remaining);

        for (int i = 0; i < (int)ItemSlotId.NormalSlotCount && amount != 0; i++)
        {
            if (inventory.Slots[i].ItemId != item) 
                continue;

            ushort transferred = acceptor.TransferFrom(inventory.Slots[i], remaining);
            totalTransferred += transferred;
            remaining -= transferred;
            if (remaining == 0)
                break;
        }

        return totalTransferred;
    }
}