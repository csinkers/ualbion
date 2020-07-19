using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryMidPane : UiElement
    {
        public InventoryMidPane(PartyCharacterId activeCharacter,  InventoryConfig.PlayerInventory config)
        {
            var backgroundStack = new FixedPositionStack();
            var background = new UiSpriteElement<FullBodyPictureId>((FullBodyPictureId)activeCharacter);
            var backgroundButton = new Button(background) { Theme = ButtonTheme.Invisible }
                .OnClick(() => Raise(new InventorySwapEvent(InventoryType.Player, (ushort)activeCharacter, ItemSlotId.CharacterBody)));
            backgroundStack.Add(backgroundButton, 1, -3);
            AttachChild(backgroundStack);

            var bodyStack = new FixedPositionStack();
            foreach (var bodyPart in config)
            {
                var itemSlotId = bodyPart.Key;
                var position = bodyPart.Value;
                bodyStack.Add(
                    new LogicalInventorySlot(new InventorySlotId(
                        InventoryType.Player,
                        (ushort)activeCharacter,
                        itemSlotId)),
                    (int)position.X,
                    (int)position.Y);
            }
            bodyStack.Add(new Spacing(0, 164), 0, 0);

            var frame = new GroupingFrame(bodyStack);
            var labelStack = new HorizontalStack(
                new InventoryOffensiveLabel(activeCharacter),
                new Spacing(4, 0),
                new InventoryWeightLabel(activeCharacter),
                new Spacing(4, 0),
                new InventoryDefensiveLabel(activeCharacter)
            );

            var mainStack = new VerticalStack(
                new Spacing(0, 1),
                new Header(new DynamicText(() =>
                    {
                        var member = Resolve<IParty>()[activeCharacter];
                        var settings = Resolve<ISettings>();
                        if (member == null)
                            return new TextBlock[0];

                        var name = member.Apparent.GetName(settings.Gameplay.Language);
                        return new[] { new TextBlock(name) { Alignment = TextAlignment.Center } };
                    })),
                new HorizontalStack(
                    new Spacing(3, 0),
                    frame,
                    new Spacing(3, 0)),
                new Spacing(0, 2),
                labelStack
                );

            AttachChild(mainStack);
        }
    }
}
