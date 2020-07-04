using System;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryScreen : Dialog
    {
        readonly InventoryConfig _config;
        InventoryMode _mode;
        int _modeSpecificId;
        InventoryPage _page;
        PartyCharacterId _activeCharacter;

        public InventoryScreen(InventoryConfig config) : base(DialogPositioning.TopLeft)
        {
            On<InventoryOpenEvent>(SetMode);
            On<ChestEvent>(SetMode);
            On<DoorEvent>(SetMode);
            On<MerchantEvent>(SetMode);

            _config = config;
        }

        protected override void Subscribed()
        {
            Rebuild();
        }

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

            var party = Resolve<IParty>();
            _activeCharacter = e.Member ?? party.Leader;
            if (party.WalkOrder.All(x => x.Id != _activeCharacter))
                _activeCharacter = party.Leader;

            Rebuild();
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

            var middlePane = new InventoryMidPane(_activeCharacter, _config.Positions[_activeCharacter]);
            var rightPane = new InventoryRightPane(
                _activeCharacter,
                () => Raise(new PopSceneEvent()),
                _mode == InventoryMode.Merchant);

            // var frameDivider = new FrameDivider(135, 0, 4, 192);

            AttachChild(new UiFixedPositionElement<SlabId>(SlabId.SLAB, UiConstants.UiExtents));
            AttachChild(new FixedPosition(new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane));
            AttachChild(new FixedPosition(new Rectangle(142, 0, 134, UiConstants.ActiveAreaExtents.Height), middlePane));
            AttachChild(new FixedPosition(new Rectangle(280, 0, 71, UiConstants.ActiveAreaExtents.Height), rightPane));
        }
    }
}
