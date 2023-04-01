using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.Input;
using UAlbion.Game.Text;

namespace UAlbion.Game.State.Player;

// TODO: Refactor / break this class up if possible
#pragma warning disable CA1506 // 'InventoryManager' is coupled with '111' different types from '23' different namespaces. Rewrite or refactor the code to decrease its class coupling below '96'.
public class InventoryManager : ServiceComponent<IInventoryManager>, IInventoryManager
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
        OnAsync<InventoryGiveItemEvent>(OnGiveItem);
        OnAsync<InventoryDiscardEvent>(OnDiscard);
        On<SetInventorySlotUiPositionEvent>(OnSetSlotUiPosition);
        On<ActivateItemEvent>(OnActivateItem);
        On<ActivateItemSpellEvent>(OnActivateItemSpell);
        On<DrinkItemEvent>(OnDrinkItem);
        On<ReadItemEvent>(OnReadItem);
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

        switch (_hand.Item.Type, slot.Item.Type)
        {
            case (AssetType.None, AssetType.None):
                return InventoryAction.Nothing;

            case (AssetType.None, _):
                return InventoryAction.Pickup;

            case (AssetType.Gold, AssetType.Gold):
            case (AssetType.Rations, AssetType.Rations):
                return InventoryAction.Coalesce;

            case (AssetType.Gold, AssetType.None):
                return id.Slot == ItemSlotId.Gold 
                    ? InventoryAction.PutDown 
                    : InventoryAction.Nothing;

            case (AssetType.Rations, AssetType.None):
                return id.Slot == ItemSlotId.Rations 
                    ? InventoryAction.PutDown 
                    : InventoryAction.Nothing;

            case (AssetType.Item, AssetType.None):
                return InventoryAction.PutDown;

            case (AssetType.Item, AssetType.Item) when slot.CanCoalesce(_hand, _getItem):
                return slot.Amount >= ItemSlot.MaxItemCount
                    ? InventoryAction.NoCoalesceFullStack
                    : InventoryAction.Coalesce;

            case (AssetType.Item, AssetType.Item):
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

    bool OnGiveItem(InventoryGiveItemEvent e, Action continuation)
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
        if (RaiseAsync(new ItemQuantityPromptEvent((TextId)text, sprite, subId, slot.Amount, slotId == ItemSlotId.Gold), continuation) == 0)
        {
            ApiUtil.Assert("ItemManager.GetQuantity tried to open a quantity dialog, but no-one was listening for the event.");
            continuation(0);
        }
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
                        _hand.Item,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        Var(GameVars.Ui.Transitions.ItemMovementTransitionTimeSeconds));

                    ItemSlot temp = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    temp.TransferFrom(_hand, null, _getItem);
                    SetCursor();
                    RaiseAsync(transitionEvent, () =>
                    {
                        slot.TransferFrom(temp, null, _getItem);
                        Update(slotId.Id);
                        continuation();
                    });
                }
                else
                {
                    slot.TransferFrom(_hand, null, _getItem);
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
                        _hand.Item,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        Var(GameVars.Ui.Transitions.ItemMovementTransitionTimeSeconds));

                    var transitionEvent2 = new LinearItemTransitionEvent(
                        slot.Item,
                        (int)slot.LastUiPosition.X,
                        (int)slot.LastUiPosition.Y,
                        (int)cursorUiPosition.X,
                        (int)cursorUiPosition.Y,
                        Var(GameVars.Ui.Transitions.ItemMovementTransitionTimeSeconds));

                    ItemSlot temp1 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    ItemSlot temp2 = new ItemSlot(new InventorySlotId(InventoryType.Temporary, 0, 0));
                    temp1.TransferFrom(_hand, null, _getItem);
                    temp2.TransferFrom(slot, null, _getItem);

                    RaiseAsync(transitionEvent1, () =>
                    {
                        slot.TransferFrom(temp1, null, _getItem);
                        Update(slotId.Id);
                        SetCursor();
                        continuation();
                    });

                    RaiseAsync(transitionEvent2, () =>
                    {
                        _hand.TransferFrom(temp2, null, _getItem);
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

            var prompt = slot.Item.Type switch
            {
                AssetType.Gold => Base.SystemText.Gold_ReallyThrowTheGoldAway,
                AssetType.Rations => Base.SystemText.Gold_ReallyThrowTheRationsAway,
                AssetType.Item when itemsToDrop == 1 => Base.SystemText.InvMsg_ReallyThrowThisItemAway,
                _ => Base.SystemText.InvMsg_ReallyThrowTheseItemsAway,
            };

            RaiseAsync(new YesNoPromptEvent((TextId)prompt), response =>
            {
                if (!response)
                {
                    continuation();
                    return;
                }

                if (!slot.Item.IsNone)
                {
                    var maxTransitions = Var(GameVars.Ui.Transitions.MaxDiscardTransitions);
                    for (int i = 0; i < itemsToDrop && i < maxTransitions; i++)
                        Raise(new GravityItemTransitionEvent(slot.Item, e.NormX, e.NormY));
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

    void RaiseStatusMessage(TextId textId)
        => Raise(new DescriptionTextEvent(Resolve<ITextFormatter>().Format(textId)));

    public int GetItemCount(InventoryId id, ItemId item) => _getInventory(id).EnumerateAll().Where(x => x.Item == item).Sum(x => (int?)x.Amount) ?? 0;
    public ushort TryGiveItems(InventoryId id, ItemSlot donor, ushort? amount)
    {
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

    void OnDrinkItem(DrinkItemEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return;

        var item = _getItem(slot.Item);
        if (item.TypeId != ItemType.Drink)
            return;

        RaiseAsync(new PartyMemberPromptEvent((TextId)Base.SystemText.InvMsg_WhoShouldDrinkThis), result =>
        {
            if (result == PartyMemberId.None)
                return;

            Raise(new SetContextEvent(ContextType.Subject, result));
            TriggerItemChain(slot.Item, () =>
            {
                slot.Amount--;
                Update(e.SlotId.Id);
            });
        });
    }

    void OnReadItem(ReadItemEvent e)
    {
        var inv = _getInventory(e.SlotId.Id);
        var slot = inv.GetSlot(e.SlotId.Slot);
        if (slot.Item.Type != AssetType.Item)
            return;

        var item = _getItem(slot.Item);
        if (item.TypeId != ItemType.Document)
            return;

        TriggerItemChain(item.Id, () => { });
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

        var spell = Resolve<IAssetManager>().LoadSpell(item.Spell);
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

    void TriggerItemChain(ItemId itemId, Action continuation)
    {
        var eventSet = Resolve<IAssetManager>().LoadEventSet(Base.EventSet.InventoryItems);
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

            RaiseAsync(triggerEvent, continuation);
            return;
        }

        // If no chain was found just continue immediately
        continuation.Invoke();
    }
}
#pragma warning restore CA1506 // 'InventoryManager' is coupled with '111' different types from '23' different namespaces. Rewrite or refactor the code to decrease its class coupling below '96'.
