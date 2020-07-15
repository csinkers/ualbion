using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryOffensiveLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        int _version;

        public InventoryOffensiveLabel(PartyCharacterId activeCharacter)
        {
            On<InventoryChangedEvent>(e => _version++);
            On<BlurEvent>(e => Raise(new HoverTextEvent(null)));
            On<HoverEvent>(Hover);

            _activeCharacter = activeCharacter;
            var source = new DynamicText(() =>
            {
                var player = Resolve<IParty>()[_activeCharacter];
                var damage = player?.Apparent.DisplayDamage ?? 0;
                return new[] { new TextBlock($": {damage}") };
            }, x => _version);

            AttachChild(
                new ButtonFrame(
                        new HorizontalStack(
                            new FixedSize(8, 8,
                                new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiOffensiveValue)),
                            new Spacing(1, 0),
                            new UiText(source)
                        )
                    )
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
            var text = tf.Format(SystemTextId.Inv_DamageN, damage); // Damage : %d
            Raise(new HoverTextEvent(text));
            e.Propagating = false;
        }
    }
}
