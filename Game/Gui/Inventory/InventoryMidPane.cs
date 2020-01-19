using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryMidPane : UiElement
    {
        public InventoryMidPane(PartyCharacterId activeCharacter,  InventoryConfig.PlayerInventory config)
        {
            var background = new FixedPositionStack();
            background.Add(new UiSprite<FullBodyPictureId>((FullBodyPictureId)activeCharacter), 1, -3);
            Children.Add(background);

            var bodyStack = new FixedPositionStack();
            foreach (var bodyPart in config)
            {
                var itemSlotId = bodyPart.Key;
                var position = bodyPart.Value;
                bodyStack.Add(new InventoryBodyPart(activeCharacter, itemSlotId), (int)position.X, (int)position.Y);
            }

            var frame = new ButtonFrame(bodyStack) { Theme = new FrameTheme() };
            var labelStack = new HorizontalStack(
                new InventoryOffensiveLabel(activeCharacter),
                new InventoryWeightLabel(activeCharacter),
                new InventoryDefensiveLabel(activeCharacter)
            );

            var mainStack = new VerticalStack(
                new Header(new DynamicText(() =>
                    {
                        var member = Resolve<IParty>()[activeCharacter];
                        var settings = Resolve<ISettings>();
                        var name = member.Apparent.GetName(settings.Gameplay.Language);
                        return new[] { new TextBlock(name) { Alignment = TextAlignment.Center } };
                    })),
                new HorizontalStack(
                    new Padding(3, 0),
                    frame,
                    new Padding(3, 0)),
                labelStack
                );

            Children.Add(mainStack);
        }
    }
}