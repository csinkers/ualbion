using System;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryScreen : Dialog
{
    public InventoryScreen(
        IEvent modeEvent,
        PartyMemberId activeCharacter,
        Func<InventoryPage> getPage,
        Action<InventoryPage> setPage) : base(DialogPositioning.TopLeft)
    {
        if (modeEvent == null) throw new ArgumentNullException(nameof(modeEvent));
        if (getPage == null) throw new ArgumentNullException(nameof(getPage));
        if (setPage == null) throw new ArgumentNullException(nameof(setPage));

        var leftPane =
            modeEvent switch
            {
                InventoryOpenEvent ioe => new InventoryCharacterPane(activeCharacter, getPage, setPage),
                MerchantEvent me => new InventoryMerchantPane(me.MerchantId),
                ChestEvent ce => ce.PickDifficulty == 0 ? (IUiElement)new InventoryChestPane(ce.ChestId) : new InventoryLockPane(ce),
                DoorEvent de => new InventoryLockPane(de),
                _ => throw new InvalidOperationException($"Unexpected inventory mode event {modeEvent}")
            };

        var middlePane = new InventoryMidPane(activeCharacter);
        var rightPane = new InventoryRightPane(activeCharacter, modeEvent is MerchantEvent);

        // var frameDivider = new FrameDivider(135, 0, 4, 192);

        AttachChild(new UiFixedPositionElement(Base.UiBackground.Slab, UiConstants.UiExtents));
        AttachChild(new FixedPosition(new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane));
        AttachChild(new FixedPosition(new Rectangle(142, 0, 134, UiConstants.ActiveAreaExtents.Height), middlePane));
        AttachChild(new FixedPosition(new Rectangle(280, 0, 71, UiConstants.ActiveAreaExtents.Height), rightPane));
    }
}