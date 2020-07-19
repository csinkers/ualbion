using System;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryScreen : Dialog
    {
        InventoryMode _mode = InventoryMode.Character;
        int _modeSpecificId;
        InventoryPage _page;
        PartyCharacterId _activeCharacter;

        public InventoryScreen() : base(DialogPositioning.TopLeft)
        {
            On<InventoryOpenEvent>(e => SetDisplayedPartyMember(e.Member));
            On<ChestEvent>(SetMode);
            On<DoorEvent>(SetMode);
            On<MerchantEvent>(SetMode);
        }

        void SetDisplayedPartyMember(PartyCharacterId? member)
        {
            var party = Resolve<IParty>();
            _activeCharacter = member ?? party.Leader;
            if (party.WalkOrder.All(x => x.Id != _activeCharacter))
                _activeCharacter = party.Leader;

            Rebuild();
        }

        protected override void Subscribed() => Rebuild();

        void SetMode(ISetInventoryModeEvent e)
        {
            _mode = e.Mode;
            _modeSpecificId = e switch
            {
                MerchantEvent merchant => (int)merchant.MerchantId,
                ChestEvent chest => (int)chest.ChestId,
                DoorEvent door => door.DoorId,
                _ => 0
            };

            SetDisplayedPartyMember(e.Member);
        }

        void Rebuild()
        {
            RemoveAllChildren();

            var leftPane =
                _mode switch
                {
                    InventoryMode.Character => (IUiElement)new InventoryCharacterPane(
                        _activeCharacter,
                        () => _page,
                        x => _page = x),

                    // InventoryMode.Merchant => new InventoryMerchantPane((MerchantId)_modeSpecificId),
                    InventoryMode.Chest => new InventoryChestPane((ChestId)_modeSpecificId),
                    InventoryMode.LockedChest => new InventoryLockPane(true),
                    InventoryMode.LockedDoor => new InventoryLockPane(false),
                    _ => throw new InvalidOperationException($"Unexpected inventory mode {_mode}")
                };

            var middlePane = new InventoryMidPane(_activeCharacter);
            var rightPane = new InventoryRightPane(
                _activeCharacter,
                () =>
                {
                    _mode = InventoryMode.Character;
                    _modeSpecificId = 0;
                    Raise(new PopSceneEvent());
                },
                _mode == InventoryMode.Merchant);

            // var frameDivider = new FrameDivider(135, 0, 4, 192);

            AttachChild(new UiFixedPositionElement<SlabId>(SlabId.SLAB, UiConstants.UiExtents));
            AttachChild(new FixedPosition(new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane));
            AttachChild(new FixedPosition(new Rectangle(142, 0, 134, UiConstants.ActiveAreaExtents.Height), middlePane));
            AttachChild(new FixedPosition(new Rectangle(280, 0, 71, UiConstants.ActiveAreaExtents.Height), rightPane));
        }
    }
}
