using System;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryMidPane : UiElement
{
    readonly PartyMemberId _activeCharacter;
    public InventoryMidPane(PartyMemberId activeCharacter) => _activeCharacter = activeCharacter;

    protected override void Subscribed()
    {
        var assets = Resolve<IAssetManager>();
        var positions = assets.LoadPartyMember(_activeCharacter).InventorySlots;
        if (positions == null)
            throw new AssetNotFoundException($"Could not load inventory slot positions for party member {_activeCharacter}");

        var backgroundStack = new FixedPositionStack();
        var background = new UiSpriteElement(_activeCharacter.ToInventoryGfx());
        backgroundStack.Add(background, 3, 10 - 1); //subtract 1px because picture starts 1px above frame

        var bodyStack = new FixedPositionStack();
        foreach (var bodyPart in positions)
        {
            var itemSlotId = bodyPart.Key;
            var position = bodyPart.Value;
            bodyStack.Add(
                new LogicalInventorySlot(new InventorySlotId(
                    _activeCharacter,
                    itemSlotId)),
                (int)position.X + 1, //take frame border into account
                (int)position.Y + 1); //take frame border into account
        }

        bodyStack.Add(new Button(new Spacing(128, 168)) { Theme = ButtonTheme.Invisible, Margin = 0, Padding = -1 }
            .OnClick(() => Raise(new InventorySwapEvent(new InventoryId(_activeCharacter), ItemSlotId.CharacterBody))), 0, 0);

        var frame = new GroupingFrame(bodyStack) { Theme = GroupingFrame.FrameThemeBackgroundless, Padding = -1 };

        var labelStack = new HorizontalStack(
            new InventoryOffensiveLabel(_activeCharacter),
            new Spacing(4, 0),
            new InventoryWeightLabel(_activeCharacter),
            new Spacing(4, 0),
            new InventoryDefensiveLabel(_activeCharacter)
        );

        var mainStack = new VerticalStack(
            new Spacing(0, 1),
            new Header(new DynamicText(() =>
            {
                var member = Resolve<IParty>()[_activeCharacter];
                if (member == null)
                    return Array.Empty<TextBlock>();

                var name = member.Apparent.GetName(Var(UserVars.Gameplay.Language));
                return new[] { new TextBlock(name) { Alignment = TextAlignment.Center } };
            }), 18),
            new HorizontalStack(
                new Spacing(3, 0),
                frame,
                new Spacing(3, 0)),
            new Spacing(0, 2),
            labelStack
        );

        AttachChild(new LayerStack(backgroundStack, mainStack));
    }
}