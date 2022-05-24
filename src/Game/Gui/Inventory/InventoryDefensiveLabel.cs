using UAlbion.Core.Events;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryDefensiveLabel : UiElement
{
    readonly PartyMemberId _activeCharacter;
    int _version;

    public InventoryDefensiveLabel(PartyMemberId activeCharacter)
    {
        On<InventoryChangedEvent>(e => _version++);
        On<BlurEvent>(e => Raise(new HoverTextEvent(null)));
        On<HoverEvent>(Hover);

        _activeCharacter = activeCharacter;

        var source = new DynamicText(() =>
        {
            var player = Resolve<IParty>()[_activeCharacter];
            var protection = player?.Apparent.DisplayProtection ?? 0;
            return new[] { new TextBlock($"{protection}") };
        }, x => _version);

        AttachChild(
            new ButtonFrame(
                new FixedSize(27, 8,
                    new HorizontalStack(
                        new FixedSize(6, 8,
                            new UiSpriteElement(Base.CoreGfx.UiDefensiveValue)),
                        new Spacing(1, 0),
                        new UiText(new LiteralText(":")),
                        new UiText(source)
                    )
                ))
            {
                State = ButtonState.Pressed,
                Padding = 0
            }
        );
    }

    void Hover(HoverEvent e)
    {
        var player = Resolve<IParty>()[_activeCharacter];
        var tf = Resolve<ITextFormatter>();

        // Protection : %d
        var protection = player?.Apparent.DisplayProtection ?? 0;
        var text = tf.Format(Base.SystemText.Inv_ProtectionN, protection);
        Raise(new HoverTextEvent(text));
        e.Propagating = false;
    }
}