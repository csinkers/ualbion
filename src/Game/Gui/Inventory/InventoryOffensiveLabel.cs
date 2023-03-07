using UAlbion.Core.Events;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryOffensiveLabel : UiElement
{
    readonly PartyMemberId _activeCharacter;
    int _version = 1;

    public InventoryOffensiveLabel(PartyMemberId activeCharacter)
    {
        On<InventoryChangedEvent>(_ => _version++);
        On<BlurEvent>(_ => Raise(new HoverTextEvent(null)));
        On<HoverEvent>(Hover);

        _activeCharacter = activeCharacter;
        var source = new DynamicText(() =>
        {
            var player = Resolve<IParty>()[_activeCharacter];
            var damage = player?.Apparent.DisplayDamage ?? 0;
            return new[] { new TextBlock($"{damage}") };
        }, _ => _version);

        AttachChild(
            new ButtonFrame(
                new FixedSize(27, 8,
                    new HorizontalStacker(
                        new FixedSize(8, 8,
                            new UiSpriteElement(Base.CoreGfx.UiOffensiveValue)),
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
        var damage = player?.Apparent.DisplayDamage ?? 0;
        var text = tf.Format(Base.SystemText.Inv_DamageN, damage); // Damage : %d
        Raise(new HoverTextEvent(text));
        e.Propagating = false;
    }
}