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
            var bodyStack = new FixedPositionStack();
            bodyStack.Add(new UiSprite<FullBodyPictureId>((FullBodyPictureId)activeCharacter), 0, 0);
            foreach (var bodyPart in config)
            {
                var itemSlotId = bodyPart.Key;
                var position = bodyPart.Value;
                bodyStack.Add(new InventoryBodyPart(itemSlotId), (int)position.X, (int)position.Y);
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
                        var state = Resolve<IStateManager>();
                        var settings = Resolve<ISettings>();
                        var member = state.State.GetPartyMember(activeCharacter);
                        var name = member.GetName(settings.Language);
                        return new[] { new TextBlock(name) { Alignment = TextAlignment.Center } };
                    })),
                frame,
                labelStack
                );

            Children.Add(mainStack);
        }
    }
}