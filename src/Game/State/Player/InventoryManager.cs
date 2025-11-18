using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.Input;
using UAlbion.Game.Text;

namespace UAlbion.Game.State.Player;

// TODO: Refactor / break this class up if possible
public class InventoryManager : GameServiceComponent<IInventoryManager>, IInventoryManager
{
    readonly Func<InventoryId, Inventory> _getInventory;
    readonly Func<ItemId, ItemData> _getItem;
    readonly ItemSlot _hand = new(new InventorySlotId(InventoryType.Temporary, 0, ItemSlotId.None));
    IEvent _returnItemInHandEvent;

    ItemSlot GetSlot(InventorySlotId id) => _getInventory(id.Id)?.GetSlot(id.Slot);
    public ReadOnlyItemSlot ItemInHand { get; }
    public InventoryManager(Func<InventoryId, Inventory> getInventory, Func<ItemId, ItemData> getItem)
    {
        _getInventory = getInventory ?? throw new ArgumentNullException(nameof(getInventory));
        _getItem = getItem ?? throw new ArgumentNullException(nameof(getItem));

        On<InventoryReturnItemInHandEvent>(_ => ReturnItemInHand());
        On<InventoryDestroyItemInHandEvent>(_ =>
        {
            if (_hand.Amount > 0)
                _hand.Amount--;
            ReturnItemInHand();
        });
        OnAsync<InventorySwapEvent>(OnSlotEvent);
        OnAsync<InventoryPickupEvent>(OnSlotEvent);
        On<InventoryGiveItemEvent>(OnGiveItem);
        OnAsync<InventoryDiscardEvent>(OnDiscard);
        On<SetInventorySlotUiPositionEvent>(OnSetSlotUiPosition);
        On<ActivateItemEvent>(OnActivateItem);
        On<ActivateItemSpellEvent>(OnActivateItemSpell);
        OnAsync<DrinkItemEvent>(OnDrinkItem);
        OnAsync<ReadItemEvent>(OnReadItem);
        On<ReadSpellScrollEvent>(OnReadSpellScroll);

        ItemInHand = new ReadOnlyItemSlot(_hand);
    }

    void ReturnItemInHand()
    {
        if (_returnItemInHandEvent == null || _hand.Item.IsNone)
            return;

        Receive(_returnItemInHandEvent, null);
    }

    void OnSetSlotUiPosition(SetInventorySlotUiPositionEvent e)
    {
        var inventory = _getInventory(e.InventorySlotId.Id);
        inventory.SetSlotUiPosition(e.InventorySlotId.Slot, new Vector2(e.X, e.Y));
    }

    public InventoryAction GetInventoryAction(InventorySlotId id)
    {
        var slot = GetSlot(id);
        if (slot == null || slot.Id.Slot == ItemSlotId.None)
            return InventoryAction.Nothing;

        return (_hand.Item.Type, slot.Item.Type) switch
        {
            (AssetType.None, AssetType.None) => InventoryAction.Nothing,
            (AssetType.None, _) => InventoryAction.Pickup,
            (AssetType.Gold, AssetType.Gold) => InventoryAction.Coalesce,
            (AssetType.Rations, AssetType.Rations) => InventoryAction.Coalesce,
            (AssetType.Gold, AssetType.None) =>
                id.Slot == ItemSlotId.Gold
                    ? InventoryAction.PutDown
                    : InventoryAction.Nothing,
            (AssetType.Rations, AssetType.None) => 
                id.Slot == ItemSlotId.Rations
                    ? InventoryAction.PutDown
                    : InventoryAction.Nothing,
            (AssetType.Item, AssetType.None) => InventoryAction.PutDown,
            (AssetType.Item, AssetType.Item) when slot.CanCoalesce(_hand, _getItem) =>
                slot.Amount >= ItemSlot.MaxItemCount
                    ? InventoryAction.NoCoalesceFullStack
                    : InventoryAction.Coalesce,
            (AssetType.Item, AssetType.Item) => InventoryAction.Swap,
            _ => InventoryAction.Nothing
        };
    }

    void Update(InventoryId id) => Raise(new InventoryChangedEvent(id));

    void PickupItem(ItemSlot slot, ushort? quantity)
    {
        if (!CanItemBeTaken(slot))
            return; // TODO: Message

        _hand.TransferFrom(slot, quantity, _getItem);
        _returnItemInHandEvent = new InventorySwapEvent(slot.Id.Id, slot.Id.Slot);
    }

    bool DoesSlotAcceptItem(ICharacterSheet sheet, ItemSlotId slotId, ItemData item)
    {
        switch (sheet.Gender)
        {
            case Gender.Male: if ((item.AllowedGender & Genders.Male) == 0) return false; break;
            case Gender.Female: if ((item.AllowedGender & Genders.Female) == 0) return false; break;
            case Gender.Neuter: if ((item.AllowedGender & Genders.Neutral) == 0) return false; break;
        }

        if (!item.Class.IsAllowed(sheet.PlayerClass))
            return false;

        // if (!item.Races.IsAllowed(sheet.Races)) // Apparently never implemented in original game?
        //     return false;

        bool force = item.SlotType == ItemSlotId.RightHandOrTail && slotId is ItemSlotId.RightHand or ItemSlotId.Tail;
        if (item.SlotType != slotId && !force)
            return false;

        switch (slotId)
        {
            case ItemSlotId.LeftHand:
            {
                var rightHandId = sheet.Inventory.RightHand.Item;
                if (rightHandId.Type != AssetType.Item)
                    return false;

                var rightHandItem = _getItem(rightHandId);
                return rightHandItem.Hands <= 1;
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
        switch (_hand?.Item.Type ?? AssetType.None)
        {
            case AssetType.None: return true;
            case AssetType.Gold: return slotId == ItemSlotId.Gold;
            case AssetType.Rations: return slotId == ItemSlotId.Rations;
            case AssetType.Item when slotId < ItemSlotId.NormalSlotCount: return true;
            case AssetType.Item when id.Type != InventoryType.Player: return false;
            case AssetType.Item:
            {
                var item = _getItem(_hand!.Item);
                var state = Resolve<IGameState>();
                var sheet = state.GetSheet(id.ToSheetId());
                return DoesSlotAcceptItem(sheet, slotId, item);
            }

            default:
                throw new InvalidOperationException($"Unexpected item type in hand: {_hand?.GetType()}");
        }
    }

    ItemSlotId GetBestSlot(InventorySlotId id)
    {
        if (_hand.Item.Type == AssetType.Gold) return ItemSlotId.Gold;
        if (_hand.Item.Type == AssetType.Rations) return ItemSlotId.Rations;
        if (_hand.Item.Type != AssetType.Item) return id.Slot; // Shouldn't be possible

        if (id.Id.Type != InventoryType.Player || !id.Slot.IsBodyPart())
            return ItemSlotId.None;

        var state = Resolve<IGameState>();
        var sheet = state.GetSheet(id.Id.ToSheetId());
        var item = _getItem(_hand.Item);

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
        switch (slot.Item.Type)
        {
            case AssetType.Gold:
            case AssetType.Rations: return true;
            case AssetType.Item:
                bool curseActive = (slot.Flags & ItemSlotFlags.Cursed) != 0 &&
                                   slot.Id.Slot.IsBodyPart();
                return !curseActive;
            default: return true;
        }
    }

    void OnGiveItem(InventoryGiveItemEvent e)
    {
        var inventory = _getInventory((InventoryId)e.MemberId);
        switch (_hand.Item.Type)
        {
            case AssetType.Gold: inventory.Gold.TransferFrom(_hand, null, _getItem); break;
            case AssetType.Rations: inventory.Rations.TransferFrom(_hand, null, _getItem); break;
            case AssetType.Item:
            {
                var item = _getItem(_hand.Item);
                ItemSlot slot = null;
                if (item.IsStackable)
                    slot = inventory.BackpackSlots.FirstOrDefault(x => x.Item == _hand.Item);

                slot ??= inventory.BackpackSlots.FirstOrDefault(x => x.Item.IsNone);
                slot?.TransferFrom(_hand, null, _getItem);
                break;
            }

            default: return; // Unknown or null
        }

        Update(inventory.Id);
        SetCursor();
    }

    AlbionTask<int> GetQuantity(bool discard, IInventory inventory, ItemSlotId slotId)
    {
        var slot = inventory.GetSlot(slotId);
        if (slot.Amount == 1)
            return AlbionTask.FromResult(1);

        var text = (slot.Item.Type, discard) switch
        {
            (AssetType.Gold, true) => Base.SystemText.Gold_ThrowHowMuchGoldAway,
            (AssetType.Gold, false) => Base.SystemText.Gold_TakeHowMuchGold,
            (AssetType.Rations, true) => Base.SystemText.Gold_ThrowHowManyRationsAway,
            (AssetType.Rations, false) => Base.SystemText.Gold_TakeHowManyRations,
            (AssetType.Item, true) => Base.SystemText.InvMsg_ThrowHowManyItemsAway,
            (AssetType.Item, false) => Base.SystemText.InvMsg_TakeHowManyItems,
            var x => throw new InvalidOperationException($"Unexpected item contents {x}")
        };

        var (sprite, subId, _) = GetSprite(slot.Item);
        var promptEvent = new ItemQuantityPromptEvent(new StringId(text), sprite, subId, slot.Amount, slotId == ItemSlotId.Gold);
        return RaiseQueryA(promptEvent);

        /* if (RaiseAsync(, continuation) == 0)
        {
            ApiUtil.Assert("ItemManager.GetQuantity tried to open a quantity dialog, but no-one was listening for the event.");
            return 0;
        } */
    }

    (SpriteId sprite, int subId, int frameCount) GetSprite(ItemId id)
    {
        switch (_hand.Item.Type)
        {
            case AssetType.None: return (AssetId.None, 0, 1);
            case AssetType.Gold: return (Base.CoreGfx.UiGold, 0, 1);
            case AssetType.Rations: return (Base.CoreGfx.UiFood, 0, 1);
            case AssetType.Item:
                {
                    var item = _getItem(_hand.Item);
                    return (item.Icon, item.IconSubId, item.IconAnim);
                }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    id,
                    $"{id} was expected to be None, Gold, Rations or an Item");
        }
    }

    async AlbionTask OnSlotEvent(InventorySlotEvent e)
    {
        var slotId = new InventorySlotId(e.Id, e.SlotId);
        bool redirected = false;

        if (!DoesSlotAcceptItemInHand(e.Id, e.SlotId))
        {
            slotId = new InventorySlotId(slotId.Id, GetBestSlot(slotId));
            redirected = true;
        }

        if (slotId.Slot is ItemSlotId.None or ItemSlotId.CharacterBody)
            return;

        Inventory inventory = _getInventory(slotId.Id);
        ItemSlot slot = inventory?.GetSlot(slotId.Slot);
        if (slot == null)
            return;

        var cursorManager = Resolve<ICursorManager>();
        var window = Resolve<IGameWindow>();
        var cursorUiPosition = window.PixelToUi(cursorManager.Position);

        switch (GetInventoryAction(slotId))
        {
            case InventoryAction.Pickup:
                {
                    if (slot.Amount == 1)
                    {
                        PickupItem(slot, null);
                    }
                    else if (e is InventoryPickupEvent pickup)
                    {
                        PickupItem(slot, pickup.Amount);
                    }
                    else
                    {
                        var quantity = await GetQuantity(false, inventory, e.SlotId);
                        if (quantity > 0)
                            PickupItem(slot, (ushort)quantity);
                    }
                    break;
                }

            case InventoryAction.PutDown:
            {
                if (!redirected)
                {
                    slot.TransferFrom(_hand, null, _getItem);
                }
                else
                {
                    var transitionEvent = new LinearItemTransitionEvent(
                        _hand.Item,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        ReadVar(V.Game.Ui.Transitions.ItemMovementTransitionTimeSeconds));

                    ItemSlot temp = new(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    temp.TransferFrom(_hand, null, _getItem);
                    SetCursor();
                    await RaiseA(transitionEvent);

                    slot.TransferFrom(temp, null, _getItem);
                }

                break;
            }

            case InventoryAction.Swap:
            {
                if (!redirected)
                {
                    SwapItems(slot);
                }
                else
                {
                    // Original game didn't handle this, but doesn't hurt.
                    var transitionEvent1 = new LinearItemTransitionEvent(
                        _hand.Item,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        ReadVar(V.Game.Ui.Transitions.ItemMovementTransitionTimeSeconds));

                    var transitionEvent2 = new LinearItemTransitionEvent(
                        slot.Item,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        ReadVar(V.Game.Ui.Transitions.ItemMovementTransitionTimeSeconds));

                    ItemSlot temp1 = new(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    ItemSlot temp2 = new(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    temp1.TransferFrom(_hand, null, _getItem);
                    temp2.TransferFrom(slot, null, _getItem);

                    var transition1 = RaiseA(transitionEvent1);
                    var transition2 = RaiseA(transitionEvent2);

                    await transition1;
                    await transition2;
                    slot.TransferFrom(temp1, null, _getItem);
                    _hand.TransferFrom(temp2, null, _getItem);
                    _returnItemInHandEvent = new InventorySwapEvent(slot.Id.Id, slot.Id.Slot);
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
                    break;
                }
            case InventoryAction.NoCoalesceFullStack: break; // No-op
        }

        Update(slotId.Id);
        SetCursor();
    }

    async AlbionTask OnDiscard(InventoryDiscardEvent e)
    {
        var inventory = _getInventory(e.Id);
        var quantity = await GetQuantity(true, inventory, e.SlotId);

        if (quantity <= 0)
        {
            return;
        }

        var slot = inventory.GetSlot(e.SlotId);
        ushort itemsToDrop = Math.Min((ushort)quantity, slot.Amount);

        var prompt = slot.Item.Type switch
        {
            AssetType.Gold => Base.SystemText.Gold_ReallyThrowTheGoldAway,
            AssetType.Rations => Base.SystemText.Gold_ReallyThrowTheRationsAway,
            AssetType.Item when itemsToDrop == 1 => Base.SystemText.InvMsg_ReallyThrowThisItemAway,
            _ => Base.SystemText.InvMsg_ReallyThrowTheseItemsAway,
        };

        var response = await RaiseQueryA(new YesNoPromptEvent(prompt));

        if (!response)
            return;

        if (!slot.Item.IsNone)
        {
            var maxTransitions = ReadVar(V.Game.Ui.Transitions.MaxDiscardTransitions);
            var transitionsToShow = Math.Min(itemsToDrop, maxTransitions);

            for (int i = 0; i < transitionsToShow; i++)
                _ = RaiseA(new GravityItemTransitionEvent(slot.Item, e.NormX, e.NormY));
        }

        slot.Amount -= itemsToDrop;
        Update(e.Id);
        SetCursor();
    }

    void SetCursor()
    {
        var (sprite, subId, frameCount) = GetSprite(_hand.Item);
        Raise(new SetCursorEvent(_hand.Item.IsNone ? Base.CoreGfx.Cursor : Base.CoreGfx.CursorSmall));
        Raise(new SetHeldItemCursorEvent(sprite, subId, frameCount, _hand.Amount, _hand.Item == AssetId.Gold));
    }

    void CoalesceItems(ItemSlot slot)
    {
        ApiUtil.Assert(slot.CanCoalesce(_hand, _getItem));
        ApiUtil.Assert(slot.Amount < ItemSlot.MaxItemCount || slot.Item.Type is AssetType.Gold or AssetType.Rations);
        slot.TransferFrom(_hand, null, _getItem);
    }

    void SwapItems(ItemSlot slot)
    {
        // Check if the item can be taken
        if (!CanItemBeTaken(slot))
            return; // TODO: Message

        _hand.Swap(slot);
        _returnItemInHandEvent = new InventorySwapEvent(slot.Id.Id, slot.Id.Slot);
    }

    // void RaiseStatusMessage(TextId textId) => Raise(new DescriptionTextEvent(Resolve<ITextFormatter>().Format(textId)));

    public int GetItemCount(InventoryId id, ItemId item) => _getInventory(id).EnumerateAll().Where(x => x.Item == item).Sum(x => (int?)x.Amount) ?? 0;
    public ushort TryGiveItems(InventoryId id, ItemSlot donor, ushort? amount)
    {
        ArgumentNullException.ThrowIfNull(donor);

        // TODO: Ensure weight limit is not exceeded?
        ushort totalTransferred = 0;
        ushort remaining = amount ?? ushort.MaxValue;
        var inventory = _getInventory(id);

        if (donor.Item == AssetId.Gold)
            return inventory.Gold.TransferFrom(donor, remaining, _getItem);

        if (donor.Item == AssetId.Rations)
            return inventory.Rations.TransferFrom(donor, remaining, _getItem);

        for (int i = 0; i < (int)ItemSlotId.NormalSlotCount && amount != 0; i++)
        {
            if (!inventory.Slots[i].CanCoalesce(donor, _getItem))
                continue;

            ushort transferred = inventory.Slots[i].TransferFrom(donor, remaining, _getItem);
            remaining -= transferred;
            totalTransferred += transferred;
            if (remaining == 0 || donor.Item.IsNone)
                break;
        }

        return totalTransferred;
    }

    public ushort TryTakeItems(InventoryId id, ItemSlot acceptor, ItemId item, ushort? amount)
    {
        ArgumentNullException.ThrowIfNull(acceptor);

        ushort totalTransferred = 0;
        ushort remaining = amount ?? ushort.MaxValue;
        var inventory = _getInventory(id);

        if (item == AssetId.Gold)
            return acceptor.TransferFrom(inventory.Gold, remaining, _getItem);

        if (item == AssetId.Rations)
            return acceptor.TransferFrom(inventory.Rations, remaining, _getItem);

        for (int i = 0; i < (int)ItemSlotId.NormalSlotCount && amount != 0; i++)
        {
            if (inventory.Slots[i].Item != item) 
                continue;

            ushort transferred = acceptor.TransferFrom(inventory.Slots[i], remaining, _getItem);
            totalTransferred += transferred;
            remaining -= transferred;
            if (remaining == 0)
                break;
        }

        return totalTransferred;
    }

    void OnActivateItem(ActivateItemEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return;

        var item = _getItem(slot.Item);
        if (item.TypeId != ItemType.HeadsUpDisplayItem)
            return;

        var textId = TextId.None;
        if (item.Id == Base.Item.Clock)
            textId = Base.SystemText.SpecialItem_TheClockHasBeenActivated;
        if (item.Id == Base.Item.Compass)
            textId = Base.SystemText.SpecialItem_TheCompassHasBeenActivated;
        if (item.Id == Base.Item.MonsterEye)
            textId = Base.SystemText.SpecialItem_TheMonsterEyeHasBeenActivated;

        if (textId.IsNone)
            return;

        var tf = Resolve<ITextFormatter>();
        Raise(new SetSpecialItemActiveEvent(item.Id, true));
        Raise(new HoverTextEvent(tf.Format(Base.SystemText.Item_Add)));

        slot.Amount--;
        Update(e.SlotId.Id);
    }

    void OnActivateItemSpell(ActivateItemSpellEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return;

        var item = _getItem(slot.Item);
        if (item.Charges <= 0 || item.Spell.IsNone)
            return;

        Warn("TODO: Actually cast the spell once the magic system is implemented");

        item.Charges--;
        Update(e.SlotId.Id);
    }

    async AlbionTask OnDrinkItem(DrinkItemEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return;

        var item = _getItem(slot.Item);
        if (item.TypeId != ItemType.Drink)
            return;

        var result = await RaiseQueryA(new PartyMemberPromptEvent(Base.SystemText.InvMsg_WhoShouldDrinkThis));

        if (result == PartyMemberId.None)
            return;

        await RaiseA(new SetContextEvent(ContextType.Subject, result));
        await TriggerItemChain(slot.Item);

        slot.Amount--;
        Update(e.SlotId.Id);
    }

    AlbionTask OnReadItem(ReadItemEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return AlbionTask.CompletedTask;

        var item = _getItem(slot.Item);
        if (item.TypeId != ItemType.Document)
            return AlbionTask.CompletedTask;

        return TriggerItemChain(item.Id);
    }

    void OnReadSpellScroll(ReadSpellScrollEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return;

        var item = _getItem(slot.Item);
        if (item.TypeId != ItemType.SpellScroll)
            return;

        var spell = Assets.LoadSpell(item.Spell);
        if (spell == null)
            return;

        var tf = Resolve<ITextFormatter>();
        var party = Resolve<IParty>();

        var target = party.StatusBarOrder
            .FirstOrDefault(x => (x.Effective.Magic.SpellClasses & spell.Class.ToFlag()) != 0);

        if (target == null)
        {
            Raise(new HoverTextEvent(tf.Format(Base.SystemText.InvMsg_NoOneKnowsThisSpellClass)));
            return;
        }

        if (target.Effective.Magic.KnownSpells.Contains(spell.Id))
        {
            Raise(new HoverTextEvent(tf.Format(Base.SystemText.InvMsg_ThisSpellIsAlreadyKnown)));
            return;
        }

        if (target.Effective.Level < spell.LevelRequirement)
        {
            Raise(new HoverTextEvent(tf.Format(Base.SystemText.InvMsg_ThisSpellsLevelIsTooHigh)));
            return;
        }

        Raise(new LearnSpellEvent(e.SlotId.Id.ToSheetId(), item.Spell));
        Raise(new HoverTextEvent(tf.Format(Base.SystemText.InvMsg_XLearnedTheSpell)));
        
        slot.Amount--;
        Update(e.SlotId.Id);
    }

    async AlbionTask TriggerItemChain(ItemId itemId)
    {
        var eventSet = Assets.LoadEventSet(Base.EventSet.InventoryItems);
        foreach (var eventIndex in eventSet.Chains)
        {
            if (eventSet.Events[eventIndex].Event is not ActionEvent action)
                continue;

            if (action.ActionType != ActionType.UseItem || action.Argument != (AssetId)itemId)
                continue;

            var triggerEvent = new TriggerChainEvent(
                eventSet,
                eventIndex,
                new EventSource(eventSet.Id, TriggerType.Action));

            await RaiseA(triggerEvent);
            return;
        }
    }
}
