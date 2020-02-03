using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Gui.Inventory
{
    abstract class InventorySlot : UiElement
    {
        const string TimerName = "InventorySlot.ClickTimer";
        protected static readonly HandlerSet SlotHandlers = new HandlerSet(
            H<InventorySlot, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventorySlot, UiBlurEvent>((x, _) =>
            {
                x.Frame.State = ButtonState.Normal;
                x.Raise(new HoverTextEvent(""));
            }),
            H<InventorySlot, UiLeftClickEvent>((x, _) => x.OnClick()),
            H<InventorySlot, TimerElapsedEvent>((x, e) => { if (e.Id == TimerName) x.OnTimer(); })
        );

        protected abstract ItemSlotId SlotId { get; }
        protected abstract ButtonFrame Frame { get; }
        protected PartyCharacterId ActiveCharacter { get; }
        bool _isClickTimerPending;

        protected InventorySlot(PartyCharacterId activeCharacter, IDictionary<Type, Handler> handlers)
            : base(handlers)
        {
            ActiveCharacter = activeCharacter;
        }

        protected void GetSlot(out ItemSlot slotInfo, out ItemData item)
        {
            var assets = Resolve<IAssetManager>();
            var member = Resolve<IParty>()[ActiveCharacter];
            slotInfo = member?.Apparent.Inventory.GetSlot(SlotId);
            item = slotInfo == null ? null : assets.LoadItem(slotInfo.Id);
        }

        void OnClick()
        {
            if (_isClickTimerPending) // If they double-clicked...
            {
                Raise(new InventoryPickupItemEvent(ActiveCharacter, SlotId, null));
                _isClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
            }
            else // For the first click, just start the double-click timer.
            {
                // TODO: If single item, fire the pickup event immediately
                Raise(new StartTimerEvent(TimerName, 300, this));
                _isClickTimerPending = true;
            }
        }
        void OnTimer()
        {
            if (!_isClickTimerPending) // They've already double-clicked
                return;

            // TODO: Show quantity selection dialog
            Raise(new InventoryPickupItemEvent(ActiveCharacter, SlotId, 1));
            _isClickTimerPending = false;
        }

        DynamicText BuildHoverText(SystemTextId template, params object[] arguments) =>
            new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var textFormatter = new TextFormatter(assets, settings.Gameplay.Language);
                return textFormatter.Format(template, arguments).Item1;
            });

        void Hover()
        {
            var state = Resolve<IInventoryScreenState>();
            var party = Resolve<IParty>();
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();

            var hand = state.ItemInHand;
            if (hand is GoldInHand || hand is RationsInHand)
                return; // Don't show hover text when holding gold / food

            var member = party[ActiveCharacter];
            if (member == null)
                return;

            var slotInfo = member.Effective.Inventory.GetSlot(SlotId);
            string itemName = null;
            if (slotInfo != null)
            {
                var item = assets.LoadItem(slotInfo.Id);
                if (item != null)
                    itemName = item.GetName(settings.Gameplay.Language);
            }

            string itemInHandName = null;
            if (hand is ItemSlot handSlot)
            {
                var itemInHand = assets.LoadItem(handSlot.Id);
                itemInHandName = itemInHand.GetName(settings.Gameplay.Language);
            }

            var action = member.GetInventoryAction(SlotId);
            switch(action)
            {
                case InventoryAction.Pickup:
                {   
                    // <Item name>
                    if (itemName != null)
                    {
                        Raise(new HoverTextEvent(itemName));
                        Frame.State = ButtonState.Hover;
                    }
                    break;
                }
                case InventoryAction.Drop:
                {
                    if (itemInHandName != null)
                    {
                        // Put down %s
                        Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_PutDownX, itemInHandName)));
                        Frame.State = ButtonState.Hover;
                    }

                    break;
                }
                case InventoryAction.Swap:
                {
                    if (itemInHandName != null && itemName != null)
                    {
                        // Swap %s with %s
                        Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_SwapXWithX, itemInHandName,
                            itemName)));
                        Frame.State = ButtonState.Hover;
                    }

                    break;
                }
                case InventoryAction.Coalesce:
                {   
                    // Add
                    Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_Add)));
                    Frame.State = ButtonState.Hover;
                    break;
                }
                case InventoryAction.NoCoalesceFullStack:
                {   
                    // {YELLOW}This space is occupied!
                    Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_ThisSpaceIsOccupied)));
                    Frame.State = ButtonState.Hover;
                    break;
                }
            }
        }
    }
}