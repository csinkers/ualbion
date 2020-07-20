using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryScreen : Dialog
    {
        public InventoryScreen(
            ISetInventoryModeEvent modeEvent,
            PartyCharacterId activeCharacter,
            Func<InventoryPage> getPage,
            Action<InventoryPage> setPage) : base(DialogPositioning.TopLeft)
        {
            var leftPane =
                modeEvent.Mode switch
                {
                    InventoryMode.Character => (IUiElement)new InventoryCharacterPane(activeCharacter, getPage, setPage),
                    InventoryMode.Merchant => new InventoryMerchantPane((MerchantId)modeEvent.Submode),
                    InventoryMode.Chest => new InventoryChestPane((ChestId)modeEvent.Submode),
                    InventoryMode.LockedChest => new InventoryLockPane((ILockedInventoryEvent)modeEvent),
                    InventoryMode.LockedDoor => new InventoryLockPane((ILockedInventoryEvent)modeEvent),
                    _ => throw new InvalidOperationException($"Unexpected inventory mode {modeEvent.Mode}")
                };

            var middlePane = new InventoryMidPane(activeCharacter);
            var rightPane = new InventoryRightPane(
                activeCharacter,
                modeEvent.Mode == InventoryMode.Merchant);

            // var frameDivider = new FrameDivider(135, 0, 4, 192);

            AttachChild(new UiFixedPositionElement<SlabId>(SlabId.SLAB, UiConstants.UiExtents));
            AttachChild(new FixedPosition(new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane));
            AttachChild(new FixedPosition(new Rectangle(142, 0, 134, UiConstants.ActiveAreaExtents.Height), middlePane));
            AttachChild(new FixedPosition(new Rectangle(280, 0, 71, UiConstants.ActiveAreaExtents.Height), rightPane));
        }
    }
}
