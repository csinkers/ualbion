using System;
using System.Diagnostics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State.Player
{
    public class PlayerInventoryManager : Component
    {
        const int MaxSlotAmount = 99;
        const int TransitionSpeedMilliseconds = 200;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<PlayerInventoryManager, InventoryChangedEvent>((x, e) => { if (e.MemberId == x._id) x.Update(); }),
            H<PlayerInventoryManager, InventoryPickupItemEvent>((x,e) => x.OnPickupItem(e)),
            H<PlayerInventoryManager, InventoryGiveItemEvent>((x,e) => x.OnGiveItem(e)),
            H<PlayerInventoryManager, EngineUpdateEvent>((x,e) =>
            {
                var elapsed = (DateTime.Now - x._lastChangeTime).TotalMilliseconds;
                var oldLerp = x._lerp;
                x._lerp = elapsed > TransitionSpeedMilliseconds ? 1.0f : (float)(elapsed / TransitionSpeedMilliseconds);
                if(x._lerp != oldLerp)
                    x.Raise(new InventoryChangedEvent(x._id));
            })
        );

        readonly PartyCharacterId _id;
        readonly CharacterSheet _base;
        readonly InventoryScreenState _inventoryScreenState;
        IEffectiveCharacterSheet _lastEffective;
        IEffectiveCharacterSheet _effective;
        DateTime _lastChangeTime;
        float _lerp;

        public IEffectiveCharacterSheet Effective => _effective;
        public IEffectiveCharacterSheet Apparent { get; }

        public PlayerInventoryManager(PartyCharacterId id, CharacterSheet sheet, InventoryScreenState inventoryScreenState) : base(Handlers)
        {
            _id = id;
            _base = sheet;
            _inventoryScreenState = inventoryScreenState;
            Apparent = new InterpolatedCharacterSheet(() => _lastEffective, () => _effective, () => _lerp);
        }

        public InventoryAction GetInventoryAction(ItemSlotId slotId)
        {
            if (slotId == ItemSlotId.None)
                return InventoryAction.Nothing;

            var itemInHand = _inventoryScreenState.ItemInHand;
            if (itemInHand == null)
            {
                if (slotId == ItemSlotId.Gold || slotId == ItemSlotId.Rations)
                    return InventoryAction.Pickup;

                var contents = _base.Inventory.GetSlot(slotId);
                if (contents == null)
                    return InventoryAction.Nothing;

                return InventoryAction.Pickup;
            }
            else
            {
                if (itemInHand is GoldInHand || itemInHand is RationsInHand)
                    return InventoryAction.Drop;

                if (!(itemInHand is ItemSlot))
                    throw new InvalidOperationException($"Unexpected item in hand of type: {_inventoryScreenState.ItemInHand.GetType()}");

                var contents = _base.Inventory.GetSlot(slotId);
                if (contents == null)
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

        protected override void Subscribed() => Update();

        void Update()
        {
            var assets = Resolve<IAssetManager>();
            _lastEffective = _effective;
            _effective = EffectiveSheetCalculator.GetEffectiveSheet(assets, _base);
            _lastEffective ??= _effective;
            _lastChangeTime = DateTime.Now;
            _lerp = 0.0f;
            Raise(new InventoryChangedEvent(_id));
            Raise(new SetCursorEvent(_inventoryScreenState.ItemInHand == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
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
            if (_base.Inventory.RightHand != null)
            {
                var assets = Resolve<IAssetManager>();
                rightHandItem = assets.LoadItem(_base.Inventory.RightHand.Id);
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

        bool DoesSlotAcceptItemInHand(ItemSlotId slotId)
        {
            if (_inventoryScreenState.ItemInHand == null)
                return true;

            if(_inventoryScreenState.ItemInHand is GoldInHand)
                return slotId == ItemSlotId.Gold;

            if(_inventoryScreenState.ItemInHand is RationsInHand)
                return slotId == ItemSlotId.Rations;

            if(_inventoryScreenState.ItemInHand is ItemSlot itemInHand)
            {
                if (slotId >= ItemSlotId.Slot0)
                    return true;

                var assets = Resolve<IAssetManager>();
                var item = assets.LoadItem(itemInHand.Id);
                return DoesSlotAcceptItem(slotId, item);
            }

            throw new InvalidOperationException($"Unexpected item type in hand: {_inventoryScreenState.ItemInHand.GetType()}");
        }

        ItemSlotId GetBestSlot(ItemSlotId slotId)
        {
            if (!(_inventoryScreenState.ItemInHand is ItemSlot itemInHand)) // gold, rations etc
                return slotId;

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(itemInHand.Id);
            if (slotId < ItemSlotId.Slot0)
            {
                if (DoesSlotAcceptItem(slotId, item)) return slotId;
                if (DoesSlotAcceptItem(ItemSlotId.Head, item)) return ItemSlotId.Head;
                if (DoesSlotAcceptItem(ItemSlotId.Neck, item)) return ItemSlotId.Neck;
                if (DoesSlotAcceptItem(ItemSlotId.Tail, item)) return ItemSlotId.Tail;
                if (DoesSlotAcceptItem(ItemSlotId.RightHand, item)) return ItemSlotId.RightHand;
                if (DoesSlotAcceptItem(ItemSlotId.LeftHand, item)) return ItemSlotId.LeftHand;
                if (DoesSlotAcceptItem(ItemSlotId.Torso, item)) return ItemSlotId.Torso;
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

            // var assets = Resolve<IAssetManager>();
            var itemSlot = _base.Inventory.GetSlot(slotId);
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
            if (e.MemberId != _id)
                return;

            if (_inventoryScreenState.ItemInHand is GoldInHand)
                Drop(ItemSlotId.Gold);
            if (_inventoryScreenState.ItemInHand is RationsInHand)
                Drop(ItemSlotId.Rations);
            if (!(_inventoryScreenState.ItemInHand is ItemSlot itemInHand))
                return; // Unknown or null

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(itemInHand.Id);

            ItemSlotId slotId = ItemSlotId.None;
            if (item.IsStackable)
            {
                for (int i = 0; i < _base.Inventory.Slots.Length; i++)
                {
                    if (_base.Inventory.Slots[i] == null)
                        continue;

                    if (_base.Inventory.Slots[i].Id == item.Id)
                        slotId = (ItemSlotId)((int)ItemSlotId.Slot0 + i);
                }
            }

            for (int i = 0; i < _base.Inventory.Slots.Length; i++)
            {
                if (_base.Inventory.Slots[i] == null)
                    slotId = (ItemSlotId)((int)ItemSlotId.Slot0 + i);
            }

            if(slotId != ItemSlotId.None)
                Drop(slotId);

            Update();
        }

        void OnPickupItem(InventoryPickupItemEvent e)
        {
            if (e.MemberId != _id)
                return;

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

            var assets = Resolve<IAssetManager>();
            var item = assets.LoadItem(slot.Id);
            return item.IsStackable;
        }

        void PickupItem(ItemSlotId slotId, int? quantity)
        {
            Debug.Assert(_inventoryScreenState.ItemInHand == null);

            // Check if the item can be taken
            if (!CanItemBeTaken(slotId))
                return; // TODO: Message

            if (slotId == ItemSlotId.Gold)
            {
                ushort amount = (ushort)Math.Min(_base.Inventory.Gold, quantity ?? _base.Inventory.Gold);
                if (amount == 0)
                    return;

                _inventoryScreenState.ReturnItemInHandEvent = new InventoryPickupItemEvent(_id, slotId);
                _inventoryScreenState.ItemInHand = new GoldInHand { Amount = amount };
            }
            else if (slotId == ItemSlotId.Rations)
            {
                ushort amount = (ushort)Math.Min(_base.Inventory.Rations, quantity ?? _base.Inventory.Rations);
                if (amount == 0)
                    return;

                _inventoryScreenState.ReturnItemInHandEvent = new InventoryPickupItemEvent(_id, slotId);
                _inventoryScreenState.ItemInHand = new RationsInHand { Amount = amount };
            }
            else
            {
                var slot = _base.Inventory.GetSlot(slotId);
                byte amount = (byte)Math.Min(slot.Amount, quantity ?? slot.Amount);
                _inventoryScreenState.ReturnItemInHandEvent = new InventoryPickupItemEvent(_id, slotId);
                _inventoryScreenState.ItemInHand = new ItemSlot
                {
                    Id = slot.Id,
                    Amount = amount,
                    Charges = slot.Charges,
                    Enchantment = slot.Enchantment,
                    Flags = slot.Flags
                };

                slot.Amount -= amount;
                if(slot.Amount == 0)
                    _base.Inventory.SetSlot(slotId, null);
            }
        }

        void CoalesceItems(ItemSlotId slotId)
        {
            var slot = _base.Inventory.GetSlot(slotId);
            var itemInHand = (ItemSlot)_inventoryScreenState.ItemInHand;
            Debug.Assert(slot.Id == itemInHand.Id);
            Debug.Assert(CanCoalesce(slotId, slot, itemInHand));
            Debug.Assert(slot.Amount < MaxSlotAmount);

            byte amountToMove = (byte)Math.Min(itemInHand.Amount, MaxSlotAmount - slot.Amount);
            itemInHand.Amount -= amountToMove;
            slot.Amount += amountToMove;

            if (itemInHand.Amount == 0)
            {
                _inventoryScreenState.ItemInHand = null;
                _inventoryScreenState.ReturnItemInHandEvent = null;
            }
        }

        void SwapItems(ItemSlotId slotId)
        {
            var slot = _base.Inventory.GetSlot(slotId);
            var itemInHand = (ItemSlot)_inventoryScreenState.ItemInHand;
            Debug.Assert(slot.Id != itemInHand.Id);

            // Check if the item can be taken
            if (!CanItemBeTaken(slotId))
                return; // TODO: Message

            _inventoryScreenState.ItemInHand = slot;
            _base.Inventory.SetSlot(slotId, itemInHand);
        }

        void Drop(ItemSlotId slotId)
        {
            Debug.Assert(_base.Inventory.GetSlot(slotId) == null);

            if (_inventoryScreenState.ItemInHand is GoldInHand gold)
            {
                _base.Inventory.Gold += gold.Amount;
                _inventoryScreenState.ItemInHand = null;
                _inventoryScreenState.ReturnItemInHandEvent = null;
            }

            if (_inventoryScreenState.ItemInHand is RationsInHand rations)
            {
                _base.Inventory.Rations += rations.Amount;
                _inventoryScreenState.ItemInHand = null;
                _inventoryScreenState.ReturnItemInHandEvent = null;
            }

            var itemInHand = (ItemSlot)_inventoryScreenState.ItemInHand;
            if (slotId >= ItemSlotId.Slot0)
            {
                _base.Inventory.Slots[slotId - ItemSlotId.Slot0] = itemInHand;
            }
            else // Body slot
            {
                _base.Inventory.SetSlot(slotId, itemInHand);
            }

            _inventoryScreenState.ItemInHand = null;
            _inventoryScreenState.ReturnItemInHandEvent = null;
        }

        void RaiseStatusMessage(SystemTextId textId)
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var text = assets.LoadString(textId, settings.Language);
            Raise(new DescriptionTextEvent(text));
        }
    }
}